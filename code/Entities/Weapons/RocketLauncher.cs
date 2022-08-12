namespace Boomer;

[Library( "boomer_rocketlauncher" ), HammerEntity]
[EditorModel( "weapons/rust_crossbow/rust_crossbow.vmdl" )]
[Title( "RocketLauncher" ), Category( "Weapons" )]
public partial class RocketLauncher : BulletDropWeapon<RocketProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/rocketlauncher/w_rocketlauncher.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/rocketlauncher/rocketlauncher.vmdl";

	public AnimatedEntity AnimationOwner => Owner as AnimatedEntity;
	public override bool GivesAirshotAward => true;
	public override string ProjectileModel => "models/gameplay/projectiles/rockets/rocket.vmdl";
	
	public override float ProjectileRadius => 2f;
	public override string TrailEffect => "particles/gameplay/weapons/rocketlauncher/trail_1.vpcf";
	public override float Spread => 0.0f;
	public override float Gravity => 0f;
	public override float Speed => 1800f;
	public override bool CanZoom => true;
	public override float PrimaryRate => 1.1f;
	public override int Bucket => 3;
	public override AmmoType AmmoType => AmmoType.Rockets;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;

		if ( DeathmatchGame.InstaGib )
		{
			Delete();
		}
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

		AnimationOwner.SetAnimParameter( "b_attack", true );

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
		draw.Color = color.WithAlpha( 1.8f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );

		// center circle
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.1f, 0.0f ) );
			var length = 2f + shootEase * 2.0f;
			draw.Circle( center, length );
		}


		draw.Color = draw.Color.WithAlpha( draw.Color.a * 0.2f );

		// outer lines
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.2f, 0.0f ) );
			var length = 10.0f + shootEase * 2.0f;
			var gap = 8.0f + shootEase * 50.0f;
			var thickness = 2.0f;

			draw.Line( thickness, center - new Vector2( 0, gap + length ), center - new Vector2( 0, gap) );
			draw.Line( thickness, center + new Vector2( 0, gap + length ), center + new Vector2( 0, gap) );
			
			draw.Line( thickness, center - new Vector2( gap + length, 0 ), center - new Vector2( gap, 0 ) );
			draw.Line( thickness, center + new Vector2( gap + length, 0 ), center + new Vector2( gap, 0 ) );
		}
	}

	protected override void OnProjectileHit( RocketProjectile projectile, TraceResult trace )
	{
		DeathmatchGame.Explosion( projectile, projectile.Attacker, projectile.Position, 180f, 80f, 1f, 0.3f );

		if ( IsClient )
		{
			trace.Surface.DoBulletImpact( trace );
		}

		base.OnProjectileHit( projectile, trace );
	}
}
