using Boomer.Movement;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class DashCount : Panel
{
	public Label dashnumber;
	public IconPanel dashicon;
	public DashCount()
	{
		dashicon = Add.Icon( "double_arrow", "dashicon" );
		dashnumber = Add.Label( "0", "dashcount" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return;
		if ( player.Controller is BoomerController controller )
		{
			dashnumber.Text = $"{controller.DashCount}";
		}
	}
}
