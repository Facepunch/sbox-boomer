using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Facepunch.Boomer.WeaponSystem;

public partial class Projectile : ModelEntity
{
	/// <summary>
	/// Projectile data asset
	/// </summary>
	[Net] protected ProjectileData Data { get; set; }

	public Particles ActiveParticle { get; set; }
	protected float GravityModifier { get; set; }

	public override void Spawn()
	{
		Predictable = true;
	}

	public void Initialize( ProjectileData data )
	{
		Data = data;

		Model = Data.CachedModel;
		Transmit = TransmitType.Always;
		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, Data.Radius );

		Tags.Add( "trigger" );

		if ( !string.IsNullOrEmpty( Data.ParticlePath ) )
			ActiveParticle = Particles.Create( Data.ParticlePath, this, true );

		// Async delete
		DeleteAsync( Data.Lifetime );

		if ( Owner.IsValid() )
		{
			Position = Owner.AimRay.Position + Owner.AimRay.Forward * 50f;
			Velocity = Owner.AimRay.Forward * data.InitialForce.x + Vector3.Up * data.InitialForce.y;
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

	[Event.Tick.Server]
	public void TickServer()
	{
		var drag = CalculateDrag( Velocity );
		Velocity += drag * Time.Delta;

		Rotation = Rotation.LookAt( Velocity.Normal );

		var newPosition = GetTargetPosition();

		var trace = Trace.Ray( Position, newPosition )
			.Size( Data.Radius )
			.Ignore( this )
			.WithAnyTags( "solid" )
			.WithoutTags( "player", "trigger" )
			.Run();

		Position = trace.EndPosition;

		if ( trace.Hit || trace.StartedSolid )
		{
			if ( trace.Tags.Any( x => Data.ExplodeHitTags.Any( y => x == y ) ) )
			{
				Explode();
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
						PlaySound( Data.BounceSoundPath );
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
	}

	public void Explode()
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
			var distanceMul = 1.0f - Math.Clamp( dist / Data.ExplosionRadius, 0.0f, 1.0f );
			var dmg = Data.ExplosionDamage * distanceMul;
			var force = (forceScale * distanceMul) * ent.PhysicsBody.Mass;
			var forceDir = (targetPos - Position).Normal;

			var damageInfo = DamageInfo.FromExplosion( Position, forceDir * force, dmg )
				.WithWeapon( this )
				.WithAttacker( Owner );

			ent.TakeDamage( damageInfo );
		}
	}
}
