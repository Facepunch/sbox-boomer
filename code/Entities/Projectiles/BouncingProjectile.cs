[Library]
public partial class BouncingProjectile : BulletDropProjectile
{
	public float BounceSoundMinimumVelocity { get; set; }
	public string BounceSound { get; set; }
	public float Bounciness { get; set; } = 1f;
	
	public Entity FromWeapon;

	protected override void PostSimulate( TraceResult trace )
	{
		if ( trace.Hit )
		{
			var reflect = Vector3.Reflect( trace.Direction, trace.Normal );

			GravityModifier = 0f;
			Velocity = reflect * Velocity.Length * Bounciness;

			if ( Velocity.Length > BounceSoundMinimumVelocity )
			{
				if ( !string.IsNullOrEmpty( BounceSound ) )
					PlaySound( BounceSound );
			}
			
			if ( trace.Entity is BoomerPlayer )
			{
				if ( trace.Entity.IsLocalPawn )
				{
					return;
				}
				Sound.FromWorld( "rl.explode", trace.EndPosition );
				Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", trace.EndPosition );
				if ( IsServer )
				{
					DeathmatchGame.Explosion( FromWeapon, Owner, Position, 400f, 100f, 1f );
				}
				OnDestroy();
			}
		}

		base.PostSimulate( trace );
	}

	protected override bool HasHitTarget( TraceResult trace )
	{
		if ( LifeTime.HasValue )
		{
			return false;
		}

		return base.HasHitTarget( trace );
	}
}
