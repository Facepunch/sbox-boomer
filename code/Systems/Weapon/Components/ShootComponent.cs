using Facepunch.Boomer.Mechanics;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Boomer.WeaponSystem;

public partial class Shoot : WeaponComponent, ISingletonComponent
{
	protected override bool UseLagCompensation => true;
	public virtual InputButton FireButton => InputButton.PrimaryAttack;
	public ComponentData Data => Weapon.WeaponData.Shoot;

	/// <summary>
	/// When penetrating a surface, this is the trace increment amount.
	/// </summary>
	protected float PenetrationIncrementAmount => 15f;

	/// <summary>
	/// How many increments?
	/// </summary>
	protected int PenetrationMaxSteps => 2;

	/// <summary>
	/// How many ricochet hits until we stop traversing
	/// </summary>
	protected float MaxAmtOfHits => 2f;

	/// <summary>
	/// Maximum angle in degrees for ricochet to be possible
	/// </summary>
	protected float MaxRicochetAngle => 45f;

	public TimeUntil TimeUntilCanFire { get; set; }
	protected Particles TracerParticle { get; set; }
	protected Particles ImpactTrailParticle { get; set; }

	protected Sound? ActiveSound { get; set; }
	protected bool IsFiring { get; set; } = false;

	public void StartFiring( Player player )
	{
		if ( IsFiring ) return;

		IsFiring = true;

		if ( Data.FireSoundOnlyOnStart )
		{
			Data.FireSound.ForEach( x => player.PlaySound( x ) );
		}
	}

	public void StopFiring( Player player )
	{
		IsFiring = false;

		ActiveSound?.Stop();
		ActiveSound = null;

		TracerParticle?.Destroy();
		TracerParticle = null;

		ImpactTrailParticle?.Destroy();
		ImpactTrailParticle = null;
	}

	protected override void OnDeactivate()
	{
		StopFiring( Player );
	}

	public override void Simulate( IClient cl, Player player )
	{
		base.Simulate( cl, player );

		if ( WishesToFire( player ) )
		{
			if ( CanFire( player ) )
			{
				StartFiring( player );
			}
		}
		else
		{
			if ( IsFiring )
			{
				StopFiring( player );
			}
		}

		if ( IsFiring )
		{
			if ( TimeSinceActivated > Data.FireDelay )
			{
				Fire( player );
			}

			if ( !Game.IsClient ) return;

			if ( TracerParticle != null || ImpactTrailParticle != null )
			{
				var pos = Weapon.EffectEntity.GetAttachment( "muzzle" ) ?? Weapon.Transform;
				TracerParticle?.SetPosition( 0, pos.Position );

				var tr = Trace.Ray( Player.AimRay.Position, Player.AimRay.Position + Player.AimRay.Forward * Data.BulletRange ).Ignore( player ).Run();

				TracerParticle?.SetPosition( 1, tr.EndPosition );
				ImpactTrailParticle?.SetPosition( 0, tr.EndPosition );
			}
		}
	}

	void Fire( Player player )
	{
		TimeSinceActivated = 0;

		player?.SetAnimParameter( "b_attack", true );

		RunGameEvent( $"{Name}.fire" );

		// Send clientside effects to the player.
		if ( Game.IsServer )
		{
			if ( !Data.FireSoundOnlyOnStart )
				Data.FireSound.ForEach( x => player.PlaySound( x ) );

			DoShootEffects( To.Single( player ) );
		}

		if ( Data.Projectile != null )
		{
			if ( Game.IsServer )
				_ = ProjectileData.Create( Data.Projectile, player );
		}
		else
		{
			ShootBullet( Data.BulletSpread, Data.BulletForce, Data.BulletSize, Data.BulletCount, Data.BulletRange );
		}

		if ( Data.KnockbackForce > 0f && !player.Tags.Has( "ducked" ) )
		{
			player.Controller.GetMechanic<WalkMechanic>()
				.ClearGroundEntity();

			player.Controller.Velocity = player.Controller.Velocity.WithZ( 0f );
			player.Controller.Velocity += player.EyeRotation.Backward * Data.KnockbackForce;
		}

		using ( Prediction.Off() )
		{
			if ( Game.IsServer ) return;

			if ( Data.TracerStartEnd && TracerParticle == null && !string.IsNullOrEmpty( Data.TracerPath ) )
			{
				TracerParticle = Particles.Create( Data.TracerPath );
			}

			if ( !string.IsNullOrEmpty( Data.ImpactTrailPath ) && ImpactTrailParticle == null )
			{
				ImpactTrailParticle = Particles.Create( Data.ImpactTrailPath );
			}

			if ( !string.IsNullOrEmpty( Data.ActivateSound ) && ActiveSound == null )
			{
				ActiveSound = Sound.FromEntity( Data.ActivateSound, player );
			}
		}
	}

	protected bool WishesToFire( Player player )
	{
		if ( Input.Down( FireButton ) ) return true;
		return false;
	}

	protected bool CanFire( Player player )
	{
		if ( TimeUntilCanFire > 0 ) return false;
		if ( Weapon.Tags.Has( "reloading" ) ) return false;
		if ( GetComponent<Ammo>() is Ammo ammo && !ammo.HasEnoughAmmo() ) return false;

		return true;
	}

	public override void OnGameEvent( string eventName )
	{
		if ( eventName == "sprint.stop" )
		{
			TimeUntilCanFire = 0.2f;
		}
		if ( eventName == "aim.start" )
		{
			TimeUntilCanFire = 0.15f;
		}
	}

