
namespace Boomer.Movement;

class Dash : BaseMoveMechanic
{

	public override string HudName => "Dash";
	public override string HudDescription => $"Press {InputActions.Walk.GetButtonOrigin()}";

	public override bool AlwaysSimulate => true;
	public override bool TakesOverControl => false;
	public TimeSince TimeSinceDash { get; private set; }
	public float DashAlpha => Math.Clamp( TimeSinceDash / .35f, 0, 1f );

	public Dash( BoomerController controller )
		: base( controller )
	{

	}

	public override void PreSimulate()
	{
		base.PostSimulate();
		if ( ctrl.Pawn is BoomerPlayer pl )
		{
			if ( pl.HasTheBall )
				return;
		}
		
		if( TimeSinceDash > 2 && ctrl.DashCount != 2 )
		{
			if ( ctrl.GroundEntity != null )
			{
				ctrl.DashCount = 2;
				if ( ctrl.Pawn.IsLocalPawn )
					Sound.FromScreen( "dashrecharge" ).SetVolume( 1f );
			}
		}

		if ( ctrl.DashCount <= 0 ) 
			return;

		if ( !InputActions.Walk.Pressed() )
			return;

		TimeSinceDash = 0;
		ctrl.DashCount--;

		float flGroundFactor = ctrl.GroundEntity != null ? 1.75f : 1.0f;
		float flMul = 100f * 1.2f;
		float forMul = 485f * 2.2f;

		var dashDirection = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 ).Normal;
		dashDirection *= Player.ViewAngles.ToRotation();

		if ( dashDirection.IsNearlyZero() )
		{
			dashDirection = ctrl.Rotation.Forward;
		}

		ctrl.Velocity = dashDirection * forMul * flGroundFactor;
		ctrl.Velocity = ctrl.Velocity.WithZ( flMul * flGroundFactor );
		ctrl.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;

		DashEffect();
	}

	private void DashEffect()
	{
		ctrl.AddEvent( "jump" );

		if ( Host.IsServer || !ctrl.Pawn.IsLocalPawn ) return;

		Particles.Create( "particles/gameplay/screeneffects/dash/ss_dash.vpcf", ctrl.Pawn );
		Sound.FromWorld( "jump.double", ctrl.Pawn.Position );
	}

}
