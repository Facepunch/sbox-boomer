using Facepunch.Boomer.Mechanics;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Boomer.WeaponSystem;

[Prefab]
public partial class Shoot : WeaponComponent
{
	[Prefab] public InputButton FireButton { get; set; } = InputButton.PrimaryAttack;

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


	[Prefab] public float BaseDamage { get; set; }
	[Prefab] public float BulletRange { get; set; }
	[Prefab] public int BulletCount { get; set; }
	[Prefab] public float BulletForce { get; set; }
	[Prefab] public float BulletSize { get; set; }
	[Prefab] public float BulletSpread { get; set; }
	[Prefab] public float FireDelay { get; set; }

	[Prefab, ResourceType( "sound" )]
	public List<string> FireSound { get; set; }

	[Prefab]
	public bool FireSoundOnlyOnStart { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string ActivateSound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string DryFireSound { get; set; }

	[Prefab, Category( "Projectile" ), ResourceType( "ple" )]
	public string Projectile { get; set; }

	[Prefab, Category( "Knockback" )]
	public float KnockbackForce { get; set; }

	[Prefab, Category( "Effects" ), ResourceType( "vpcf" )]
	public string TracerPath { get; set; }

	[Prefab, Category( "Effects" ), ResourceType( "vpcf" )]
	public string ImpactTrailPath { get; set; }

	[Prefab, Category( "Effects" )]
	public bool DisableBulletImpacts { get; set; }

	[Prefab, Category( "Effects" )]
	public bool TracerStartEnd { get; set; }

	public TimeUntil TimeUntilCanFire { get; set; }
	protected Particles TracerParticle { get; set; }
	protected Particles ImpactTrailParticle { get; set; }

	protected Sound? ActiveSound { get; set; }
	protected bool IsFiring { get; set; } = false;

	public void StartFiring( Player player )
	{
		if ( IsFiring ) return;

		IsFiring = true;

		if ( FireSoundOnlyOnStart )
		{
			FireSound.ForEach( x => player.PlaySound( x ) );
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
			if ( TimeSinceActivated > FireDelay )
			{
				Fire( player );
			}

			if ( !Game.IsClient ) return;

			if ( TracerParticle != null || ImpactTrailParticle != null )
			{
				var pos = Weapon.EffectEntity.GetAttachment( "muzzle" ) ?? Weapon.Transform;
				TracerParticle?.SetPosition( 0, pos.Position );

				var tr = Trace.Ray( Player.EyePosition, Player.EyePosition + Player.EyeRotation.Forward * BulletRange )
					.WithAnyTags( "solid", "glass" )
					.Ignore( player )
					.Run();

				TracerParticle?.SetPosition( 1, tr.EndPosition );
				ImpactTrailParticle?.SetPosition( 0, tr.EndPosition + ( tr.Normal * 1f ) );
				ImpactTrailParticle?.SetForward( 0, tr.Normal );
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
			if ( !FireSoundOnlyOnStart )
				FireSound.ForEach( x => player.PlaySound( x ) );

			DoShootEffects( To.Single( player ) );
		}

		if ( Projectile != null )
		{
			if ( Game.IsServer )
				_ = ProjectileData.Create( Projectile, player );
		}
		else
		{
			ShootBullet( BulletSpread, BulletForce, BulletSize, BulletCount, BulletRange );
		}

		if ( KnockbackForce > 0f && !player.Tags.Has( "ducked" ) )
		{
			player.Controller.GetMechanic<WalkMechanic>()
				.ClearGroundEntity();

			player.Controller.Velocity = player.Controller.Velocity.WithZ( 0f );
			player.Controller.Velocity += player.EyeRotation.Backward * KnockbackForce;
		}

		using ( Prediction.Off() )
		{
			if ( Game.IsServer ) return;

			if ( TracerStartEnd && TracerParticle == null && !string.IsNullOrEmpty( TracerPath ) )
			{
				TracerParticle = Particles.Create( TracerPath );
			}

			if ( !string.IsNullOrEmpty( ImpactTrailPath ) && ImpactTrailParticle == null )
			{
				ImpactTrailParticle = Particles.Create( ImpactTrailPath );
			}

			if ( !string.IsNullOrEmpty( ActivateSound ) && ActiveSound == null )
			{
				ActiveSound = Sound.FromEntity( ActivateSound, player );
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
		if ( GetComponent<Ammo>( true ) is Ammo ammo && !ammo.HasEnoughAmmo() ) return false;

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
		if ( eventName == "shoot.fire" || eventName == "secondaryshoot.fire" )
		{
			TimeSinceActivated = 0;
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
			var dist = tr.Distance.Remap( 0, BulletRange, 1, 0.5f ).Clamp( 0.5f, 1f );
			damage *= dist;

			start = tr.EndPosition;
			end = tr.EndPosition + (reflectDir * BulletRange);

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
						var newTrace = DoTraceBullet( newStart, newStart + tr.Direction * BulletRange, radius );
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

			var damage = BaseDamage;

			foreach ( var tr in TraceBullet( Player.AimRay.Position, Player.AimRay.Position + forward * bulletRange, bulletSize, ref damage ) )
			{
				if ( !DisableBulletImpacts )
					tr.Surface.DoBulletImpact( tr );

				if ( !Game.IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Player )
					.WithWeapon( Weapon );

				tr.Entity.TakeDamage( damageInfo );

				if ( !string.IsNullOrEmpty( TracerPath ) && !TracerStartEnd )
				{
					using ( Prediction.Off() )
					{
						var tracer = Particles.Create( TracerPath, tr.StartPosition );
						tracer?.SetPosition( 1, tr.EndPosition );
					}
				}
			}
		}
	}
}
