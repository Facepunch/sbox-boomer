namespace Boomer;

public abstract partial class BulletDropWeapon<T> : DeathmatchWeapon where T : BulletDropProjectile, new()
{
	public virtual string ProjectileModel => "";
	public virtual float ProjectileRadius => 10f;
	public virtual float ProjectileLifeTime => 10f;
	public virtual string ImpactEffect => null;
	public virtual string TrailEffect => null;
	public virtual string HitSound => null;
	public virtual float InheritVelocity => 0f;
	public virtual string MuzzleAttachment => "muzzle";
	public virtual float BulletRange => 10000f;
	public virtual float DamageFalloffStart => 2000f;
	public virtual string DamageType => "bullet";
	public virtual float DamageFalloffEnd => 6000f;
	public virtual float BaseDamage => 20f;
	public virtual float Gravity => 50f;
	public virtual float Speed => 2000f;
	public virtual float Spread => 0.05f;
	public virtual List<string> FlybySounds => new()
	{
		"flyby"
	};

	public float GetDamageFalloff( float distance, float damage )
	{
		return WeaponUtil.GetDamageFalloff( distance, damage, DamageFalloffStart, DamageFalloffEnd );
	}

	public override void AttackPrimary()
	{
		if ( Prediction.FirstTime )
        {
			Rand.SetSeed( Time.Tick );
			FireProjectile();
        }
	}

	public virtual void FireProjectile()
	{
		if ( Owner is not BoomerPlayer player )
			return;

		var projectile = new T()
		{
			ExplosionEffect = ImpactEffect,
			FaceDirection = true,
			IgnoreEntity = this,
			FlybySounds = FlybySounds,
			TrailEffect = TrailEffect,
			Simulator = player.Projectiles,
			Attacker = player,
			HitSound = HitSound,
			LifeTime = ProjectileLifeTime,
			Gravity = Gravity,
			ModelName = ProjectileModel
		};

		OnCreateProjectile( projectile );

		var muzzle = EffectEntity.GetAttachment( MuzzleAttachment );
		var position = muzzle.Value.Position;
		var forward = player.EyeRotation.Forward;
		var endPosition = player.EyePosition + forward * BulletRange;
		var trace = Trace.Ray( player.EyePosition, endPosition )
			.WithAnyTags( "weapon", "player", "solid" )
			.Ignore( player )
			.Ignore( this )
			.Run();

		var direction = (trace.EndPosition - position).Normal;
		direction += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
		direction = direction.Normal;

		var velocity = (direction * Speed) + (player.Velocity * InheritVelocity);
		velocity = AdjustProjectileVelocity( velocity );
		projectile.Initialize( position, velocity, ProjectileRadius, (p, t) => OnProjectileHit( (T)p, t ) );
	}

	protected void DealDamage( Entity target, Vector3 position, Vector3 force )
	{
		DealDamage( target, position, force, BaseDamage );
	}

	protected void DealDamage( Entity target, Vector3 position, Vector3 force, float damage )
	{
		var damageInfo = new DamageInfo()
			.WithAttacker( Owner )
			.WithWeapon( this )
			.WithPosition( position )
			.WithForce( force )
			.WithTag( DamageType );

		damageInfo.Damage = damage;

		target.TakeDamage( damageInfo );
	}

	protected virtual float ModifyDamage( Entity victim, float damage )
	{
		return damage;
	}

	protected virtual Vector3 AdjustProjectileVelocity( Vector3 velocity )
	{
		return velocity;
	}

	protected virtual void DamageInRadius( Vector3 position, float radius, float baseDamage, float force = 1f )
	{
		DeathmatchGame.Explosion( this, Owner, position, radius, baseDamage, force );
	}

	protected virtual void OnCreateProjectile( T projectile )
	{

	}

	protected virtual void OnProjectileHit( T projectile, TraceResult trace )
	{
		if ( IsServer && trace.Entity.IsValid() )
		{
			var distance = trace.Entity.Position.Distance( projectile.StartPosition );
			var damage = GetDamageFalloff( distance, BaseDamage );
			DealDamage( trace.Entity, projectile.Position, projectile.Velocity * 0.1f, damage );
		}
	}
}
