using Boomer.UI;

namespace Boomer;

[Library( "boomer_masterball" ), HammerEntity]
[EditorModel( "models/dm_crowbar.vmdl" )]
[Title( "MasterBall" ), Category( "Weapons" )]
partial class MasterBall : DeathmatchWeapon
{
	[Net]
	public TimeSince DroppedBall { get; set; }
	public override string ViewModelPath => "models/gameplay/weapons/masterball/masterball.vmdl";

	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 1.0f;
	public override AmmoType AmmoType => AmmoType.None;
	public override int Bucket => 6;

	[Net]
	public TimeUntil PickupCooldown { get; set; }

	[Net]
	public bool PickedUpOnce { get; set; } = false;

	private Particles BallEffect { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger.Delete();

		Transmit = TransmitType.Always;

		SetModel( "models/gameplay/weapons/masterball/w_masterball.vmdl" );

		SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Vector3.Zero, 16f );
		PhysicsEnabled = true;
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack();
	}
	
	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		// woosh sound
		// screen shake
		PlaySound( "dm.crowbar_attack" );

		Rand.SetSeed( Time.Tick );

		var forward = Owner.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0.1f;
		forward = forward.Normal;

		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 70, 15 ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 32, 25 )
				.UsingTraceResult( tr )
				.WithAttacker( Owner )
				.WithWeapon( this );

			tr.Entity.TakeDamage( damageInfo );
		}
		ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );
		ViewModelEntity?.SetAnimParameter( "attack", true );
		ViewModelEntity?.SetAnimParameter( "holdtype_attack", false ? 2 : 1 );
		if ( Owner is BoomerPlayer player )
		{
			player.SetAnimParameter( "b_attack", true );
		}
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 5 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );

		if ( Owner.IsValid() )
		{
			ViewModelEntity?.SetAnimParameter( "b_grounded", Owner.GroundEntity.IsValid() );
			ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );
		}
	}

	public override void OnCarryStart( Entity carrier )
	{
		base.OnCarryStart( carrier );

		if ( carrier is not Player pl ) 
			return;

		pl.Inventory.SetActive( this );
		
		PhysicsEnabled = false;

		PickedUpOnce = true;

		MasterballHud.NotifyHasBall( To.Everyone, carrier.Client.Id );
		//BoomerChatBox.AddInformation( To.Everyone, $"{carrier.Client.Name} has the ball!", $"avatar:{carrier.Client.PlayerId}" );

	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );

		PhysicsEnabled = true;
		PickupCooldown = 1.5f;
		DroppedBall = 0;

		MasterballHud.NotifyDroppedBall( To.Everyone, dropper.Client.Id );
		//BoomerChatBox.AddInformation( To.Everyone, $"{dropper.Client.Name} has dropped the ball!", $"avatar:{dropper.Client.PlayerId}" );
	}

	[Event.Tick.Client]
	public void UpdateParticle()
	{
		if ( Local.Pawn == Owner && BallEffect != null )
		{
			BallEffect.Destroy();
			BallEffect = null;
		}
		else if ( Local.Pawn != Owner && BallEffect == null )
		{
			BallEffect = Particles.Create( "particles/gameplay/gamemodes/masterball/masterball.vpcf", this );
		}
	}

	[Event.Tick.Server]
	private void OnServerTick()
	{
		if ( !Owner.IsValid() )
		{
			if ( DroppedBall > 30 && PickedUpOnce  )
			{
				Position = new Vector3( 0, 0, 2096 );
				PickedUpOnce = false;

				MasterballHud.NotifyBallReset( To.Everyone );
				//BoomerChatBox.AddInformation( To.Everyone, $"Ball has been reset!", $"avatar:{null}" );
			}		
		}
		else
		{
			DroppedBall = 0;
		}

		if ( Owner is not Player pl )
		{
			return;
		}

		if( pl.ActiveChild != this )
		{
			pl.Inventory.Drop( this );
		}
	}

}
