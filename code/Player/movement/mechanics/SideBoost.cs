
using Sandbox;

namespace Boomer.Movement
{
	class SideBoost : BaseMoveMechanic
	{

		public override bool TakesOverControl => true;

		private TimeSince timeSinceSideBoost;
		private TimeSince timeSinceMoveLeft;
		private TimeSince timeSinceMoveRight;

		public SideBoost( BoomerController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( timeSinceSideBoost < .8f )
				return false;

			if ( ctrl.GroundEntity == null )
				return false;

			var moveLeft = Input.Pressed( InputButton.Left );
			var moveRight = Input.Pressed( InputButton.Right );

			if ( !moveLeft && !moveRight )
				return false;

			var timeSince = 0f;

			if ( moveLeft )
			{
				timeSince = timeSinceMoveLeft;
				timeSinceMoveLeft = 0;
				timeSinceMoveRight = 100f;
			}

			if ( moveRight )
			{
				timeSince = timeSinceMoveRight;
				timeSinceMoveRight = 0;
				timeSinceMoveLeft = 100f;
			}

			if ( timeSince > .3f )
				return false;

			ctrl.ClearGroundEntity();

			var boostVelocity = new Vector3( 0, Player.InputDirection.y, 0 ) * Player.ViewAngles.ToRotation();
			boostVelocity *= 325f;
			boostVelocity += Vector3.Up * 130;

			ctrl.Velocity += boostVelocity;
			timeSinceSideBoost = 0;

			return true;
		}

		public override void Simulate()
		{
			base.Simulate();

			ctrl.Move();

			if ( timeSinceSideBoost < .15f )
				return;

			IsActive = false;
		}

	}
}
