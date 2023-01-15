using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Boomer;

public abstract partial class Gamemode : Entity
{
	/// <summary>
	/// A quick accessor to get how many people are in the game
	/// </summary>
	[Net] public int PlayerCount { get; private set; }

	/// <summary>
	/// Can specify a panel to be created when the gamemode is made.
	/// </summary>
	/// <returns></returns>
	internal virtual Panel HudPanel => null;

	/// <summary>
	/// Gamemodes can define what pawn to create
	/// </summary>
	/// <param name="cl"></param>
	/// <returns></returns>
	internal virtual Player GetPawn( IClient cl ) => new Player();

	/// <summary>
	/// Should we allow spectating?
	/// </summary>
	public virtual bool AllowSpectating => false;

	/// <summary>
	/// Decides whether or not players can move
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowMovement => true;

	/// <summary>
	/// Decides whether or not players can take damage
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowDamage => true;

	/// <summary>
	/// Specify the list of teams that are supported in this mode.
	/// </summary>
	public virtual IEnumerable<Team> Teams => null;

	// Stats
	[ConVar.Server( "boomer_minimum_players" )]
	protected static int MinimumPlayersConVar { get; set; } = 2;

	/// <summary>
	/// How many players should be in the game before it starts?
	/// </summary>
	public virtual int MinimumPlayers => MinimumPlayersConVar;

	/// <summary>
	/// Are capture points allowed to be used as spawn points?
	/// </summary>
	public virtual bool CapturePointsAreSpawnPoints => false;

	/// <summary>
	/// What's the max score for this game? Can be unused.
	/// </summary>
	public virtual int MaximumScore => 4;

	protected Player LastKilledPlayer;

	public virtual string GetTimeLeftLabel()
	{
		return "00:00";
	}

	public virtual string GetGameStateLabel()
	{
		return "N/A";
	}

	internal virtual void CreatePawn( IClient cl )
	{
		cl.Pawn?.Delete();
		var pawn = GetPawn( cl );
		cl.Pawn = pawn;
		pawn.Respawn();
	}

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	/// <summary>
	/// Called when a client joins the game
	/// </summary>
	/// <param name="cl"></param>
	internal virtual void OnClientJoined( IClient cl )
	{
		PlayerCount++;
	}

	/// <summary>
	/// Called when a client leaves the game
	/// </summary>
	/// <param name="cl"></param>
	/// <param name="reason"></param>
	internal virtual void OnClientDisconnected( IClient cl, NetworkDisconnectionReason reason )
	{
		PlayerCount--;
	}

	/// <summary>
	/// Used to apply a loadout to a player
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	internal virtual bool PlayerLoadout( Player player )
	{
		return false;
	}

	/// <summary>
	/// Called when a player dies.
	/// Gamemodes can define the life state a player will move to upon death. 
	/// <see cref="LifeState.Respawning"/> is the default behavior, which will automatically respawn the player in a few seconds.
	/// <see cref="LifeState.Dead"/> means the gamemode has to respawn the player.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="damageInfo"></param>
	/// <param name="lifeState"></param>
	internal virtual void OnPlayerKilled( Player player, DamageInfo damageInfo, out LifeState lifeState )
	{
		lifeState = LifeState.Respawning;
	}

	internal virtual void PostPlayerKilled( Player player, DamageInfo lastDamage )
	{
		LastKilledPlayer = player;
	}

	internal float GetSpawnpointWeight( Entity pawn, Entity spawnpoint )
	{
		// We want to find the closest player (worst weight)
		float distance = float.MaxValue;

		foreach ( var client in Game.Clients )
		{
			var clientPawn = client.Pawn as Player;

			if ( !clientPawn.IsValid() ) continue;
			if ( clientPawn == pawn ) continue;
			if ( clientPawn.LifeState != LifeState.Alive ) continue;

			var spawnDist = (spawnpoint.Position - client.Pawn.Position).Length;
			distance = MathF.Min( distance, spawnDist );
		}

		return distance;
	}

	internal IEnumerable<SpawnPoint> GetValidSpawnPoints( Player player )
	{
		// TODO - custom spawn point type?
		return Entity.All.OfType<SpawnPoint>();
	}

	/// <summary>
	/// Allows gamemodes to override player spawn locations
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	internal virtual Transform? GetDefaultSpawnPoint( Player player )
	{
		// Default behavior
		var spawnPoints = GetValidSpawnPoints( player );
		if ( spawnPoints.Count() < 1 ) return null;

		Log.Info( $"{spawnPoints.Count()} valid spawn points found." );

		return spawnPoints.FirstOrDefault()?.Transform ?? default;
	}

	internal virtual void PreSpawn( Player player )
	{
		//
	}

	internal virtual void RespawnAllPlayers()
	{
		All.OfType<Player>()
			.ToList()
			.ForEach( x => x.Respawn() );
	}

	/// <summary>
	/// Called on Client Tick, allows gamemodes to define custom post processing
	/// </summary>
	internal virtual void PostProcessTick()
	{
	}

	internal virtual void CleanupMap()
	{
	}

	public override void BuildInput()
	{
		if ( !AllowMovement )
		{
			Input.AnalogMove = Vector3.Zero;
			Input.ClearButton( InputButton.Jump );
			Input.ClearButton( InputButton.Duck );
			Input.ClearButton( InputButton.PrimaryAttack );
			Input.StopProcessing = true;
		}
	}

	[Event.Tick.Server]
	protected void EventServerTick()
	{
		TickServer();
	}

	protected virtual void TickServer()
	{
		//
	}

	internal virtual void Initialize()
	{
	}
}
