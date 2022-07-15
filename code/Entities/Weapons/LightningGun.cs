﻿using Boomer.Movement;

[Library( "dm_lightninggun" ), HammerEntity]
[EditorModel( "weapons/rust_shotgun/rust_shotgun.vmdl" )]
[Title( "LightningGun" ), Category( "Weapons" )]
partial class LightningGun : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "weapons/rust_shotgun/rust_shotgun.vmdl" );
	public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";

	public override float PrimaryRate => 50f;
	public override int Bucket => 5;
	public override AmmoType AmmoType => AmmoType.Lightning;

	Particles LightningEffect;

	Sound LightningSound;
	public override int MoveSpeed => Zoomed ? 150 : base.MoveSpeed;

	[Net, Predicted]
	public bool Zoomed { get; set; }

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
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		
		//
		// Shoot the bullets
		//
		ShootBullet( 0.01f, 1.5f, 10.0f, 2.0f );
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

				//if ( tr.Distance > 200 )
				//{
				//	var pos = EffectEntity.GetAttachment( "muzzle" ) ?? Transform;
				//	var tracer = Particles.Create( LightningEffect, pos.Position );
				//	tracer.SetPosition( 1, tr.EndPosition );

				//	//CreateTracerEffect( tr.EndPosition );
				//}

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

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		
		Zoomed = Input.Down( InputButton.SecondaryAttack );
		
		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			var forward = Owner.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0 * 0.25f;
			forward = forward.Normal;

			var tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + forward * 5000 ).UseHitboxes()
				//.HitLayer( CollisionLayer.Water, !InWater )
				.Ignore( Owner )
				.Ignore( this )
				.Size( 1.0f )
				.Run();


			var pos = EffectEntity.GetAttachment( "muzzle" ) ?? Transform;

			if ( LightningEffect == null )
			{
				PlaySound( "rl.shoot" );
				LightningEffect = Particles.Create( "particles/gameplay/weapons/lightninggun/lightninggun_trace.vpcf" );
				LightningSound = Sound.FromEntity( "lg.beam", this );
			}
			using ( Prediction.Off() )
			{
				LightningEffect.SetPosition( 0, pos.Position );
				LightningEffect.SetPosition( 1, tr.EndPosition );
			}
		}
		else
		{
			LightningSound.Stop();
			LightningEffect?.Destroy();
			LightningEffect = null;
		}

	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		if ( Zoomed )
		{
			camSetup.FieldOfView = 20;
			camSetup.ViewModel.FieldOfView = 40;
		}
	}

	public override void BuildInput( InputBuilder owner )
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();
		CrosshairLastShoot = 0;
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
		draw.Color = color.WithAlpha( 0.2f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );

		// outer lines
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.4f, 0.0f ) );
			var length = 10.0f;
			var gap = 40.0f + shootEase * 50.0f;

			gap -= zoomFactor * 20.0f;


			draw.Line( 0, center + Vector2.Up * gap, length, center + Vector2.Up * (gap + length) );
			draw.Line( 0, center - Vector2.Up * gap, length, center - Vector2.Up * (gap + length) );

			draw.Color = draw.Color.WithAlpha( draw.Color.a * zoomFactor );

			for ( int i = 0; i < 4; i++ )
			{
				gap += 40.0f;

				draw.Line( 0, center - Vector2.Left * gap, length, center - Vector2.Left * (gap + length) );
				draw.Line( 0, center + Vector2.Left * gap, length, center + Vector2.Left * (gap + length) );

				draw.Color = draw.Color.WithAlpha( draw.Color.a * 0.5f );
			}
		}
	}
}