
namespace Boomer.Movement;

class Slide : BaseMoveMechanic
{

	public float StopSpeed => 75f;
	public float Friction => 1.5f;
	public float EndSlideSpeed => 120f;
	public float StartSlideSpeed => 240f;
	public float SlideBoost => 475f;
	public TimeSince TimeSinceSlide { get; set; }
	public bool Sliding { get; private set; }

	public override float EyePosMultiplier => .45f;
	public override bool TakesOverControl => true;

	private TimeSince timeSinceLastSlide = 0;

	public Slide( BoomerController ctrl )
		: base( ctrl )
	{

	}

	protected override bool TryActivate()
	{
		if ( InputActions.Jump.Down() ) return false;
		if ( !InputActions.Duck.Down() ) return false;
		if ( ctrl.GroundEntity == null ) return false;
		if ( ctrl.Velocity.WithZ( 0 ).Length < StartSlideSpeed ) return false;
		if ( timeSinceLastSlide < .5 ) return false;

		TimeSinceSlide = 0;

		var len = ctrl.Velocity.WithZ( 0 ).Length;
		var newLen = len + SlideBoost;
		ctrl.Velocity *= newLen / len;
		ctrl.SetTag( "skidding" );
		Sound.FromEntity( "slide.stop", ctrl.Pawn );

		new FallCameraModifier( -300 );

		return true;
	}

	public override void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale = 1 )
	{
		base.UpdateBBox( ref mins, ref maxs, scale );

		maxs = maxs.WithZ( 20 * scale );
	}

	public override float GetWishSpeed()
	{
		return 100;
	}

	public override void PreSimulate()
	{

		if ( !StillSliding() )
		{
			IsActive = false;
			return;
		}

		if ( ctrl.GroundNormal.z < 1 )
		{
			var slopeDir = Vector3.Cross( Vector3.Up, Vector3.Cross( Vector3.Up, ctrl.GroundNormal ) );
			var dot = Vector3.Dot( ctrl.Velocity.Normal, slopeDir );
			var slopeForward = Vector3.Cross( ctrl.GroundNormal, ctrl.Pawn.Rotation.Right );
			var spdGain = 50f.LerpTo( 350f, 1f - ctrl.GroundNormal.z );

			if ( dot > 0 )
				spdGain *= -1;

			ctrl.Velocity += spdGain * slopeForward * Time.Delta;
		}
		else
		{
			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			ctrl.ApplyFriction( StopSpeed, Friction );
			timeSinceLastSlide = 0;
		}

		ctrl.SetTag( "skidding" );
		ctrl.Move();
	}

	private bool StillSliding()
	{
		if ( !InputActions.Duck.Down() ) return false;
		if ( ctrl.GroundEntity == null )
		{
			var tr = ctrl.TraceBBox( ctrl.Position, ctrl.Position + Vector3.Down * 3f, 2f );
			if ( tr.Hit ) return true;
			return false;
		}
		if ( ctrl.Velocity.WithZ( 0 ).Length < EndSlideSpeed ) return false;
		if ( InputActions.Jump.Down() ) return false;
		return true;
	}

}
