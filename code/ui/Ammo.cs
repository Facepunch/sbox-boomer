using Sandbox.UI;
using Sandbox.UI.Construct;

public class Ammo : Panel
{
	public Image Icon;
	public Label AmmoInv;

	public Ammo()
	{
		Icon = Add.Image( "ui/vitals/ammo.png", "ammoicon" );
		AmmoInv = Add.Label( "100", "ammoinv" );
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
		AmmoInv.Text = $"{inv}";
		AmmoInv.SetClass( "active", inv >= 0 );
	}
}
