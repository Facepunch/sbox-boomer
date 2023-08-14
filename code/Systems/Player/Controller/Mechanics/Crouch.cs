using Facepunch.Boomer.WeaponSystem;
using Sandbox;

namespace Facepunch.Boomer.Mechanics;

/// <summary>
/// The basic crouch mechanic for players.
/// </summary>
public partial class CrouchMechanic : PlayerControllerMechanic
{
	public override int SortOrder => 9;
	public override float? WishSpeed => 120f;
	public override float? EyeHeight => 40f;

	protected override bool ShouldStart()
	{
		if ( !Input.Down( "Duck" ) ) return false;
		if ( !Controller.GroundEntity.IsValid() ) return false;

		return true;
	}

	protected override void OnStart()
	{
		Player.Tags.Add( "ducked" );
	}

	protected override void OnStop()
	{
		Player.Tags.Remove( "ducked" );
	}
}
