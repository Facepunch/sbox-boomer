using Sandbox;

namespace Facepunch.Boomer.Mechanics;

public partial class SlideMechanic : PlayerControllerMechanic
{
	public override float? EyeHeight => 40f;
	public override int SortOrder => 20;
	public override float? FrictionOverride => 0.5f;
	public override float? WishSpeed => 150f;
	public override Vector3? MoveInputScale => new( 0.5f, 0.5f );

	/// <summary>
	/// How fast we must be going before activating Slide.
	/// </summary>
	private float MinimumSpeed => 250f;

	/// <summary>
	/// We'll try to push the player forward to reach this speed.
	/// Make sure it's pretty high, as friction will make this much harder to reach.
	/// </summary>
	private float SlideSpeed => 1500f;

	/// <summary>
	/// If lock is true, we'll be stuck sliding until ShouldSlide says otherwise.
	/// </summary>
	private bool Lock = false;

	protected bool ShouldSlide()
	{
		if ( !Controller.GroundEntity.IsValid() ) return false;
		if ( Controller.Velocity.Length < MinimumSpeed ) return false;
		if ( TimeSinceStart < 0.5 ) return true;
		if ( TimeSinceStart > 0.5 ) return false;

		return true;
	}

	protected override bool ShouldStart()
	{
		if ( Lock ) return true;
		if ( !Input.Pressed( InputButton.View ) ) return false;
		if ( !Controller.GroundEntity.IsValid() ) return false;
		if ( Controller.Velocity.Length <= MinimumSpeed ) return false;

		return true;
	}

	protected override void OnStart()
	{
		base.OnStart();

		// Give a speed boost
		var forward = new Vector3( Controller.Velocity.x, Controller.Velocity.y, 0 ).Normal;

		Controller.Player.PlaySound( "slide.stop" );

		Controller.Velocity += forward * 500.0f;

		Lock = true;
	}

	protected override void Simulate()
	{
		var hitNormal = Controller.GroundNormal;
		var slopeForward = new Vector3( hitNormal.x, hitNormal.y, 0 );

		if ( Controller.Velocity.Length < SlideSpeed )
			Controller.Velocity += slopeForward * SlideSpeed * Time.Delta;

		if ( !ShouldSlide() )
			Lock = false;
	}
}
