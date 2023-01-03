using Sandbox;

namespace Facepunch.Boomer.Mechanics;

/// <summary>
/// The jump mechanic for players.
/// </summary>
public partial class JumpMechanic : PlayerControllerMechanic
{
	public override int SortOrder => 25;
	private float Gravity => 900f;
	private float JumpPower => 375f;

	protected override bool ShouldStart()
	{
		if ( !Input.Down( InputButton.Jump ) ) return false;
		if ( !Controller.GroundEntity.IsValid() ) return false;
		return true;
	}

	protected override void OnStart()
	{
		float flGroundFactor = 1.0f;
		float flMul = JumpPower;
		float startz = Velocity.z;

		var wish = Controller.GetWishInput();

		// Velocity boost in the player's wish dir
		Velocity += wish * 200f;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;


		Controller.GetMechanic<WalkMechanic>()
			.ClearGroundEntity();
	}
}
