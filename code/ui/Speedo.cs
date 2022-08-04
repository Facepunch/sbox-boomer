using Boomer;
using Boomer.Movement;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Speedo : Panel
{
	//public Label dashnumber;
	public Label Speed;


	public List<Panel> DashSegments;
	public Speedo()
	{
		Speed = Add.Label( "", "text" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return;
		if ( player.Controller is BoomerController controller )
		{
			Speed.Text = $"{Math.Floor(controller.Velocity.WithZ( 0 ).Length )}";
			Speed.SetClass("active", ClientSettings.Current.Speedometer);
		}
	}
}
