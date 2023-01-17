using Facepunch.Boomer.UI;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facepunch.Boomer.Gamemodes;

public partial class TeamDeathmatch : Gamemode
{
	/// <summary>
	/// The current game state.
	/// </summary>
	[Net] public GameState CurrentState { get; set; }
	[Net] public TimeUntil TimeUntilNextState { get; set; }

	[Net] public IDictionary<Team, int> Scores { get; set; }

	public int MaxScore => 75;

	/// <summary>
	/// How long the countdown is after we've got enough players in-game.
	/// </summary>
	protected float GameCountdown => 10f;

	/// <summary>
	/// How long the game lasts, after that we'll decide the winner.
	/// </summary>
	protected float GameDuration => 300f;

	/// <summary>
	/// Team setup for TDM. Default 2 teams.
	/// </summary>
	public override IEnumerable<Team> Teams 
	{
		get
		{
			yield return Team.Red;
			yield return Team.Blue;
		}
	}

	internal override Panel HudPanel => new TDMHud();

	protected void ResetScores()
	{
		Scores = null;
		foreach ( var team in Teams )
		{
			Scores.Add( team, 0 );
		}
	}

	internal override void Initialize()
	{
		_ = GameLoop();

		ResetScores();
	}

	internal override void OnClientJoined( IClient cl )
	{
		base.OnClientJoined( cl );

		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		if ( teamComponent != null )
		{
			teamComponent.Team = TeamSystem.GetLowestCount();
		}
	}

	TimeSpan TimeRemaining => TimeSpan.FromSeconds( TimeUntilNextState );
	public override string GetTimeLeftLabel()
	{
		return TimeRemaining.ToString( @"mm\:ss" );
	}

	public override string GetGameStateLabel()
	{
		return CurrentState switch
		{
			GameState.Warmup => "Waiting for players",
			GameState.Countdown => "Game starting in",
			GameState.GameActive => "Get the most kills",
			GameState.GameOver => "Game over",
			_ => null
		};
	}

	protected void AddScore( Team team, int amount = 1 )
	{
		Scores[team] += amount;
	}

	public int GetScore( Team team )
	{
		return Scores[team];
	}

	internal override void PostPlayerKilled( Player player, DamageInfo lastDamage )
	{
		base.PostPlayerKilled( player, lastDamage );

		if ( lastDamage.Attacker is Player attacker )
		{
			var attackerTeam = TeamSystem.GetTeam( attacker.Client );
			AddScore( attackerTeam );
		}
	}

	private async Task WaitForPlayers()
	{
		while ( PlayerCount < MinimumPlayers )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		// We're ready to start the game
	}

	private async Task WaitAsync( float time )
	{
		TimeUntilNextState = time;
		await Task.DelayRealtimeSeconds( time );
	}

	/// <summary>
	/// The core game loop for Deathmatch.
	/// </summary>
	protected async Task GameLoop()
	{
		// Wait until we've got players in the game.
		CurrentState = GameState.Warmup;
		Log.Info( "Waiting for players." );
		await WaitForPlayers();

		CurrentState = GameState.Countdown;

		// Make sure all the players respawn.
		RespawnAllPlayers();

		Chat.AddInformation( To.Everyone, $"The game will start in {GameCountdown} seconds." );
		await WaitAsync( GameCountdown );

		// The game's now active.
		CurrentState = GameState.GameActive;
		Chat.AddInformation( To.Everyone, $"The game begins." );
		await WaitAsync( GameDuration );

		// The game's over.
		CurrentState = GameState.GameOver;
		Chat.AddInformation( To.Everyone, $"Game over." );

		// TODO - Decide Winner
		// TODO - Map Vote

		// Wait a certain amount of time. Replace this with Map Vote task.
		await WaitAsync( 20f );

		// For now, we'll just restart the current map.
		Game.ChangeLevel( Game.Server.MapIdent );
	}

	/// <summary>
	/// Only allow damage if we're in warmup / active game.
	/// </summary>
	public override bool AllowDamage => CurrentState == GameState.Warmup || CurrentState == GameState.GameActive;

	/// <summary>
	/// Disallow movement while we're counting down.
	/// </summary>
	public override bool AllowMovement => CurrentState != GameState.Countdown;

	/// <summary>
	/// Disallow hurting friendlies in TDM.
	/// </summary>
	public override bool AllowFriendlyFire => false;

	public enum GameState
	{
		Warmup,
		Countdown,
		GameActive,
		GameOver
	}
}