	[ClientRpc]
	public static void DoShootEffects()
	{
		Game.AssertClient();
		WeaponViewModel.Current?.SetAnimParameter( "fire", true );
	}

	protected TraceResult DoTraceBullet( Vector3 start, Vector3 end, float radius )
	{
		return Trace.Ray( start, end )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "glass" )
			.Ignore( Entity )
			.Size( radius )
			.Run();
	}

	protected bool ShouldPenetrate()
	{
		return true;
	}

	protected bool ShouldBulletContinue( TraceResult tr, float angle, ref float damage )
	{
		float maxAngle = MaxRicochetAngle;

		if ( angle > maxAngle )
			return false;

		return true;
	}

	protected Vector3 CalculateRicochetDirection( TraceResult tr, ref float hits )
	{
		if ( tr.Entity is GlassShard )
		{
			// Allow us to do another hit
			hits--;
			return tr.Direction;
		}

		return Vector3.Reflect( tr.Direction, tr.Normal ).Normal;
	}

	public IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius, ref float damage )
	{
		float curHits = 0;
		var hits = new List<TraceResult>();

		while ( curHits < MaxAmtOfHits )
		{
			curHits++;

			var tr = DoTraceBullet( start, end, radius );
			if ( tr.Hit )
			{
				if ( curHits > 1 )
					damage *= 0.5f;
				hits.Add( tr );
			}

			var reflectDir = CalculateRicochetDirection( tr, ref curHits );
			var angle = reflectDir.Angle( tr.Direction );
			var dist = tr.Distance.Remap( 0, Data.BulletRange, 1, 0.5f ).Clamp( 0.5f, 1f );
			damage *= dist;

			start = tr.EndPosition;
			end = tr.EndPosition + (reflectDir * Data.BulletRange);

			var didPenetrate = false;
			if ( ShouldPenetrate() )
			{
				// Look for penetration
				var forwardStep = 0f;

				while ( forwardStep < PenetrationMaxSteps )
				{
					forwardStep++;

					var penStart = tr.EndPosition + tr.Direction * (forwardStep * PenetrationIncrementAmount);
					var penEnd = tr.EndPosition + tr.Direction * (forwardStep + 1 * PenetrationIncrementAmount);

					var penTrace = DoTraceBullet( penStart, penEnd, radius );
					if ( !penTrace.StartedSolid )
					{
						var newStart = penTrace.EndPosition;
						var newTrace = DoTraceBullet( newStart, newStart + tr.Direction * Data.BulletRange, radius );
						hits.Add( newTrace );
						didPenetrate = true;
						break;
					}
				}
			}

			if ( didPenetrate || !ShouldBulletContinue( tr, angle, ref damage ) )
				break;
		}

		return hits;
	}

	public void ShootBullet( float spread, float force, float bulletSize, int bulletCount = 1, float bulletRange = 5000f )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Game.SetRandomSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var rot = Rotation.LookAt( Player.AimRay.Forward );

			var forward = rot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			var damage = Data.BaseDamage;

			foreach ( var tr in TraceBullet( Player.AimRay.Position, Player.AimRay.Position + forward * bulletRange, bulletSize, ref damage ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !Game.IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Player )
					.WithWeapon( Weapon );

				tr.Entity.TakeDamage( damageInfo );

				if ( !string.IsNullOrEmpty( Data.TracerPath ) && !Data.TracerStartEnd )
				{
					using ( Prediction.Off() )
					{
						var tracer = Particles.Create( Data.TracerPath, tr.StartPosition );
						tracer?.SetPosition( 1, tr.EndPosition );
					}
				}
			}
		}
	}

	/// <summary>
	/// Data asset information.
	/// </summary>
	public struct ComponentData
	{
		public float BaseDamage { get; set; }
		public float BulletRange { get; set; }
		public int BulletCount { get; set; }
		public float BulletForce { get; set; }
		public float BulletSize { get; set; }
		public float BulletSpread { get; set; }
		public float FireDelay { get; set; }

		[ResourceType( "sound" )]
		public List<string> FireSound { get; set; }

		public bool FireSoundOnlyOnStart { get; set; }

		[ResourceType( "sound" )]
		public string ActivateSound { get; set; }

		[ResourceType( "sound" )]
		public string DryFireSound { get; set; }

		[Category( "Projectile" ), ResourceType( "ple" )]
		public string Projectile { get; set; }

		[Category( "Knockback" )]
		public float KnockbackForce { get; set; }

		[Category( "Effects" ), ResourceType( "vpcf" )]
		public string TracerPath { get; set; }

		[Category( "Effects" ), ResourceType( "vpcf" )]
		public string ImpactTrailPath { get; set; }

		[Category( "Effects" )]
		public bool TracerStartEnd { get; set; }

		public void Precache()
		{
			if ( FireSound.Count > 0 )
			{
				FireSound.ForEach( Sandbox.Precache.Add );
			}

			if ( !string.IsNullOrEmpty( DryFireSound ) ) Sandbox.Precache.Add( DryFireSound );
			if ( !string.IsNullOrEmpty( TracerPath ) ) Sandbox.Precache.Add( TracerPath );
			if ( !string.IsNullOrEmpty( ImpactTrailPath ) ) Sandbox.Precache.Add( ImpactTrailPath );
		}
	}
}
