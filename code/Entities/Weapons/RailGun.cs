using Boomer.Movement;

namespace Boomer;

[Library( "boomer_railgun" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/railgun/w_railgun.vmdl" )]
[Title( "RailGun" ), Category( "Weapons" )]
partial class RailGun : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/railgun/w_railgun.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/railgun/railgun.vmdl";
	public override string Crosshair => "ui/crosshair/crosshair056.png";
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

		if ( Owner is BoomerPlayer player && !Input.Down( InputButton.Duck ) )
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
		if ( DeathmatchGame.InstaGib || DeathmatchGame.RailTag )
		{
			ShootBullet( 0.01f, 1.5f, 10000.0f, 15.0f );
		}
		else
		{
			ShootBullet( 0.01f, 1.5f, 80.0f, 15.0f );
		}

	}

	public override void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1 )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Rand.SetSeed( Time.Tick );

		var effectStart = EffectEntity?.GetAttachment( "muzzle" )?.Position ?? Transform.Position;
		var effectEnd = effectStart + Player.EyeRotation.Forward * 5000;

		for ( int i = 0; i < bulletCount; i++ )
		{
			var forward = Player.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Player.EyePosition, Player.EyePosition + forward * 5000, bulletSize ) )
			{
				// Move into the normal by the bullet radius to give us a better chance of making a decal
				var impactTrace = tr;
				impactTrace.EndPosition -= tr.Normal * (bulletSize * 0.5f);
				tr.Surface.DoBulletImpact( impactTrace );

				effectEnd = tr.EndPosition;

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Player )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}

		var tracer = Particles.Create( RailEffect, effectStart );
		tracer?.SetPosition( 1, effectEnd );
		//CreateTracerEffect( tr.EndPosition );
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;

	}

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Shotgun;
	}

	TimeSince timeSinceZoomed;

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		//var draw = Render.Draw2D;

		//if ( Zoomed )
		//	timeSinceZoomed = 0;

		//var zoomFactor = timeSinceZoomed.Relative.LerpInverse( 0.4f, 0 );

		//var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );
		//draw.BlendMode = BlendMode.Lighten;
		//draw.Color = color.WithAlpha( 0.4f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );


		//// center circle
		//{
		//	var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.1f, 0.0f ) );
		//	var length = 4.0f + shootEase * 2.0f;
		//	draw.Circle( center, length );
		//}
	}
}
