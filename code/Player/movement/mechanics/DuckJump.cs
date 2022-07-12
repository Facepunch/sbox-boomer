
using Sandbox;

namespace Boomer.Movement
{
	class DuckJump : BaseMoveMechanic
	{

		public override string HudName => "Duck Jump";
		public override string HudDescription => $"Press {InputActions.Duck.GetButtonOrigin()}+{InputActions.Jump.GetButtonOrigin()} while moving slowly";

		public override bool AlwaysSimulate => true;
		public override bool TakesOverControl => false;

		public bool IsDuckjumping;

		public float JumpPower => 480f;

		public DuckJump( BoomerController controller )
			: base( controller )
		{

		}

		public override void PreSimulate()
		{
			base.PostSimulate();

			if ( ctrl.GroundEntity == null )
			{
				IsDuckjumping = false;
				return;
			}

			//if ( ctrl.GetMechanic<Slide>().TimeSinceSlide <= 0) return;
			//This controls the time we can LJ during slide. ^^^^ TimeSince start of slide.
			//This also allows for combo jumps in the player can time correctly.

			//if ( !Input.Pressed( InputButton.Jump ) ) return;
			//if ( !Input.Down( InputButton.Duck ) ) return;
			//if ( ctrl.Velocity.WithZ( 0 ).Length >= 130 ) return;
			//Some Reason this made the longjump feel bad.

			if ( InputActions.Jump.Pressed() && InputActions.Duck.Down() && ctrl.Velocity.WithZ( 0 ).Length <= 150 )
			{
				IsDuckjumping = true;

				var flGroundFactor = 1.0f;
				var flMul = JumpPower;
				var startz = ctrl.Velocity.z;
				var jumpPower = startz + flMul * flGroundFactor;

				ctrl.ClearGroundEntity();
				ctrl.Velocity = ctrl.Rotation.Backward * 64 * flGroundFactor;
				ctrl.Velocity = ctrl.Velocity.WithZ( jumpPower );
				ctrl.AddEvent( "jump" );

				DuckJumpEffect();
			}

		}

		private void DuckJumpEffect()
		{
			ctrl.AddEvent( "jump" );

			if ( !ctrl.Pawn.IsServer ) return;
			using var _ = Prediction.Off();

			Sound.FromWorld( "player.duckjump", ctrl.Pawn.Position );

		}

	}
}
