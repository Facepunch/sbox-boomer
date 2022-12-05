
using Sandbox;

namespace Boomer.Movement
{
	class WallRun : BaseMoveMechanic
	{
		public float WallRunTime => 3f;
		public float WallRunMinHeight => 90f;
		public float WallJumpPower => 268f;
		public float MinWallHeight => 90;
		public float WallClimbSpeed => 200f;
		public float WallClimbTime => 1f;

		public override bool TakesOverControl => true;
		public WallInfo Wall { get; private set; }

		private TimeSince timeSinceWallRun;
		private Vector3 prevWallRunStart;

		public WallRun( BoomerController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( !InputActions.Jump.Down()) return false;
			if ( ctrl.GroundEntity != null ) return false;
			if ( ctrl.Velocity.z > 100 ) return false;
			if ( ctrl.Velocity.z < -150 ) return false;

			var wall = FindRunnableWall();
			if ( wall == null ) return false;

			var startPos = wall.Value.Trace.EndPosition;
			var dist = prevWallRunStart.WithZ( 0 ).Distance( startPos.WithZ( 0 ) );
			// check x dist is a certain amount to avoid wallrunning straight up multiple times
			if ( dist < 50 && timeSinceWallRun < 2f ) return false;

			Wall = wall.Value;
			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			timeSinceWallRun = 0;
			prevWallRunStart = wall.Value.Trace.EndPosition;

			return true;
		}

		public override void Simulate()
		{
			
			if ( !StillWallRunning() )
			{
				IsActive = false;
				return;
			}

			if ( InputActions.Jump.Pressed() && timeSinceWallRun > .1f )
			{
				JumpOffWall();
				IsActive = false;
				return;
			}
			
			var wishVel = ctrl.GetWishVelocity( true );
			var gravity = timeSinceWallRun / WallRunTime * 150f;
			var lookingAtWall = Vector3.Dot( Wall.Normal, wishVel.Normal ) < -.5f;

			if ( lookingAtWall && Player.InputDirection.x > 0 )
			{
				if ( timeSinceWallRun > WallClimbTime )
				{
					IsActive = false;
					return;
				}
				ctrl.Velocity = new Vector3( 0, 0, WallClimbSpeed );
			}
			else
			{
				ctrl.Velocity = ctrl.Velocity.WithZ( -gravity );
			}

			var dest = ctrl.Position + ctrl.Velocity * Time.Delta;
			var pm = ctrl.TraceBBox( ctrl.Position, dest );

			if ( pm.Fraction == 1 )
			{
				ctrl.Position = pm.EndPosition;
				return;
			}
			ctrl.Move();
		}

		private void JumpOffWall()
		{
			var jumpDir = ctrl.EyeRotation.Forward;
			jumpDir.z *= .5f;
			if ( Vector3.Dot( Wall.Normal, jumpDir ) <= -.3f )
				jumpDir = Wall.Normal;

			var jumpVelocity = (jumpDir + Vector3.Up) * WallJumpPower;
			jumpVelocity += jumpDir * ctrl.Velocity.WithZ( 0 ).Length / 2f;

			//_jumpReleased = false;

			ctrl.Velocity = jumpVelocity;
			Wall = default;

			new FallCameraModifier( -jumpVelocity.Length );
		}

		private bool StillWallRunning()
		{
			if ( ctrl.GroundEntity != null )
				return false;

			var wishVel = ctrl.GetWishVelocity( true );

			if ( wishVel.Length.AlmostEqual( 0f ) )
				return false;

			if ( ctrl.Velocity.Length < 1.0f && timeSinceWallRun > .5f )
				return false;

			var trStart = ctrl.Position + Wall.Normal;
			var trEnd = ctrl.Position - Wall.Normal * ctrl.BodyGirth * 2;
			var tr = ctrl.TraceBBox( trStart, trEnd );

			if ( !tr.Hit || tr.Normal != Wall.Normal )
				return false;

			return true;
		}

		private WallInfo? FindRunnableWall()
		{
			var wall = GetWallInfo( ctrl.Rotation.Forward );

			if ( wall == null ) return null;
			if ( wall.Value.Distance > ctrl.BodyGirth ) return null;
			if ( wall.Value.Height < MinWallHeight ) return null;
			if ( !wall.Value.Normal.z.AlmostEqual( 0, .1f ) ) return null;

			return wall;
		}

	}
}
