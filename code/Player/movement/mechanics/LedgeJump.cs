
using Sandbox;

namespace Boomer.Movement
{
	class LedgeJump : BaseMoveMechanic
	{

		public float JumpPower => 200f;
		public float VelocityMulti => 1.2f;
		public float MinLedgeHeight => 64;

		private TimeSince timeSinceJump;

		public override bool TakesOverControl => true;

		public LedgeJump( BoomerController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( ctrl.GroundEntity == null ) return false;
			if ( !InputActions.Walk.Down() ) return false;

			var trStart = ctrl.Position + ctrl.Velocity.WithZ(0) * Time.Delta;
			var trEnd = trStart + Vector3.Down * MinLedgeHeight;
			var tr = ctrl.TraceBBox( trStart, trEnd, 1f );

			if ( tr.Hit ) return false;

			timeSinceJump = 0;

			ctrl.ClearGroundEntity();
			ctrl.Velocity *= VelocityMulti;
			ctrl.Velocity = ctrl.Velocity.WithZ( JumpPower );

			return true;
		}

		public override void Simulate()
		{
			base.Simulate();

			ctrl.Move();

			if ( timeSinceJump < .3f )
				return;

			IsActive = false;
		}

	}
}
