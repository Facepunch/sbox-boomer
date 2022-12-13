
using Sandbox;
using System;

namespace Boomer.Movement
{
	class AirMove : BaseMoveMechanic
	{

		public float Gravity => 800.0f;
		public float AirControl => 60.0f;
		public float AirAcceleration => 85.0f;

		public override bool AlwaysSimulate => true;

		private Vector3 velocityAtStart;
		private bool groundedAtStart;

		public AirMove( BoomerController controller )
			: base( controller )
		{

		}

		public override void Simulate()
		{
			if ( ctrl.GroundEntity != null ) return;

			var wishVel = ctrl.GetWishVelocity( true );
			var wishdir = wishVel.Normal;
			var wishspeed = wishVel.Length;

			ctrl.Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );
			ctrl.Velocity += ctrl.BaseVelocity;
			ctrl.Move();
			ctrl.Velocity -= ctrl.BaseVelocity;
		}

		public override void PreSimulate()
		{
			ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			ctrl.Velocity += new Vector3( 0, 0, ctrl.BaseVelocity.z ) * Time.Delta;

			ctrl.BaseVelocity = ctrl.BaseVelocity.WithZ( 0 );

			velocityAtStart = ctrl.Velocity;
			groundedAtStart = ctrl.GroundEntity != null;
		}

		public override void PostSimulate()
		{
			ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			if ( ctrl.GroundEntity != null && !groundedAtStart )
				DoFallDamage();

			if ( ctrl.GroundEntity == null && groundedAtStart )
				new FallCameraModifier( -150, 1.5f );
		}

		private void DoFallDamage()
		{
			var fallSpeed = Math.Abs( velocityAtStart.z );

			if ( Game.IsClient )
			{
				new FallCameraModifier( fallSpeed );
			}

			if ( fallSpeed < 500 )
				return;

			//var walk = ctrl.GetMechanic<Walk>();

			//var willSlide = Input.Down( InputButton.Duck ) && ctrl.Velocity.WithZ( 0 ).Length > duck.SlideThreshold;
			var fallSpeedMaxLoss = 3000;
			var a = 1f - MathF.Min( fallSpeed / fallSpeedMaxLoss, 1 );

			ctrl.Velocity = ctrl.Velocity.ClampLength( ctrl.Velocity.Length * a );
			//walk.Momentum *= a;
		}

	}
}
