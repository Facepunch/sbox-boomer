using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Boomer;

public partial class AmmoVital : Panel
{
	public Image Icon;
	public Label AmmoInv;

	public static AmmoVital Current { get; private set; }

	BoomerPlayer Player => BoomerCamera.Target ?? Local.Pawn as BoomerPlayer;

	public AmmoVital()
	{
		Current = this;

		Icon = Add.Image( "ui/vitals/ammo.png", "ammoicon" );
		AmmoInv = Add.Label( "100", "ammoinv" );

		AmmoInv.BindClass( "low", () => Player?.SinceAmmoWentDown <= 0.1f );
		AmmoInv.BindClass( "gained", () => Player?.SinceAmmoWentUp <= 0.1f );
	}

	public override void Tick()
	{
		var player = Player;
		if ( !player.IsValid() ) return;

		var weapon = player.ActiveChild as DeathmatchWeapon;
		SetClass( "active", weapon.IsValid() );

		if ( !weapon.IsValid() ) return;

		var inv = weapon.AvailableAmmo();
		AmmoInv.Text = $"{inv}";
		AmmoInv.SetClass( "active", inv >= 0 );
	}
}
