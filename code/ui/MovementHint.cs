using Boomer;
using Boomer.Movement;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class MovementHint : Panel
{
	//public Label dashnumber;
	public Label Jump;
	public Label Duck;

	public List<Panel> DashSegments;
	public MovementHint()
	{
		Jump = Add.Label( "keyboard_double_arrow_up", "jump" );
		Duck = Add.Label( "keyboard_double_arrow_down", "duck" );
	}
	
	public override void Tick()
	{
		var player = Game.LocalPawn as Player;
		if ( player == null ) return;

		if ( ClientSettings.Current.ShowMovementHint )
		{
			Jump.SetClass( "active", Input.Down( InputButton.Jump ) );
			Duck.SetClass( "active", Input.Down( InputButton.Duck ) );
		}
	}
}
