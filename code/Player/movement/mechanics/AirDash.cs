
using Sandbox;

namespace Boomer.Movement
{
	class AirDash : BaseMoveMechanic
	{

		public override string HudName => "Air Dash";
		public override string HudDescription => $"Press {InputActions.Walk.GetButtonOrigin()} in air";

		public override bool AlwaysSimulate => true;
		public override bool TakesOverControl => false;

		public int AmountOfDash { get; private set; }

		public bool IsAirDashing;

		private bool CanDash;

		private TimeSince LastAirDash;

		public AirDash( BoomerController controller )
			: base( controller )
		{

		}

		public override void PreSimulate()
		{
			base.PostSimulate();

			if ( ctrl.GroundEntity != null && ctrl.DashCount >= 0 )
			{
				CanDash = true;
				IsAirDashing = false;
				AmountOfDash = 0;
				return;
			}
			if ( ctrl.DashCount <= 1 )
			{
				if ( LastAirDash > 1 )
				{
					LastAirDash = 0;
					if ( ctrl.GroundEntity != null )
					{
						ctrl.DashCount = 2;
						
						if ( Host.IsServer || !ctrl.Pawn.IsLocalPawn ) return;
						Sound.FromScreen( "charge_added" ).SetVolume( .1f );
					}

				}
			}
			//if ( ctrl.GetMechanic<Slide>().TimeSinceSlide >= 0.20 ) return;
			//This controls the time we can LJ during slide. ^^^^ TimeSince start of slide.
			//This also allows for combo jumps in the player can time correctly.

			//if ( !Input.Pressed( InputButton.Jump ) ) return;
			//if ( !Input.Down( InputButton.Duck ) ) return;
			//if ( ctrl.Velocity.WithZ( 0 ).Length >= 130 ) return;
			//Some Reason this made the longjump feel bad.

			var result = new Vector3( Input.Forward, Input.Left, 0 ).Normal;
			result *= Input.Rotation;

			if ( InputActions.Walk.Pressed() && CanDash == true )
			{
				if ( ctrl.DashCount == 0 )
				{
					IsAirDashing = true;
					CanDash = false;
					return;
				}

				ctrl.DashCount--;

				float flGroundFactor = 1.0f;
				float flMul = 100f * 1.8f;
				float forMul = 585f * 1.4f;
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

			if ( Host.IsServer || !ctrl.Pawn.IsLocalPawn ) return;

			Particles.Create( "particles/gameplay/screeneffects/dash/ss_dash.vpcf", ctrl.Pawn );
			Sound.FromWorld( "jump.double", ctrl.Pawn.Position );
		}

	}
}
