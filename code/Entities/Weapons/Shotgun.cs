using Boomer.Movement;

[Library( "dm_shotgun" ), HammerEntity]
[EditorModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" )]
[Title( "Shotgun" ), Category( "Weapons" )]
partial class Shotgun : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/shotgun/w_shotgun.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/shotgun/shotgun.vmdl";
	public override float PrimaryRate => 1.2f;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int Bucket => 0;
	public override int BucketWeight => 200;
	public override bool CanZoom => true;
	public override float ZoomedFov => 65;
	public override float ZoomedViewmodelFov => 100;

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
			ShootBullet( 0.025f, 0.3f, 10.0f, 15.0f, 8 );
		}
		else
		{
			ShootBullet( 0.2f, 0.3f, 6.0f, 15.0f, 14 );
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
		draw.Color = color.WithAlpha( 0.4f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f );

		// center
		{
			var shootEase = 1 + Easing.BounceIn( lastAttack.LerpInverse( 0.3f, 0.0f ) );
			draw.Ring( center, 32 * shootEase, 30 * shootEase );
		}

		// center circle
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.1f, 0.0f ) );
			var length = 1.5f + shootEase * 2.0f;
			draw.Circle( center, length );
		}


	}
}
