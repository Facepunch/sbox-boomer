
using Sandbox;

namespace Boomer.Movement
{
	class LongJump : BaseMoveMechanic
	{

		public override string HudName => "Long Jump";
		public override string HudDescription => $"Press {InputActions.Duck.GetButtonOrigin()}+{InputActions.Jump.GetButtonOrigin()} while running";

		public override bool AlwaysSimulate => true;
		public override bool TakesOverControl => false;

		public bool IsLongjumping;

		public LongJump( BoomerController controller )
			: base( controller )
		{

		}

		public override void PreSimulate()
		{
			base.PostSimulate();

			if ( ctrl.GroundEntity == null )
			{
				IsLongjumping = false;
				return;
			}

			if ( ctrl.GetMechanic<Slide>().TimeSinceSlide >= 0.20 ) return;
			//This controls the time we can LJ during slide. ^^^^ TimeSince start of slide.
			//This also allows for combo jumps in the player can time correctly.

			//if ( !Input.Pressed( InputButton.Jump ) ) return;
			//if ( !Input.Down( InputButton.Duck ) ) return;
			//if ( ctrl.Velocity.WithZ( 0 ).Length >= 130 ) return;
			//Some Reason this made the longjump feel bad.

			if ( InputActions.Jump.Pressed() && InputActions.Duck.Down() && ctrl.Velocity.WithZ( 0 ).Length >= 120 )
			{
				IsLongjumping = true;

				float flGroundFactor = 1.0f;
				float flMul = 300f * 1.2f;
				float forMul = 485f * 1.2f;

				ctrl.Velocity = ctrl.Rotation.Forward * forMul * flGroundFactor;
				ctrl.Velocity = ctrl.Velocity.WithZ( flMul * flGroundFactor );
				ctrl.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;

				LongJumpEffect();
			}
		}

		private void LongJumpEffect()
		{
			ctrl.AddEvent( "jump" );

			if ( !ctrl.Pawn.IsServer ) return;
			using var _ = Prediction.Off();

		}

	}
}
