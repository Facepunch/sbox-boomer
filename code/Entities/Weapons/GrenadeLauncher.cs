[Library( "dm_grenadelauncher" ), HammerEntity]
[EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
[Title( "GrenadeLauncher" ), Category( "Weapons" )]
partial class GrenadeLauncher : BulletDropWeapon<GrenadeProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/grenadelauncher/w_grenadelauncher.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/grenadelauncher/grenadelauncher.vmdl";

	public override string ProjectileModel => "models/gameplay/projectiles/grenades/grenade.vmdl";
	public override string TrailEffect => "particles/grenade.vpcf";
	public override float ProjectileLifeTime => 1.5f;
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

		player.SetAnimParameter( "b_attack", true );

		base.AttackPrimary();

		if ( IsServer && player.AmmoCount( AmmoType.Grenade ) == 0 )
		{
			Delete();
			player.SwitchToBestWeapon();
		}
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
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
		DeathmatchGame.Explosion( projectile, projectile.Attacker, projectile.Position, 400f, 100f, 1f );
	}
}
