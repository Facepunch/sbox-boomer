namespace Boomer.UI;

public partial class DeathmatchHud : HudEntity<HudRootPanel>
{
	[ClientRpc]
	public void OnPlayerDied( BoomerPlayer player )
	{
		Game.AssertClient();
	}

	[ClientRpc]
	public void ShowDeathScreen( string attackerName )
	{
		Game.AssertClient();
	}
}
