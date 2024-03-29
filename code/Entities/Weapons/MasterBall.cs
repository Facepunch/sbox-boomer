﻿using Boomer.UI;

namespace Boomer;

[Library( "boomer_masterball" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/masterball/w_masterball.vmdl" )]
[Title( "MasterBall" ), Category( "Weapons" )]
partial class MasterBall : DeathmatchWeapon
{
	[Net]
	public TimeSince DroppedBall { get; set; }
	public override string ViewModelPath => "models/gameplay/weapons/masterball/masterball.vmdl";

	public override string Crosshair => "ui/crosshair/crosshair001.png";
	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 1.0f;
	public override AmmoType AmmoType => AmmoType.None;
	public override int Bucket => 6;

	[Net]
	public TimeUntil PickupCooldown { get; set; }

	[Net]
	public bool PickedUpOnce { get; set; } = false;

	private Particles BallEffect { get; set; }
	private Particles BallTimer { get; set; }

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

		Game.SetRandomSeed( Time.Tick );

		var forward = Player.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0.1f;
		forward = forward.Normal;

		foreach ( var tr in TraceBullet( Player.EyePosition, Player.EyePosition + forward * 70, 15 ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 32, 75 )
				.UsingTraceResult( tr )
				.WithAttacker( Player )
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

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
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

		if ( BallTimer != null )
			BallTimer.Destroy();
		//BoomerChatBox.AddInformation( To.Everyone, $"{carrier.Client.Name} has the ball!", $"avatar:{carrier.Client.SteamId}" );

	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );

		PhysicsEnabled = true;
		PickupCooldown = 1.5f;
		DroppedBall = 0;

		MasterballHud.NotifyDroppedBall( To.Everyone, dropper.Client.Id );

		BallTimer = Particles.Create( "particles/gameplay/gamemodes/masterball/masterball_b.vpcf", this );
		//BoomerChatBox.AddInformation( To.Everyone, $"{dropper.Client.Name} has dropped the ball!", $"avatar:{dropper.Client.SteamId}" );
	}

	[Event.Tick.Client]
	public void UpdateParticle()
	{
		if ( Game.LocalPawn == Owner && BallEffect != null )
		{
			BallEffect.Destroy();
			BallEffect = null;
		}
		else if ( Game.LocalPawn != Owner && BallEffect == null )
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

				if ( BallTimer != null )
					BallTimer.Destroy();
				
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
