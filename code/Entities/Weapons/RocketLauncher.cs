[Library( "dm_rocketlauncher" ), HammerEntity]
[EditorModel( "weapons/rust_crossbow/rust_crossbow.vmdl" )]
[Title( "RocketLauncher" ), Category( "Weapons" )]
public partial class RocketLauncher : BulletDropWeapon<RocketProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/rocketlauncher/w_rocketlauncher.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/rocketlauncher/rocketlauncher.vmdl";

	public AnimatedEntity AnimationOwner => Owner as AnimatedEntity;
	public override bool GivesAirshotAward => true;
	public override string ProjectileModel => "models/gameplay/projectiles/rockets/rocket.vmdl";
	public override string TrailEffect => "particles/gameplay/weapons/rocketlauncher/trail_1.vpcf";
	public override float Gravity => 0f;
	public override float Speed => 1800f;
	public override bool CanZoom => true;
	public override float PrimaryRate => 1.4f;
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
		DeathmatchGame.Explosion( projectile, projectile.Attacker, projectile.Position, 180f, 40f, 1f, 0.3f );

		if ( IsClient )
		{
			trace.Surface.DoBulletImpact( trace );
		}

		base.OnProjectileHit( projectile, trace );
	}
}
