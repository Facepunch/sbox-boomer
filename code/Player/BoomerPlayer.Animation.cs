﻿using Sandbox;

namespace Boomer;

public partial class BoomerPlayer
{
	private Entity LastWeaponEntity { get; set; }

	protected void SimulateAnimation()
	{
		if ( Controller is null ) return;

		var turnSpeed = 0.02f;
		Rotation rotation;

		// If we're a bot, spin us around 180 degrees.
		if ( Client.IsBot )
			rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
		else
			rotation = ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, idealRotation, Controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle );

		var animHelper = new CitizenAnimationHelper( this );

		animHelper.WithWishVelocity( Controller.WishVelocity );
		animHelper.WithVelocity( Velocity );
		animHelper.WithLookAt( EyePosition + rotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, Controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && Client.IsValid()) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = Controller.HasTag( "sitting" );
		animHelper.IsNoclipping = Controller.HasTag( "noclip" );
		animHelper.IsClimbing = Controller.HasTag( "climbing" );
		animHelper.IsSwimming = false;
		animHelper.IsWeaponLowered = false;

		if ( Controller.HasEvent( "jump" ) ) animHelper.TriggerJump();
		if ( ActiveChild != LastWeaponEntity ) animHelper.TriggerDeploy();

		if ( ActiveChild is DeathmatchWeapon weapon )
		{
			weapon.SimulateAnimator( animHelper );
		}
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}

		LastWeaponEntity = ActiveChild;
	}
}
