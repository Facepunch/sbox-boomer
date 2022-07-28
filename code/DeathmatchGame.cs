global using Sandbox;
global using SandboxEditor;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
using Boomer.Movement;

/// <summary>
/// This is the heart of the gamemode. It's responsible for creating the player and stuff.
/// </summary>
partial class DeathmatchGame : Game
{
	[Net] private DeathmatchHud Hud { get; set; }
	private StandardPostProcess PostProcessing { get; set; }

	public static bool HasFirstPlayerDied { get; set; }

	public DeathmatchGame()
	{
		//
		// Create the HUD entity. This is always broadcast to all clients
		// and will create the UI panels clientside.
		//
		if ( IsServer )
		{
			PrecacheAssets();

			Hud = new DeathmatchHud();

			_ = GameLoopAsync();
		}

		if ( IsClient )
		{
			PostProcessing = new StandardPostProcess();
			PostProcess.Add( PostProcessing );
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

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
		ItemRespawn.Init();
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		var player = new BoomerPlayer();
		player.UpdateClothes( cl );
		player.Respawn();
		
		player.PlayerColor = Color.Random;
		
		cl.Pawn = player;
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		var spawnpoint = All
			.OfType<SpawnPoint>()
			.OrderByDescending( x => SpawnpointWeight( pawn, x ) )
			.ThenBy( x => Guid.NewGuid() )
			.FirstOrDefault();

		if ( spawnpoint == null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {pawn}!" );
			return;
		}

		pawn.Transform = spawnpoint.Transform;
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
		float distance = 0;

		foreach ( var client in Client.All )
		{
			if ( client.Pawn == null ) continue;
			if ( client.Pawn == pawn ) continue;
			if ( client.Pawn.LifeState != LifeState.Alive ) continue;

			var spawnDist = (spawnpoint.Position - client.Pawn.Position).Length;
			distance = MathF.Max( distance, spawnDist );
		}

		return distance;
	}

	public override void OnKilled( Client client, Entity pawn )
	{
		base.OnKilled( client, pawn );
		Hud.OnPlayerDied( To.Everyone, pawn as BoomerPlayer );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		PostProcessing.Sharpen.Enabled = false;
		PostProcessing.Sharpen.Strength = 0.5f;

		PostProcessing.FilmGrain.Enabled = true;
		PostProcessing.FilmGrain.Intensity = 0.2f;
		PostProcessing.FilmGrain.Response = 1;

		PostProcessing.Vignette.Enabled = true;
		PostProcessing.Vignette.Intensity = 1.0f;
		PostProcessing.Vignette.Roundness = 1.5f;
		PostProcessing.Vignette.Smoothness = 0.5f;
		PostProcessing.Vignette.Color = Color.Black;

		PostProcessing.Saturate.Enabled = true;
		PostProcessing.Saturate.Amount = 1;

		PostProcessing.Blur.Enabled = false;

		PostProcessing.Pixelate.Enabled = false;
		PostProcessing.Pixelate.PixelCount = 512;

		PostProcessing.MotionBlur.Enabled = false;

		PostProcessing.PaniniProjection.Enabled = false;

		Audio.SetEffect( "core.player.death.muffle1", 0 );

		if ( Local.Pawn is BoomerPlayer localPlayer )
		{
			var timeSinceDamage = localPlayer.TimeSinceDamage.Relative;
			var damageUi = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.3f;
			if ( damageUi > 0 )
			{
			//	PostProcessing.Saturate.Amount -= damageUi;
				PostProcessing.Vignette.Color = Color.Lerp( PostProcessing.Vignette.Color, Color.Red, damageUi );
				PostProcessing.Vignette.Intensity += damageUi;
				PostProcessing.Vignette.Smoothness += damageUi;
				PostProcessing.Vignette.Roundness += damageUi;

				PostProcessing.Blur.Enabled = true;
				PostProcessing.Blur.Strength = damageUi * 0.5f;
			}

			if( localPlayer.Controller is BoomerController ctrl )
			{
				var alpha = ctrl.GetMechanic<Dash>().DashAlpha;
				var parabola = (float)Math.Pow( 4 * alpha * (1 - alpha), 2 );
				PostProcessing.MotionBlur.Enabled = false;
				PostProcessing.MotionBlur.Scale = parabola * 5f;
				PostProcessing.MotionBlur.Samples = 4;
				PostProcessing.Brightness.Enabled = ctrl.IsDashing;
				PostProcessing.Brightness.Multiplier = 1f + 1f * parabola;
			}

			var healthDelta = localPlayer.Health.LerpInverse( 0, 100.0f, true );
			healthDelta = MathF.Pow( healthDelta, 0.5f );

			PostProcessing.Vignette.Color = Color.Lerp( PostProcessing.Vignette.Color, Color.Red, 1 - healthDelta );
			PostProcessing.Vignette.Intensity += (1 - healthDelta) * 1f;
			PostProcessing.Vignette.Smoothness += (1 - healthDelta) * 5f;
			PostProcessing.Vignette.Roundness += (1 - healthDelta) * 1f;
			//PostProcessing.Saturate.Amount *= healthDelta;
			//PostProcessing.FilmGrain.Intensity += (1 - healthDelta) * 0.1f;

			Audio.SetEffect( "core.player.death.muffle1", 1 - healthDelta, velocity: 2.0f );
		}

		if ( CurrentState == GameStates.Warmup )
		{
			PostProcessing.FilmGrain.Intensity = 0.4f;
			PostProcessing.FilmGrain.Response = 0.5f;
			PostProcessing.Saturate.Amount = 0;
		}
	}

	public static void Explosion( Entity weapon, Entity owner, Vector3 position, float radius, float damage, float forceScale, float ownerDamageScale = 1f )
	{
		Sound.FromWorld( "gl.explode", position );
		Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", position );

		if ( Host.IsClient ) return;

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
				dmg *= ownerDamageScale;
				forceDir = (targetPos - (position + Vector3.Down * 32f)).Normal;
			}

			var damageInfo = DamageInfo.Explosion( position, forceDir * force, dmg )
				.WithFlag( DamageFlags.Blast )
				.WithWeapon( weapon )
				.WithAttacker( owner );

			entity.TakeDamage( damageInfo );
		}
	}

	public override void RenderHud()
	{
		base.RenderHud();

		if ( Local.Pawn is not BoomerPlayer pl ) 
			return;

		var scale = Screen.Height / 1080.0f;
		var screenSize = Screen.Size / scale;
		var matrix = Matrix.CreateScale( scale );

		using ( Render.Draw2D.MatrixScope( matrix ) )
		{
			pl.RenderHud( screenSize );
		}
	}

	[ClientRpc]
	public override void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
		Sandbox.UI.KillFeed.Current?.AddEntry( leftid, left, rightid, right, method );
	}

	[DebugOverlay( "entities", "Entities", "" )]
	public static void EntityDebugOverlay()
	{
		if ( !Host.IsClient ) return;

		var scale = Screen.Height / 1080.0f;
		var screenSize = Screen.Size / scale;
		var matrix = Matrix.CreateScale( scale );

		using var _ = Render.Draw2D.MatrixScope( matrix );

		foreach ( var ent in FindInSphere( CurrentView.Position, 600 ) )
		{
			var pos = ent.Position.ToScreen( screenSize );
			if ( !pos.HasValue ) continue;

			var str = $"{ent}";
			Render.Draw2D.FontFamily = "Poppins";
			Render.Draw2D.FontWeight = 600;
			Render.Draw2D.FontSize = 14;

			var textRect = Render.Draw2D.MeasureText( new Rect( pos.Value ), str );

			Render.Draw2D.Color = Color.Black;
			Render.Draw2D.DrawText( new Rect( pos.Value + Vector2.One ), str );
			Render.Draw2D.Color = Color.White;
			Render.Draw2D.DrawText( new Rect( pos.Value ), str );
		}
	}

}
