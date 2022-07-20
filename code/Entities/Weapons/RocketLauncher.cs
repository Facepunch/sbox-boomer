﻿using Boomer.Movement;

[Library( "dm_rocketlauncher" ), HammerEntity]
[EditorModel( "weapons/rust_crossbow/rust_crossbow.vmdl" )]
[Title( "RocketLauncher" ), Category( "Weapons" )]
public partial class RocketLauncher : BulletDropWeapon<RocketProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/rocketlauncher/w_rocketlauncher.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/rocketlauncher/rocketlauncher.vmdl";

	public override string ProjectileModel => "models/gameplay/projectiles/rockets/rocket.vmdl";
	public override float Gravity => 10f;
	public override float Speed => 1500f;
	public override bool CanZoom => true;
	public override float PrimaryRate => 1;
	public override int Bucket => 3;
	public override AmmoType AmmoType => AmmoType.Rockets;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
	}

	public override void AttackPrimary()
	{
		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}
			return;
		}

		ShootEffects();
		PlaySound( "rl.shoot" );

		base.AttackPrimary();
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;

	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}

	TimeSince timeSinceZoomed;

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );
		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.2f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );

		// center circle
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.1f, 0.0f ) );
			var length = 2.0f + shootEase * 2.0f;
			draw.Circle( center, length );
		}


		draw.Color = draw.Color.WithAlpha( draw.Color.a * 0.2f );

		// outer lines
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.2f, 0.0f ) );
			var length = 3.0f + shootEase * 2.0f;
			var gap = 30.0f + shootEase * 50.0f;
			var thickness = 2.0f;

			draw.Line( thickness, center + Vector2.Up * gap + Vector2.Left * length, center + Vector2.Up * gap - Vector2.Left * length );
			draw.Line( thickness, center - Vector2.Up * gap + Vector2.Left * length, center - Vector2.Up * gap - Vector2.Left * length );

			draw.Line( thickness, center + Vector2.Left * gap + Vector2.Up * length, center + Vector2.Left * gap - Vector2.Up * length );
			draw.Line( thickness, center - Vector2.Left * gap + Vector2.Up * length, center - Vector2.Left * gap - Vector2.Up * length );
		}
	}

	protected override void OnProjectileHit( RocketProjectile projectile, TraceResult trace )
	{
		Sound.FromWorld( "rl.explode", trace.EndPosition );
		Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", trace.EndPosition );

		if ( IsServer )
		{
			foreach ( var item in FindInSphere( trace.EndPosition, 96f ).ToList() )
			{
				if ( item is BoomerPlayer player )
				{
					Vector3 middlePos = (player.Position + player.EyePosition) / 2;
					var tr = Trace.Ray( Position, middlePos ).Run();
					if ( tr.Hit )
					{
						player.GroundEntity = null;
						player.Velocity += (middlePos - Position) * 12;
						var damage = (middlePos - Position).Length;
						damage = damage.Remap( 0, 64, 10, 40 );
						if ( tr.Entity == Owner ) damage /= 4;
						player.TakeDamage( DamageInfo.Generic( damage ) );
					}
				}
			}
		}
		else
		{
			trace.Surface.DoBulletImpact( trace );
		}

		base.OnProjectileHit( projectile, trace );
	}
}
