namespace Boomer.UI;

public partial class DeathmatchHud : HudEntity<HudRootPanel>
{
	[ClientRpc]
	public void OnPlayerDied( BoomerPlayer player )
	{
		Host.AssertClient();
	}

	[ClientRpc]
	public void ShowDeathScreen( string attackerName )
	{
		Host.AssertClient();
	}
}
