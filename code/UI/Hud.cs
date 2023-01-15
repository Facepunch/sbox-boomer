using Facepunch.Boomer;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.Boomer.UI;

public partial class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( !Game.IsClient )
			return;

		RootPanel.StyleSheet.Load( "/UI/Hud.scss" );
		RootPanel.AddChild<Chat>();
		RootPanel.AddChild<Info>();
		RootPanel.AddChild<Players>();
		RootPanel.AddChild<Crosshair>();
	}
}
