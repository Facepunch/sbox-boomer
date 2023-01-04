using Sandbox;
using System;

namespace Facepunch.Boomer.Mechanics;

public partial class DashMechanic : PlayerControllerMechanic
{
	protected override bool ShouldStart()
	{
		if ( !Input.Pressed( InputButton.Run ) ) return false;

		return true;
	}

	protected override void OnStart()
	{
		// Give a speed boost
		var wish = Controller.GetWishInput();
		Controller.Velocity += wish * 1250.0f;
	}
}
