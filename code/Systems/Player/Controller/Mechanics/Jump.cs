using Sandbox;

namespace Facepunch.Boomer.Mechanics;

/// <summary>
/// The jump mechanic for players.
/// </summary>
public partial class JumpMechanic : PlayerControllerMechanic
{
	public override int SortOrder => 25;
	private float Gravity => GameGlobals.Gravity;
	private float JumpPower => 342f;
	public int MaxJumps => 2;

	public int CurrentJumps { get; set; }

	protected override bool ShouldStart()
	{
		// If we hit the ground, we'll bunny hop.
		if ( GroundEntity.IsValid() )
		{
			if ( !Input.Down( InputButton.Jump ) ) return false;
		}
		// If we're in the air, we want to make sure we're pressing the button again.
		else
		{
			if ( !Input.Pressed( InputButton.Jump ) ) return false;
		}

		return CurrentJumps > 0f;
	}

	public override void OnGameEvent( string eventName )
	{
		if ( eventName == "walkmechanic.land" )
		{
			CurrentJumps = MaxJumps;
		}
	}

	protected override void OnStart()
	{
		float flMul = JumpPower;
		float startz = Velocity.z;

		var wish = Controller.GetWishInput();

		// Velocity boost in the player's wish dir
		Velocity += wish * 100f;

		Velocity = Velocity.WithZ( startz + flMul );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<WalkMechanic>()
			.ClearGroundEntity();

		Controller.Player.PlaySound( "jump.single" );

		CurrentJumps--;

		IsActive = false;
	}
}
