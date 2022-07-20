[Library( "NailProjectile" )]
[HideInEditor]
partial class NailProjectile : ModelEntity
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/projectiles/nails/nails.vmdl" );

	public Entity FromWeapon;

	bool Stuck;

	bool Passed = false;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		Predictable = false;
	}

	[Event.Tick.Server]
	public virtual void Tick()
	{
		if ( !IsServer )
			return;

		if ( Stuck )
			return;
			
		float Speed = 1500.0f;
		var velocity = Rotation.Forward * Speed;

		var start = Position;
		var end = start + velocity * Time.Delta;

		var tr = Trace.Ray( start, end )
				.UseHitboxes()
				//.HitLayer( CollisionLayer.Water, !InWater )
				.WithAnyTags( "weapon", "player", "solid" )
				.Ignore( Owner )
				.Ignore( this )
				.Size( 1.0f )
				.Run();

		FlyBy( start, end );
		
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
													.WithWeapon( FromWeapon );
				tr.Entity.TakeDamage( damageInfo );
			}

			// TODO: Parent to bone so this will stick in the meaty heads
			SetParent( tr.Entity, tr.Bone );
			Owner = null;

			//
			// Surface impact effect
			//
			tr.Normal = Rotation.Forward * -1;
			tr.Surface.DoBulletImpact( tr );
			velocity = default;

			// delete self in 60 seconds
			_ = DeleteAsync( 10.0f );
		}
		else
		{
			Position = end;
		}
	}

	public void FlyBy(Vector3 start,Vector3 end)
	{
		var trace = Trace.Ray( start, end )
		.WithAnyTags( "weapon", "player", "solid" )
		.Ignore( this )
		.Ignore( Owner )
		.Size( 128.0f )
		.Run();

		if ( trace.Hit )
		{
			
			if ( trace.Entity is BoomerPlayer player )
			{
				
				if ( !Passed)
				{
					//
					//Only play to flyby target
					//
					PlayFlyBySound( To.Single( player ), "flyby" );

					//
					//Only play once
					//
					Passed = true;
				}
			}

		}
	}

	[ClientRpc]
	public void PlayFlyBySound( string sound )
	{
		Sound.FromEntity( sound,this );
	}
}
