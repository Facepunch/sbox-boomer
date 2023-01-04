using Sandbox;
using System.Linq;
using Facepunch.Boomer.Mechanics;

namespace Facepunch.Boomer.WeaponSystem;

public partial class Projectile : ModelEntity
{
	/// <summary>
	/// Projectile data asset
	/// </summary>
	[Net] protected ProjectileData Data { get; set; }

	public Particles ActiveParticle { get; set; }
	protected float GravityModifier { get; set; }

	private bool HasExploded = false;

	public ProjectileSimulator Simulator => (Owner as Player)?.ProjectileSimulator;

	public override void Spawn()
	{
		Predictable = true;
	}

	[ClientRpc]
	protected void SendActiveEffects()
	{
		if ( !string.IsNullOrEmpty( Data.ParticlePath ) )
		{
			ActiveParticle = Particles.Create( Data.ParticlePath, this, true );
		}
	}

	public void Initialize( ProjectileData data )
	{
		Data = data;

		Model = Data.CachedModel;
		Transmit = TransmitType.Always;
		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, Data.Radius );

		Tags.Add( "trigger" );

		SendActiveEffects( );

		// Async delete
		DeleteAsync( Data.Lifetime );

		if ( Owner.IsValid() )
		{
			var tr = Trace.Ray( Owner.AimRay, 50f )
				.Ignore( Owner )
				.Run();

			Position = tr.EndPosition;
			Velocity = Owner.AimRay.Forward * data.InitialForce.x + Vector3.Up * data.InitialForce.y;

			Simulator.Add( this );
		}

		// Simulate first frame to setup shit like Rotation
		Simulate( null );
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

	public override void Simulate( IClient cl )
	{
		var drag = CalculateDrag( Velocity );
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

		if ( trace.Hit || trace.StartedSolid )
		{
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
						using ( Prediction.Off() )
						{
							PlaySound( Data.BounceSoundPath );
						}
					}
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		if ( Game.IsServer )
		{
			if ( Data.ExplodeOnDeath )
			{
				Explode();
			}

			// Destroy active effects
			ActiveParticle?.Destroy( true );
		}

		Simulator?.Remove( this );
	}

	public void Explode()
	{
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
					.WithWeapon( this )
					.WithAttacker( Owner );

				ent.TakeDamage( damageInfo );

				if ( ent is Player player && player.Controller != null )
				{
					player.Controller.GetMechanic<WalkMechanic>()
						.ClearGroundEntity();

					if ( Data.ClearZVelocity )
						player.Controller.Velocity = player.Controller.Velocity.WithZ( 0f );

					player.Controller.Velocity += forceDir * force;
				}
			}
		}
	}
}
