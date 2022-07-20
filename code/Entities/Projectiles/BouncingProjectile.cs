[Library]
public partial class BouncingProjectile : BulletDropProjectile
{
	public float BounceSoundMinimumVelocity { get; set; }
	public string BounceSound { get; set; }
	public float Bounciness { get; set; } = 1f;

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
