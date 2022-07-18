
using Sandbox;

namespace Boomer.Movement
{
	class Glide : BaseMoveMechanic
	{

		public float GlideGravity => 20f;
		public bool Gliding { get; set; }

		public override bool TakesOverControl => false;
		public override bool AlwaysSimulate => true;

		private TimeSince tsJumpHold;

		public Glide( BoomerController controller ) : base( controller )
		{
		}

		public override void PreSimulate()
		{
			base.PreSimulate();

			Gliding = false;

			if ( ctrl.GroundEntity != null ) return;
			if ( ctrl.Energy == 0 ) return;
			if ( !InputActions.Jump.Down() )
			{
				tsJumpHold = 0;
				return;
			}
			if ( ctrl.Velocity.z > 0 ) return;
			if( tsJumpHold < .15f ) return;

			Gliding = true;
			ctrl.Energy = (ctrl.Energy - ctrl.EnergyDrain * Time.Delta).Clamp( 0f, ctrl.MaxEnergy );
			ctrl.Velocity = ctrl.Velocity.WithZ( -GlideGravity );
		}
	}
}
