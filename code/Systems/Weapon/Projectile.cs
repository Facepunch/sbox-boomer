using Sandbox;
using System.Linq;
using Facepunch.Boomer.Mechanics;

namespace Facepunch.Boomer.WeaponSystem;

public partial class Projectile : ModelEntity
{
	/// <summary>
	/// Projectile data asset
	/// </summary>
	[Net, Predicted] protected ProjectileData Data { get; set; }
	public Weapon Weapon { get; set; }

	public Particles ActiveParticle { get; set; }
	public Sound ActiveSound { get; set; }

	protected float GravityModifier { get; set; }

	private TimeUntil DestroyTime { get; set; }
	private bool HasExploded = false;

	public ProjectileSimulator Simulator { get; set; }

	public bool IsClientProxy()
	{
		return !IsClientOnly && Owner.IsValid() && Owner.IsLocalPawn;
	}

	public override void Spawn()
	{
		Predictable = true;

		base.Spawn();
	}

	public override void ClientSpawn()
	{
		// We only want to create effects if we're the server-side copy.
		if ( !IsClientProxy() )
		{
			CreateEffects();
		}

		base.ClientSpawn();
	}

	protected void CreateEffects()
	{
		if ( !string.IsNullOrEmpty( Data.ParticlePath ) )
		{
			ActiveParticle = Particles.Create( Data.ParticlePath, this, true );
		}

		if ( !string.IsNullOrEmpty( Data.ActiveSoundPath ) )
		{
			ActiveSound = Sound.FromEntity( Data.ActiveSoundPath, this ).SetVolume( 0.75f ).SetPitch( 1.5f );
		}

		if ( !string.IsNullOrEmpty( Data.ModelPath ) )
		{
			ModelEntity = new SceneObject( Game.SceneWorld, Data.ModelPath );
			ModelEntity.Transform = Transform;
		}
	}

	protected SceneObject ModelEntity { get; set; }

	public void Initialize( ProjectileData data )
	{
		EnableDrawing = false;
		DestroyTime = data.Lifetime;
		Transmit = TransmitType.Always;
		Data = data;

		if ( Simulator.IsValid() )
		{
			Simulator?.Add( this );
			Owner = Simulator.Owner;

			var tr = Trace.Ray( Owner.AimRay, 50f )
				.Ignore( Owner )
				.Run();

			Position = tr.EndPosition;
			Velocity = Owner.AimRay.Forward * data.InitialForce.x + Vector3.Up * data.InitialForce.y;
			Rotation = Rotation.LookAt( Velocity.Normal );

			if ( Game.IsServer )
			{
				using ( LagCompensation() )
				{
					// Work out the number of ticks for this client's latency that it took for us to receive this input.
					var tickDifference = ((float)(Owner.Client.Ping / 1000f) / Time.Delta).CeilToInt();
					Log.Info( $"We took {tickDifference} ticks..." );

					// Advance the simulation by that number of ticks.
					for ( var i = 0; i < tickDifference; i++ )
					{
						if ( !HasExploded && IsValid )
						{
							Simulate();
						}
					}
				}
			}
		}

		if ( IsClientOnly )
		{
			using ( Prediction.Off() )
			{
				CreateEffects();
			}
		}
	}

	protected virtual Vector3 GetTargetPosition()
	{
		var newPosition = Position;
		newPosition += Velocity * Time.Delta;

		GravityModifier += Data.Gravity;
		newPosition -= new Vector3( 0f, 0f, GravityModifier * Time.Delta );

		return newPosition;
	}

	public static readonly float KGM3ToKGI3 = 0.0164f; // kg/m³ -> kg/in³
	public static readonly float AirDensity = 1.204f * KGM3ToKGI3; // kg/in³
	public static readonly float DragCoefficient = 0.5f;

	private static Vector3 CalculateDrag( Vector3 velocity )
	{
		var dragForce = 0.1f * AirDensity * velocity.LengthSquared * DragCoefficient;
		return -dragForce * velocity.Normal;
	}

