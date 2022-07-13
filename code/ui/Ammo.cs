using Sandbox.UI;
using Sandbox.UI.Construct;

public class Ammo : Panel
{
	public Label Inventory;

	public Ammo()
	{
		Inventory = Add.Label( "100", "inventory" );
	}

	int weaponHash;

	public override void Tick()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return;

		var weapon = player.ActiveChild as DeathmatchWeapon;
		SetClass( "active", weapon != null );

		if ( weapon == null ) return;

		var inv = weapon.AvailableAmmo();
		Inventory.Text = $"{inv}";
		Inventory.SetClass( "active", inv >= 0 );
	}
}
