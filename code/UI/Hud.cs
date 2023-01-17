using Facepunch.Boomer;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.Boomer.UI;

public partial class Hud : HudEntity<RootPanel>
{
	public static Panel CurrentHudPanel { get; protected set; }

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


	/// <summary>
	/// Little bit hacky, but a way to set the hud panel to match the gamemode for now.
	/// </summary>
	[Event.Tick.Client]
	protected void DoTick()
	{
		if ( !GamemodeSystem.Current.IsValid() ) return;
		if ( CurrentHudPanel is not null ) return;

		var gamemode = GamemodeSystem.Current;
		CurrentHudPanel = gamemode.HudPanel;

		if ( CurrentHudPanel is not null )
			CurrentHudPanel.Parent = this.RootPanel;
	}
}
