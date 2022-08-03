using Boomer.Movement;

namespace Boomer;

[Library( "dm_railgun" ), HammerEntity]
[EditorModel( "weapons/rust_shotgun/rust_shotgun.vmdl" )]
[Title( "RailGun" ), Category( "Weapons" )]
partial class RailGun : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/railgun/w_railgun.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/railgun/railgun.vmdl";

	public AnimatedEntity AnimationOwner => Owner as AnimatedEntity;
	public override bool GivesAirshotAward => true;
	public override bool CanZoom => true;
	public override float PrimaryRate => .55f;
	public override int Bucket => 4;
	public override AmmoType AmmoType => AmmoType.Rails;

	public string RailEffect = "particles/gameplay/weapons/railgun/railgun_trace.vpcf";

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}
			return;
		}

		//
		//Push player back
		//
		float flGroundFactor = 1.0f;
		float flMul = 100f * 1.8f;
		float forMul = 150f * 1.4f;
		
		if ( Owner is BoomerPlayer player )
		{
			player.Velocity += player.EyeRotation.Backward * forMul * flGroundFactor;
			player.Velocity += player.Velocity.WithZ( flMul * flGroundFactor );
			player.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;
		}

		AnimationOwner.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "rl.shoot" );
		PlaySound( "rg.shoot.1" );
		PlaySound( "rg.shoot.2" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.01f, 1.5f, 80.0f, 30.0f );
	}

	public override void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1 )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Rand.SetSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var forward = Owner.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( tr.Distance > 200 )
				{
					var pos = EffectEntity.GetAttachment( "muzzle" ) ?? Transform;
					var tracer = Particles.Create( RailEffect, pos.Position );
					tracer.SetPosition( 1, tr.EndPosition );
					//CreateTracerEffect( tr.EndPosition );
				}

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;

	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}

	TimeSince timeSinceZoomed;

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		if ( Zoomed )
			timeSinceZoomed = 0;

		var zoomFactor = timeSinceZoomed.Relative.LerpInverse( 0.4f, 0 );

		var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );
		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.4f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );


		// center circle
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.1f, 0.0f ) );
			var length = 4.0f + shootEase * 2.0f;
			draw.Circle( center, length );
		}
	}
}
