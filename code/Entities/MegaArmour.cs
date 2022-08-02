﻿using Boomer.Movement;

namespace Boomer;

/// <summary>
/// Gives 25 health points.
/// </summary>
[Library( "bm_megaarmour" ), HammerEntity]
[EditorModel( "models/gameplay/mega_armour/mega_armour.vmdl" )]
[Title( "Mega Armour" )]
partial class MegaArmour : AnimatedEntity, IRespawnableEntity
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/mega_armour/mega_armour.vmdl" );

	public int RespawnTime = 240;
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
			if ( pl.Armour >= pl.MaxArmour ) return;

			var newhealth = pl.Armour + 200;

			newhealth = newhealth.Clamp( 0, pl.MaxArmour );

			pl.Armour = newhealth;

			PickEffect( pl );
			PlayPickupSound();

			PickupFeed.OnPickup( To.Single( pl ), $"+Mega Health" );
			ItemRespawn.Taken( this, RespawnTime );

			Delete();
		}
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "megahealth.pickup", Position );
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
