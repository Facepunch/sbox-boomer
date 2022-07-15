using Boomer.Movement;

[Library( "dm_shotgun" ), HammerEntity]
[EditorModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" )]
[Title( "Shotgun" ), Category( "Weapons" )]
partial class Shotgun : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 1.2f;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int Bucket => 0;
	public override int BucketWeight => 200;
	public override bool CanZoom => true;

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
			return;
		}

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "rust_pumpshotgun.shoot" );
		if ( Zoomed )
		{
			ShootBullet( 0.025f, 0.3f, 20.0f, 2.0f, 4 );
		}
		else
		{
			ShootBullet( 0.2f, 0.3f, 15.0f, 2.0f, 4 );
		}
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		if ( Zoomed )
		{
			camSetup.FieldOfView -= 30f;
			camSetup.ViewModel.FieldOfView = 100;
		}
	}

	public override void BuildInput( InputBuilder owner )
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, .65f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimParameter( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );
		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.2f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f );

		// center
		{
			var shootEase = 1 + Easing.BounceIn( lastAttack.LerpInverse( 0.3f, 0.0f ) );
			draw.Ring( center, 15 * shootEase, 14 * shootEase );
		}

		// outer lines
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.4f, 0.0f ) );
			var length = 30.0f;
			var gap = 30.0f + shootEase * 50.0f;
			var thickness = 4.0f;
			var extraAngle = 30 * shootEase;

			draw.CircleEx( center + Vector2.Right * gap, length, length - thickness, 32, 220, 320 );
			draw.CircleEx( center - Vector2.Right * gap, length, length - thickness, 32, 40, 140 );

			draw.Color = draw.Color.WithAlpha( 0.1f );
			draw.CircleEx( center + Vector2.Right * gap * 2.6f, length, length - thickness * 0.5f, 32, 220, 320 );
			draw.CircleEx( center - Vector2.Right * gap * 2.6f, length, length - thickness * 0.5f, 32, 40, 140 );
		}
	}
}