	public void Simulate()
	{
		var drag = CalculateDrag( Velocity ) * Data.DragScale;
		Velocity += drag * Time.Delta;

		Rotation = Rotation.LookAt( Velocity.Normal );

		var newPosition = GetTargetPosition();

		var trace = Trace.Ray( Position, newPosition )
			.Size( Data.Radius )
			.Ignore( this )
			.Ignore( Owner )
			.WithAnyTags( "solid", "player" )
			.WithoutTags( "trigger" )
			.Run();

		Position = trace.EndPosition;

		if ( DestroyTime )
		{
			if ( Data.ExplodeOnDeath )
			{
				Explode();
			}

			Delete();

			return;
		}

		if ( trace.Hit || trace.StartedSolid )
		{
			if ( Data.ExplodeOnImpact )
			{
				Explode();
				Delete();
			}

			if ( trace.Tags.Any( x => Data.ExplodeHitTags.Any( y => x == y ) ) )
			{
				Explode();

				if ( Data.NoDeleteOnExplode == false )
					Delete();
			}
			else
			{
				if ( Data.Bounciness > 0f )
				{
					var reflect = Vector3.Reflect( Velocity.Normal, trace.Normal );

					GravityModifier = 0f;
					Velocity = reflect * Velocity.Length * Data.Bounciness;

					if ( !string.IsNullOrEmpty( Data.BounceSoundPath ) && Velocity.Length > Data.BounceSoundMinVelocity )
					{
						PlaySound( Data.BounceSoundPath );
					}
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		ActiveParticle?.Destroy( true );
		ActiveSound.Stop();
		ModelEntity?.Delete();
		Simulator?.Remove( this );
	}

	[Event.PreRender]
	protected virtual void PreRender()
	{
		if ( ModelEntity.IsValid() )
		{
			ModelEntity.Transform = Transform;
		}
	}

	[Event.Tick.Server]
	protected virtual void ServerTick()
	{
		if ( !Simulator.IsValid() )
		{
			Simulate();
		}
	}

	public void Explode()
	{
		// If we're already exploded. Do nothing.
		if ( HasExploded ) return;

		HasExploded = true;

		using ( Prediction.Off() )
		{
			// Effects
			if ( !string.IsNullOrEmpty( Data.ExplosionSoundPath ) )
				Sound.FromWorld( Data.ExplosionSoundPath, Position );

			if ( !string.IsNullOrEmpty( Data.ExplosionParticlePath ) )
				Particles.Create( Data.ExplosionParticlePath, Position );

			// Damage
			if ( Game.IsServer )
			{
				var overlaps = FindInSphere( Position, Data.ExplosionRadius );

				foreach ( var overlap in overlaps )
				{
					if ( overlap is not ModelEntity ent || !ent.IsValid() )
						continue;

					if ( ent.LifeState != LifeState.Alive )
						continue;

					if ( !ent.PhysicsBody.IsValid() )
						continue;

					if ( ent.IsWorld )
						continue;

					var targetPos = ent.PhysicsBody.MassCenter;

					var dist = Vector3.DistanceBetween( Position, targetPos );
					if ( dist > Data.ExplosionRadius )
						continue;

					var tr = Trace.Ray( Position, targetPos )
						.Ignore( this )
						.WorldOnly()
						.Run();

					if ( tr.Fraction < 0.98f )
						continue;

					var forceScale = 1f;
					var distanceMul = Data.ExplosionDamageFalloff.Evaluate( dist / Data.ExplosionRadius );

					var dmg = Data.ExplosionDamage * distanceMul;
					var force = (forceScale * distanceMul) * ent.PhysicsBody.Mass;
					var forceDir = (targetPos - Position).Normal;

					if ( ent == Owner || ent == (Owner as Player)?.ActiveWeapon )
						dmg *= Data.SelfDamageScale;

					var damageInfo = DamageInfo.FromExplosion( Position, forceDir * force, dmg )
						.WithTag( "blast" )
						.WithWeapon( this )
						.WithAttacker( Owner );

					ent.TakeDamage( damageInfo );
					ent.ApplyAbsoluteImpulse( forceDir * force );

					if ( ent is Player player && player.Controller != null )
					{
						forceDir = (targetPos - (Position + Vector3.Down * 32f)).Normal;

						player.Controller.ApplyForce( forceDir * force );
					}
				}
			}
		}
	}
}
