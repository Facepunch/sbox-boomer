[Library( "dm_grenadelauncher" ), HammerEntity]
[EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
[Title( "GrenadeLauncher" ), Category( "Weapons" )]
partial class GrenadeLauncher : BulletDropWeapon<BouncingProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/grenadelauncher/w_grenadelauncher.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/grenadelauncher/grenadelauncher.vmdl";

	public override string ProjectileModel => "models/gameplay/projectiles/grenades/grenade.vmdl";
	public override string TrailEffect => "particles/grenade.vpcf";
	public override float ProjectileLifeTime => 3f;
	public override string HitSound => "gl.impact";
	public override float Gravity => 30f;
	public override float Speed => 1000f;
	public override float PrimaryRate => 0.75f;
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
		return velocity + Vector3.Up * 300f;
	}

	protected override void OnCreateProjectile( BouncingProjectile projectile )
	{
		projectile.Bounciness = 0.8f;
		projectile.BounceSound = "gl.impact";

		base.OnCreateProjectile( projectile );
	}

	protected override void OnProjectileHit( BouncingProjectile projectile, TraceResult trace )
	{
		Sound.FromWorld( "rl.explode", trace.EndPosition );
		Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", trace.EndPosition );

		if ( IsServer )
		{
			DeathmatchGame.Explosion( this, Owner, projectile.Position, 400f, 100f, 1f );
		}
	}
}
