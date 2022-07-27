﻿public partial class DeathmatchWeapon : BaseWeapon, IRespawnableEntity
{
	public virtual AmmoType AmmoType => AmmoType.Pistol;
	public virtual int Bucket => 1;
	public virtual int BucketWeight => 100;
	public virtual int MoveSpeed => Zoomed ? 150 : 350;

	public virtual int Order => (Bucket * 10000) + BucketWeight;

	// We'll store how much ammo has been taken otherwise people would just drop and pick up to dupe ammo 
	public int PickupAmmo { get; set; } = 0;

	// How much ammo this weapon should start with, and give to its first Owner
	public virtual int StartingAmmo => 0;

	public virtual bool CanZoom => false;
	public virtual float ZoomedFov => 20;
	public virtual float ZoomedViewmodelFov => 40;
	public virtual bool GivesAirshotAward => false;

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }
	[Net, Predicted]
	public bool Zoomed { get; set; }

	public PickupTrigger PickupTrigger { get; protected set; }

	protected BoomerPlayer Player => Owner as BoomerPlayer;

	public int AvailableAmmo()
	{
		var owner = Owner as BoomerPlayer;
		if ( owner == null ) return 0;
		return owner.AmmoCount( AmmoType );
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );

		PickupTrigger = new PickupTrigger();
		PickupTrigger.Parent = this;
		PickupTrigger.Position = Position;

		PickupAmmo = StartingAmmo;
	}

	public override void Reload()
	{
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.25f )
			return;

		Zoomed = CanZoom && Input.Down( InputButton.SecondaryAttack );

		base.Simulate( owner );
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );

		// TODO - player third person model reload
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
	
		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;



	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1 )
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
					CreateTracerEffect( tr.EndPosition );
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
	public void CreateTracerEffect( Vector3 hitPosition )
	{
		// get the muzzle position on our effect entity - either viewmodel or world model
		var pos = EffectEntity.GetAttachment( "muzzle" ) ?? Transform;

		var system = Particles.Create( "particles/tracer.standard.vpcf" );
		system?.SetPosition( 0, pos.Position );
		system?.SetPosition( 1, hitPosition );
	}

	public bool TakeAmmo( int amount )
	{
		
		return Player.TakeAmmo( AmmoType, amount ) > 0;
	}

	[ClientRpc]
	public virtual void DryFire()
	{
		PlaySound( "dm.dryfire" );
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new DmViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel( ViewModelPath );
		ViewModelEntity.SetAnimParameter( "deploy", true );
	}

	public override void CreateHudElements()
	{
		if ( Local.Hud == null ) return;
	}

	public bool IsUsable()
	{
		if ( AmmoType == AmmoType.None ) return true;
		return AvailableAmmo() > 0;
	}

	public override void OnCarryStart( Entity carrier )
	{
		base.OnCarryStart( carrier );

		if ( PickupTrigger.IsValid() )
		{
			PickupTrigger.EnableTouch = false;
		}
	}

	//public override void OnCarryDrop( Entity dropper )
	//{
	//	base.OnCarryDrop( dropper );

	//	if ( PickupTrigger.IsValid() )
	//	{
	//		PickupTrigger.EnableTouch = true;
	//	}
	//}

	protected TimeSince CrosshairLastShoot { get; set; }
	protected TimeSince CrosshairLastReload { get; set; }

	public virtual void RenderHud( in Vector2 screensize )
	{
		var center = screensize * 0.5f;

		if ( AvailableAmmo() == 0 )
			CrosshairLastReload = 0;

		RenderCrosshair( center, CrosshairLastShoot.Relative, CrosshairLastReload.Relative );
	}

	private const float FoV = 100;
	private const float VMFoV = 45;

	private float CurrentFoV = FoV;
	private float CurrentVMFoV = VMFoV;
	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		var targetVMFoV = Zoomed ? ZoomedViewmodelFov : 45f;
		var targetFoV = Zoomed ? ZoomedFov : 90f;
		CurrentFoV = CurrentFoV.LerpTo( targetFoV, 15f * Time.Delta );
		CurrentVMFoV = CurrentVMFoV.LerpTo( targetVMFoV, 15f * Time.Delta );

		camSetup.FieldOfView = CurrentFoV;
		camSetup.ViewModel.FieldOfView = CurrentVMFoV;
	}

	public override void BuildInput( InputBuilder owner )
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, CurrentFoV / FoV );
		}
	}

	public virtual void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload ) { }

}
