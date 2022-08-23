namespace Boomer;

[Library( "boomer_grenadelauncher" ), HammerEntity]
[EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
[Title( "GrenadeLauncher" ), Category( "Weapons" )]
partial class GrenadeLauncher : BulletDropWeapon<GrenadeProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/grenadelauncher/w_grenadelauncher.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/grenadelauncher/grenadelauncher.vmdl";

	public override string ProjectileModel => "models/gameplay/projectiles/grenades/grenade.vmdl";
	public override string TrailEffect => "particles/grenade.vpcf";
	public override float ProjectileLifeTime => 1.5f;
	public override float Spread => 0.0f;
	public override string HitSound => "gl.impact";
	public override float Gravity => 30f;
	public override float Speed => 1300f;
	public override float PrimaryRate => 1.15f;
	public override AmmoType AmmoType => AmmoType.Grenade;
	public override int Bucket => 2;
	
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( Owner is not BoomerPlayer player ) return;
		
		if ( !TakeAmmo( 1 ) )
		{
			Reload();
			return;
		}

		PlaySound( "gl.shoot" );
		Reload();

		ShootEffects();
		
		player.SetAnimParameter( "b_attack", true );

		base.AttackPrimary();
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );
		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( .4f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );

		// outer lines
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.2f, 0.0f ) );
			var length = 10.0f + shootEase * 2.0f;
			var gap = 8.0f + shootEase * 50.0f;
			var thickness = 2.0f;


			draw.Line( thickness, center - Vector2.Up * gap * 3f + Vector2.Left * length * 2.5f, center - Vector2.Up * gap * 3f - Vector2.Left * length * 2.5f );
			draw.Line( thickness, center - Vector2.Up * gap * 1.5f + Vector2.Left * length * 2f, center - Vector2.Up * gap * 1.5f - Vector2.Left * length * 2f );
			draw.Line( thickness, center - Vector2.Up + Vector2.Left * length * 1.5f, center - Vector2.Up - Vector2.Left * length * 1.5f );
			draw.Line( thickness, center + Vector2.Up * gap * 1.5f + Vector2.Left * length * 1f, center + Vector2.Up * gap * 1.5f - Vector2.Left * length * 1f );
			draw.Line( thickness, center + Vector2.Up * gap * 3f + Vector2.Left * length * .5f, center + Vector2.Up * gap * 3f - Vector2.Left * length * .5f );

		}
	}

	protected override Vector3 AdjustProjectileVelocity( Vector3 velocity )
	{
		return velocity + Vector3.Up * 400f;
	}

	protected override void OnCreateProjectile( GrenadeProjectile projectile )
	{
		projectile.BounceSoundMinimumVelocity = 50f;
		projectile.Bounciness = 0.8f;
		projectile.BounceSound = "gl.impact";
		projectile.FromWeapon = this;

		base.OnCreateProjectile( projectile );
	}

	protected override void OnProjectileHit( GrenadeProjectile projectile, TraceResult trace )
	{
		DeathmatchGame.Explosion( projectile, projectile.Attacker, projectile.Position, 140f, 100f, 1f );
	}
}
