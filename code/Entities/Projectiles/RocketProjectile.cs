[Library( "rocketprojectile" )]
[HideInEditor]
partial class RocketProjectile : ModelEntity
{
	public static readonly Model WorldModel = Model.Load( "models/light_arrow.vmdl" );
	
	public Entity IgnoreEntity { get; set; }

	private Sound RocketTrailSound;
	bool Stuck;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		Predictable = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		RocketTrailSound.Stop();
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		RocketTrailSound = Sound.FromEntity( "rl.trail", this );

	}

	[Event.Tick.Server]
	public virtual void Tick()
	{
		if ( !IsServer )
			return;

		if ( Stuck )
			return;

		float Speed = 2000.0f;
		var velocity = Rotation.Forward * Speed;

		var start = Position;
		var end = start + velocity * Time.Delta;

		var tr = Trace.Ray( start, end )
				.UseHitboxes()
				//.HitLayer( CollisionLayer.Water, !InWater )
				.Ignore( Owner )
				.Ignore( this )
				.Size( 1.0f )
				.Run();


		if ( tr.Hit )
		{
			// TODO: CLINK NOISE (unless flesh)

			// TODO: SPARKY PARTICLES (unless flesh)

			Stuck = true;
			Position = tr.EndPosition + Rotation.Forward * -1;

			if ( tr.Entity.IsValid() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, tr.Direction * 200, 20 )
													.UsingTraceResult( tr )
													.WithAttacker( Owner )
													.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
			Sound.FromWorld
			(
				"rl.explode",
				Position
			);
			RocketTrailSound.Stop();


			// TODO: Parent to bone so this will stick in the meaty heads
			SetParent( tr.Entity, tr.Bone );
			Owner = null;

			Position = tr.EndPosition + tr.Normal;
			Explode();

			//
			// Surface impact effect
			//
			tr.Normal = Rotation.Forward * -1;
			tr.Surface.DoBulletImpact( tr );
			velocity = default;

			// delete self in 60 seconds
			_ = DeleteAsync( 60.0f );

		}
		else
		{
			Position = end;
		}
	}

	public void Explode()
	{
		Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", Position );
		//trailParticle.Destroy();

		
		
		foreach ( var item in Entity.FindInSphere( Position, 96f ).ToList() )
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
					if ( tr.Entity == IgnoreEntity )
						damage /= 4;
					player.TakeDamage( DamageInfo.Generic( damage ) );
				}
			}
		}
		Delete();
	}
}
