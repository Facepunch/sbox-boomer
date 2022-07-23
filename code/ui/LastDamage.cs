using Sandbox.UI;
using Sandbox.UI.Construct;
public class LastDamage : Panel
{
	public Label DmgValue;

	public LastDamage()
	{
		DmgValue = Add.Label( "0", "lastdamage" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as BoomerPlayer;
		if ( player == null ) return;

			SetClass( "nodmg", player.LastDamageDealt == 0 );

			DmgValue.Text = $"-{(int)player.LastDamageDealt}";

	//	DmgValue.Text = $"LOL";

	}
}
