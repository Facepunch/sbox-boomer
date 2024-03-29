﻿global using Sandbox;
global using Editor;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;

using Boomer.Movement;
using Boomer.UI;

namespace Boomer;

/// <summary>
/// This is the heart of the gamemode. It's responsible for creating the player and stuff.
/// </summary>
partial class DeathmatchGame : GameManager
{
	public static new DeathmatchGame Current => GameManager.Current as DeathmatchGame;

	[Net] private DeathmatchHud Hud { get; set; }

	public static bool HasFirstPlayerDied { get; set; }

	[ConVar.Replicated( "bm_unlimitedammo" )]
	public static bool UnlimitedAmmo { get; set; } = false;

	[ConVar.Replicated( "bm_norocketselfdamage" )]
	public static bool NoRocketSelfDmg { get; set; } = false;

	[ConVar.Replicated( "bm_teams_friendly_fire" )]
	public static bool FriendlyFire { get; set; } = false;

	[Net] public TeamManager TeamManager { get; set; }

	// Accessor
	public static bool IsTeamPlayEnabled => Current?.TeamManager?.IsTeamPlayEnabled ?? false;

	public DeathmatchGame()
	{
		//
		// Create the HUD entity. This is always broadcast to all clients
		// and will create the UI panels clientside.
		//
		if ( Game.IsServer )
		{
			PrecacheAssets();

			Hud = new();
			TeamManager = new();

			_ = GameLoopAsync();
		}
	}

	private void PrecacheAssets()
	{
		var assets = FileSystem.Mounted.ReadJsonOrDefault<List<string>>( "resources/boomer.assets.json" );

		foreach ( var asset in assets )
		{
			Log.Info( $"Precaching: {asset}" );
			Precache.Add( asset );
		}
	}
	
