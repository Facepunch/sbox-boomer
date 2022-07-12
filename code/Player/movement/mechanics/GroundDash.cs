
using Sandbox;

namespace Boomer.Movement
{
	class GroundDash : BaseMoveMechanic
	{

		public override string HudName => "Ground Dash";
		public override string HudDescription => $"Press {InputActions.Walk.GetButtonOrigin()} in air";

		public override bool AlwaysSimulate => true;
		public override bool TakesOverControl => false;

		public int AmountOfDash { get; private set; }

		public bool IsAirDashing;

		private bool CanDash;

		private TimeSince LastGroundDash;

		public GroundDash( BoomerController controller )
			: base( controller )
		{

		}

		public override void PreSimulate()
		{
			base.PostSimulate();

			if ( ctrl.GroundEntity != null && ctrl.DashCount >= 0)
			{
				CanDash = true;
				IsAirDashing = false;
			}

			if ( ctrl.GroundEntity != null && ctrl.DashCount <= 1 )
			{
				
				if (LastGroundDash > 1)
				{
					ctrl.IsDashing = false;
					ctrl.DashCount = 2;
					LastGroundDash = 0;
					Sound.FromScreen( "charge_added" ).SetVolume( .1f );

				}
			}
			var result = new Vector3( Input.Forward, Input.Left, 0 );
			result *= Input.Rotation;

			if (ctrl.GroundEntity != null && InputActions.Walk.Pressed() && CanDash == true )
			{

				if ( ctrl.DashCount == 0 )
				{
					IsAirDashing = true;
					CanDash = false;
					return;
				}

				LastGroundDash = 0;

				ctrl.DashCount--;

				ctrl.IsDashing = true;

				float flGroundFactor = 1.75f;
				float flMul = 100f * 1.2f;
				float forMul = 585f * 2.2f;

				if ( result.IsNearlyZero() )
				{
					ctrl.Velocity = ctrl.Rotation.Forward * forMul * flGroundFactor;
					ctrl.Velocity = ctrl.Velocity.WithZ( flMul * flGroundFactor );
					ctrl.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;
				}
				else
				{

					ctrl.Velocity = result * forMul * flGroundFactor;
					ctrl.Velocity = ctrl.Velocity.WithZ( flMul * flGroundFactor );
					ctrl.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;
				}

				DashEffect();
			}

			

		}

		private void DashEffect()
		{
			ctrl.AddEvent( "jump" );

			if ( !ctrl.Pawn.IsServer ) return;
			using var _ = Prediction.Off();
			
			Particles.Create( "particles/gameplay/screeneffects/dash/ss_dash.vpcf", ctrl.Pawn );
			Sound.FromWorld( "player.land1", ctrl.Pawn.Position );
		}

	}
}
