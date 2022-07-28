
using Sandbox;

namespace Boomer.Movement
{
	class LedgeGrab : BaseMoveMechanic
	{
		public override bool TakesOverControl => true;

		public Vector3 LedgeDestination;
		public Vector3 LedgeGrabLocation;
		public Vector3 GrabNormal;

		public Vector3 StartLocation;
		public Vector3 TargetLocation;

		public float TimeUntilNextGrab;

		public float PlayerRadius => 17.0f;
		public float LedgeGrabTime => .35f;

		private Vector3 PreVelocity;
		private TimeSince TimeSinceLedgeGrab;

		public LedgeGrab( BoomerController controller ) : base( controller )
		{
		}

		protected override bool TryActivate()
		{
			if ( InputActions.Jump.Down() )
			{
				if ( TimeUntilNextGrab > Time.Now )
					return false;

				if ( TryGrabUpperLedge() )
				{
					PreVelocity = ctrl.Velocity.WithZ( 0 );
					StartLocation = ctrl.Position;
					TimeSinceLedgeGrab = 0f;
					return true;
				}
			}

			return false;
		}

		public override void Simulate()
		{
			base.Simulate();
			
			ctrl.SetTag( "grabbing_wall" );

			var a = TimeSinceLedgeGrab / LedgeGrabTime;
			a = Easing.EaseIn( a );

			ctrl.Rotation = (-GrabNormal).EulerAngles.WithPitch( 0 ).ToRotation();
			ctrl.Position = Vector3.Lerp( StartLocation, TargetLocation, a );

			if ( TimeSinceLedgeGrab > LedgeGrabTime )
			{
				IsActive = false;
				ctrl.Velocity = PreVelocity;
				return;
			}

			// Climb up
			// Effects
			ctrl.Pawn.PlaySound( "player.slam.land" );

			TargetLocation = LedgeDestination;
		}

		internal bool TryGrabUpperLedge()
		{
			// Need to be on air to check for upper ledge
			if ( ctrl.GroundEntity != null )
				return false;

			var center = ctrl.Position;
			center.z += 48;
			var dest = (center + (ctrl.Pawn.Rotation.Forward.WithZ( 0 ).Normal * 48.0f));
			var destup = (center + (ctrl.Pawn.Rotation.Up * 32.0f));

			// Should be done.
			var trup = ctrl.TraceBBox( center, destup );
			var tr = ctrl.TraceBBox( center, dest );

			if ( trup.Hit ) return false;
			if ( tr.Hit )
			{
				var normal = tr.Normal;
				var destinationTestPos = tr.EndPosition - (normal * PlayerRadius) + (Vector3.Up * 64.0f);
				var originTestPos = tr.EndPosition + (normal * PlayerRadius);

				// Trace again to check if we have a valid ground
				tr = Trace.Ray( destinationTestPos, destinationTestPos - (Vector3.Up * 64.0f) )
					.Ignore( ctrl.Pawn )
					.WithoutTags( "player", "npc" )
					.Radius( 4 )
					.Run();

				if ( tr.Hit )
				{
					// That's a valid position, set our destination pos
					destinationTestPos = tr.EndPosition;
					// Adjust grab position
					originTestPos = originTestPos.WithZ( destinationTestPos.z - 64.0f );

					// Then check if we have enough room to climb

					tr = Trace.Ray( destinationTestPos + (Vector3.Up * PlayerRadius + 1.0f), destinationTestPos + (Vector3.Up * 64.0f) )
						.Ignore( ctrl.Pawn )
						.Radius( PlayerRadius )
						.Run();
					
					if ( tr.Hit )
					{
						// We can't climb
						return false;
					}
					else
					{
						// Yeah, we can climb
						LedgeDestination = destinationTestPos;
						LedgeGrabLocation = originTestPos;
						GrabNormal = normal;
						TimeUntilNextGrab = Time.Now + 1.5f;

						//Default bottom ledge to grab
						TargetLocation = LedgeGrabLocation;

						//Effects
						ctrl.Pawn.PlaySound( "rail.slide.start" );

						return true;
					}
				}

			}

			return false;
		}

		internal bool TryGrabBottomLedge()
		{
			// Need to be on ground to check for bottom ledge
			if ( ctrl.GroundEntity == null )
				return false;
			return false;
		}


	}
}
