﻿using Boomer.Movement;
/// <summary>
/// Gives 25 health points.
/// </summary>
[Library( "dm_healthkit" ), HammerEntity]
[EditorModel( "models/gameplay/healthkit/healthkit.vmdl" )]
[Title( "Health Kit" )]
partial class HealthKit : AnimatedEntity, IRespawnableEntity
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/healthkit/healthkit.vmdl" );

	public int RespawnTime = 30;
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;

		PhysicsEnabled = false;
		UsePhysicsCollision = true;

		
		Tags.Add( "trigger" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( IsServer )
		{
			if ( other is not BoomerPlayer pl ) return;
			if ( pl.Health >= pl.MaxHealth ) return;

			var newhealth = pl.Health + 15;

			newhealth = newhealth.Clamp( 0, pl.MaxHealth );

			pl.Health = newhealth;

			PickEffect( pl );

			PlayPickupSound();
			PickupFeed.OnPickup( To.Single( pl ), $"+15 Health" );
			ItemRespawn.Taken( this, RespawnTime );
			Delete();
		}
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "dm.item_health", Position );
	}

	private void PickEffect( BoomerPlayer player )
	{
		if ( player.Controller is not BoomerController ctrl ) 
		return;

		if ( Host.IsServer || !player.IsLocalPawn )
		return;

		Particles.Create( "particles/gameplay/screeneffects/healthpickup/ss_healthpickup.vpcf",ctrl.Pawn);
	}
}
