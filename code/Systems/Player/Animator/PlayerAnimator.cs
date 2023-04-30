using Facepunch.Boomer.Mechanics;
using Sandbox;
using System;

namespace Facepunch.Boomer;

public partial class PlayerAnimator : EntityComponent<Player>, ISingletonComponent
{
	public virtual void Simulate( IClient cl )
	{
		var player = Entity;
		var controller = player.Controller;
		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( player );

		animHelper.WithWishVelocity( controller.GetWishVelocity() );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( player.EyePosition + player.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = player.EyeRotation;
		animHelper.FootShuffle = 0f;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, 1 - controller.CurrentEyeHeight.Remap( 30, 72, 0, 1 ).Clamp( 0, 1 ), Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && cl.IsValid()) ? cl.Voice.LastHeard < 0.5f ? cl.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = controller.GroundEntity != null;
		//animHelper.IsSitting = controller.HasTag( "sitting" );
		//animHelper.IsNoclipping = controller.HasTag( "noclip" );
		//animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = player.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		player.SetAnimParameter( "special_movement_states", player.Controller.GetMechanic<SlideMechanic>().IsActive ? 3 : 0 );
		
		var weapon = player.ActiveWeapon;
		if ( weapon.IsValid() )
		{
			player.SetAnimParameter( "holdtype", (int)weapon.HoldType );
			player.SetAnimParameter( "holdtype_handedness", (int)weapon.Handedness );
			player.SetAnimParameter( "holdtype_pose", (int)weapon.HoldTypePose );
		}
	}

	public virtual void FrameSimulate( IClient cl )
	{
		//
	}
}
