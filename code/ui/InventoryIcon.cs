﻿using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Boomer.UI;

class InventoryIcon : Panel
{
	public DeathmatchWeapon Weapon;
	public Panel Icon;
	public Label Value;
	public InputHint Hint;

	public InputButton FromBucket( int bucket )
	{
		return bucket switch
		{
			0 => InputButton.Slot1,
			1 => InputButton.Slot2,
			2 => InputButton.Slot3,
			3 => InputButton.Slot4,
			4 => InputButton.Slot5,
			5 => InputButton.Slot6,
			_ => InputButton.Slot0,
		};
	}

	public InventoryIcon( DeathmatchWeapon weapon )
	{
		Weapon = weapon;
		Icon = Add.Panel( "icon" );
		Value = Add.Label( $"0", "ammocount");
		Hint = AddChild<InputHint>( "hint" );
		Hint.SetButton( FromBucket( weapon.Bucket ) );
		Hint.DisableOnGamepad = true;

		AddClass( weapon.ClassName );
	}

	public override void Tick()
	{
		base.Tick();

		if( !Weapon.IsValid || Weapon.Owner != Game.LocalPawn )
		{
			Delete( true );
			return;
		}

		var active = Weapon.Owner is BoomerPlayer pl && pl.ActiveChild == Weapon;
		var empty = !Weapon.IsUsable();

		SetClass( "active", active );
		SetClass( "empty", empty );

		if(DeathmatchGame.UnlimitedAmmo )
			Value.Text = "∞";
		else
			Value.Text = Weapon.AvailableAmmo().ToString();
	//	Value.Text = $"{Weapon.AvailableAmmo()}";
		
	}
}