	[Net]
	public string BackUpMap { get; set; } = "facepunch.bm_dockyard";

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
		ItemRespawn.Init();
	}

	[Event.Entity.PostSpawn]
	public static void PreMapCheck()
	{
		var startingweapons = All
		.OfType<StartingWeapons>()
				.FirstOrDefault();

		if ( startingweapons == null )
		{
			Log.Error( $"Map is not valid, changing to {Current.BackUpMap}. Map is missing the boomer_startingweapons entity, Changing Map in 1m." );
			Log.Info( "Map is not valid, changing to " + Current.BackUpMap );
			Log.Info( "Map is missing the boomer_startingweapons entity" );
			Log.Info( "Changing Map in 1m" );
			GameTime = 1;
		}
	}

	[ConCmd.Server]
	public static void MapCheck()
	{
		var startingweapons = All
		.OfType<StartingWeapons>()
				.FirstOrDefault();

		if ( startingweapons == null )
		{
			Game.ChangeLevel( Current.BackUpMap );
		}
	}


	public static bool ScoreSystemDisabled { get; set; } = false;

	public override void ClientJoined( IClient cl )
	{
		if ( cl.IsBot )
			ScoreSystemDisabled = true;

		Log.Info( $"\"{cl.Name}\" has joined the game" );
		BoomerChatBox.AddInformation( To.Everyone, $"{cl.Name} has joined", $"avatar:{cl.SteamId}" );

		TeamManager.OnClientJoined( cl );

		var player = new BoomerPlayer();
		player.RpcSetClothes( To.Single( cl ) );
		cl.Pawn = player;
		player.Respawn();
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		Log.Info( $"\"{cl.Name}\" has left the game ({reason})" );
		BoomerChatBox.AddInformation( To.Everyone, $"{cl.Name} has left ({reason})", $"avatar:{cl.SteamId}" );
		var player = cl.Pawn as BoomerPlayer;
		if ( cl.Pawn.IsValid() )
		{
			cl.Pawn.Delete();
			cl.Pawn = null;
			if ( player.TaggedPlayer )
			{
				StartTag();
			}
		}

		TeamManager.OnClientDisconnect( cl );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		var spawnpoint = All
			.OfType<SpawnPoint>()
			.OrderByDescending( x => SpawnpointWeight( pawn, x ) )
			.FirstOrDefault();

		if ( spawnpoint == null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {pawn}!" );
			return;
		}

		pawn.Transform = spawnpoint.Transform;

		if ( pawn is BoomerPlayer pl )
		{
			pl.SetViewAngles( pawn.Rotation.Angles() );
		}
	}

	[ConCmd.Admin]
	public static void GiveAwardCmd( string awardName )
	{
		if ( ConsoleSystem.Caller.Pawn is BoomerPlayer player )
		{
			player.GiveAward( awardName );
		}
	}

	/// <summary>
	/// The higher the better
	/// </summary>
	public float SpawnpointWeight( Entity pawn, Entity spawnpoint )
	{
		// We want to find the closest player (worst weight)
		float distance = float.MaxValue;

		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn == null ) continue;
			if ( client.Pawn == pawn ) continue;
			if ( client.Pawn is not BoomerPlayer pl ) continue;
			if ( pl.LifeState != LifeState.Alive ) continue;

			var spawnDist = (spawnpoint.Position - client.Pawn.Position).Length;
			distance = MathF.Min( distance, spawnDist );
		}

		return distance;
	}

	public override void OnKilled( IClient client, Entity pawn )
	{
		base.OnKilled( client, pawn );
		Hud.OnPlayerDied( To.Everyone, pawn as BoomerPlayer );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		
		//var PostProcessing = Map.Camera.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();
		
		//PostProcessing.FilmGrain.Intensity = 0.2f;
		//PostProcessing.FilmGrain.Response = 1;

		//PostProcessing.Vignette.Intensity = 1.0f;
		//PostProcessing.Vignette.Roundness = 1.5f;
		//PostProcessing.Vignette.Smoothness = 0.5f;
		//PostProcessing.Vignette.Color = Color.Black;


		//PostProcessing.Saturation = 1;

		//PostProcessing.MotionBlur.Scale = 0;

		Audio.SetEffect( "core.player.death.muffle1", 0 );

		if ( Game.LocalPawn is BoomerPlayer localPlayer )
		{
			var timeSinceDamage = localPlayer.TimeSinceDamage.Relative;
			var damageUi = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.3f;
			if ( damageUi > 0 )
			{
				//	PostProcessing.Saturate.Amount -= damageUi;
				//PostProcessing.Vignette.Color = Color.Lerp( PostProcessing.Vignette.Color, Color.Red, damageUi );
				//PostProcessing.Vignette.Intensity += damageUi;
				//PostProcessing.Vignette.Smoothness += damageUi;
				//PostProcessing.Vignette.Roundness += damageUi;

				//PostProcessing.MotionBlur.Scale = damageUi * 0.5f;
			}

			if ( localPlayer.Controller is BoomerController ctrl )
			{
				//var alpha = ctrl.GetMechanic<Dash>().DashAlpha;
				//var parabola = (float)Math.Pow( 4 * alpha * (1 - alpha), 2 );
				//PostProcessing.MotionBlur.Scale = parabola * 5f;
				//PostProcessing.MotionBlur.Samples = 4;
				//PostProcessing.Brightness = 1f + 1f * parabola;
			}

			var healthDelta = localPlayer.Health.LerpInverse( 0, 100.0f, true );
			healthDelta = MathF.Pow( healthDelta, 0.5f );

			//PostProcessing.Vignette.Color = Color.Lerp( PostProcessing.Vignette.Color, Color.Red, 1 - healthDelta );
			//PostProcessing.Vignette.Intensity += (1 - healthDelta) * 1f;
			//PostProcessing.Vignette.Smoothness += (1 - healthDelta) * 5f;
			//PostProcessing.Vignette.Roundness += (1 - healthDelta) * 1f;
			//PostProcessing.Saturate.Amount *= healthDelta;
			//PostProcessing.FilmGrain.Intensity += (1 - healthDelta) * 0.1f;

			Audio.SetEffect( "core.player.death.muffle1", 1 - healthDelta, velocity: 2.0f );
		}

		if ( CurrentState == GameStates.Warmup )
		{
			//PostProcessing.FilmGrain.Intensity = 0.4f;
			//PostProcessing.FilmGrain.Response = 0.5f;
			//PostProcessing.Saturation = 0;
		}
	}

	public static void Explosion( Entity weapon, Entity owner, Vector3 position, float radius, float damage, float forceScale, float ownerDamageScale = 1f )
	{
		Sound.FromWorld( "gl.explode", position );
		Particles.Create( "particles/gameplay/weapons/rocketlauncher/explosion/boomer_explosion_barrel.vpcf", position );

		if ( Game.IsClient ) return;

		var overlaps = FindInSphere( position, radius );

		foreach ( var overlap in overlaps )
		{
			if ( overlap is not ModelEntity entity || !entity.IsValid() )
				continue;

			if ( entity.LifeState != LifeState.Alive )
				continue;

			if ( !entity.PhysicsBody.IsValid() )
				continue;

			if ( entity.IsWorld )
				continue;

			var targetPos = entity.PhysicsBody.MassCenter;

			var dist = Vector3.DistanceBetween( position, targetPos );
			if ( dist > radius )
				continue;

			var tr = Trace.Ray( position, targetPos )
				.Ignore( weapon )
				.WorldOnly()
				.Run();

			if ( tr.Fraction < 0.98f )
				continue;

			var distanceMul = 1.0f - Math.Clamp( dist / radius, 0.0f, 1.0f );
			var dmg = damage * distanceMul;
			var force = (forceScale * distanceMul) * entity.PhysicsBody.Mass;
			var forceDir = (targetPos - position).Normal;

			if ( overlap == owner )
			{
				if ( NoRocketSelfDmg || MasterTrio || RocketArena )
				{
					dmg = 0;
				}
				else
				{
					dmg *= ownerDamageScale;
				}
				forceDir = (targetPos - (position + Vector3.Down * 32f)).Normal;
			}

			var damageInfo = DamageInfo.FromExplosion( position, forceDir * force, dmg )
				.WithTag( "blast" )
				.WithWeapon( weapon )
				.WithAttacker( owner );

			entity.TakeDamage( damageInfo );
		}
	}
	
	public override void RenderHud()
	{
		base.RenderHud();

		if ( Game.LocalPawn is not BoomerPlayer pl )
			return;

		pl.RenderHud( Screen.Size );
	}

	[ClientRpc]
	public override void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
		Sandbox.UI.KillFeed.Current?.AddEntry( leftid, left, rightid, right, method );
	}

	//[DebugOverlay( "entities", "Entities", "" )]
	//public static void EntityDebugOverlay()
	//{
	//	if ( !Game.IsClient ) return;

	//	var scale = Screen.Height / 1080.0f;
	//	var screenSize = Screen.Size / scale;
	//	var matrix = Matrix.CreateScale( scale );

	//	using var _ = Render.Draw2D.MatrixScope( matrix );

	//	foreach ( var ent in FindInSphere( CurrentView.Position, 600 ) )
	//	{
	//		var pos = ent.Position.ToScreen( screenSize );
	//		if ( !pos.HasValue ) continue;

	//		var str = $"{ent}";
	//		Render.Draw2D.FontFamily = "Poppins";
	//		Render.Draw2D.FontWeight = 600;
	//		Render.Draw2D.FontSize = 14;

	//		var textRect = Render.Draw2D.MeasureText( new Rect( pos.Value ), str );

	//		Render.Draw2D.Color = Color.Black;
	//		Render.Draw2D.DrawText( new Rect( pos.Value + Vector2.One ), str );
	//		Render.Draw2D.Color = Color.White;
	//		Render.Draw2D.DrawText( new Rect( pos.Value ), str );
	//	}
	//}
}
