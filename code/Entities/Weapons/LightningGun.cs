using Boomer.Movement;

[Library( "dm_lightninggun" ), HammerEntity]
[EditorModel( "weapons/rust_shotgun/rust_shotgun.vmdl" )]
[Title( "LightningGun" ), Category( "Weapons" )]
partial class LightningGun : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/lightninggun/w_lightninggun.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/lightninggun/lightninggun.vmdl";

	public override bool CanZoom => true;
	public override float PrimaryRate => 50f;
	public override int Bucket => 5;
	public override AmmoType AmmoType => AmmoType.Lightning;

	[Net, Predicted] private bool IsLightningActive { get; set; }
	[Net, Predicted] private int DamageModifier { get; set; } = 0;

	private Particles LightningEffect { get; set; }
	private Sound LightningSound { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		IsLightningActive = false;
		DamageModifier = 0;

		LightningSound.Stop();
		LightningEffect?.Destroy();
		LightningEffect = null;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( Owner is not BoomerPlayer player ) return;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}

			return;
		}


		player.SetAnimParameter( "b_attack", true );

		ShootEffects();
		ShootBullet( 0.01f, 0.75f, 1f, 5.0f );
	}

	public override void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1 )
	{
		Rand.SetSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var forward = Owner.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 5000f, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !tr.Entity.IsValid() ) continue;

				if ( tr.Entity is BoomerPlayer pl )
					DamageModifier++;
				else
					DamageModifier--;

				DamageModifier = DamageModifier.Clamp( 0, 10 );

				if ( !IsServer ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100f * force, damage * DamageModifier )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	public override void Simulate( Client cl )
	{
		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			IsLightningActive = true;
		}
		else
		{
			IsLightningActive = false;
			DamageModifier = 0;
		}

		base.Simulate( cl );
	}

	[Event.Frame]
	private void OnFrame()
	{
		if ( IsLightningActive )
		{
			if ( LightningEffect == null )
			{
				LightningEffect = Particles.Create( "particles/gameplay/weapons/lightninggun/lightninggun_trace.vpcf" );
				LightningSound = Sound.FromEntity( "lg.beam", this );
				PlaySound( "rl.shoot" );
			}
		}
		else if ( LightningEffect != null )
		{
			LightningSound.Stop();
			LightningEffect?.Destroy();
			LightningEffect = null;
		}

		if ( LightningEffect == null ) 
			return;

		if ( Owner.IsLocalPawn )
		{
		//	DebugOverlay.ScreenText( DamageModifier.ToString() );
		}

		var forward = Owner.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0 * 0.25f;
		forward = forward.Normal;

		var tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + forward * 5000f )
			.UseHitboxes()
			.WithoutTags( "trigger" )
			.Ignore( Owner )
			.Ignore( this )
			.Size( 1.0f )
			.Run();

		var pos = EffectEntity.GetAttachment( "muzzle" ) ?? Transform;

		LightningEffect.SetPosition( 0, pos.Position );
		LightningEffect.SetPosition( 1, tr.EndPosition );
		LightningEffect.SetPosition( 2, new Vector3( DamageModifier * 10f, 0, 0 ) );
	}

	protected override void OnDestroy()
	{
		LightningSound.Stop();
		LightningEffect?.Destroy();
		LightningEffect = null;

		base.OnDestroy();
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();
		CrosshairLastShoot = 0;
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 1 );
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
