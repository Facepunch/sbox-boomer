using Sandbox;
using System;

namespace Facepunch.Boomer.WeaponSystem;

public partial class Projectile : ModelEntity
{
	/// <summary>
	/// Projectile data asset
	/// </summary>
	[Net] protected ProjectileData Data { get; set; }

	public Particles ActiveParticle { get; set; }

	public void Initialize( ProjectileData data )
	{
		Data = data;

		Model = Data.CachedModel;
		Transmit = TransmitType.Always;
		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, Data.Radius );

		if ( !string.IsNullOrEmpty( Data.ParticlePath ) )
			ActiveParticle = Particles.Create( Data.ParticlePath, this, true );

		// Async delete
		DeleteAsync( Data.Lifetime );
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
