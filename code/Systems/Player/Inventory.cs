using Facepunch.Boomer.WeaponSystem;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Boomer;

/// <summary>
/// The player's inventory holds a player's weapons, and holds the player's current weapon.
/// It also drives functionality such as weapon switching.
/// </summary>
public partial class Inventory : EntityComponent<Player>, ISingletonComponent
{
	[Net] protected IList<Weapon> Weapons { get; set; }
	[Net, Predicted] public Weapon ActiveWeapon { get; set; }
	[Net, Predicted] public int LastWeaponSlot { get; set; }

	public Weapon FindWeapon( string name )
	{
		return Weapons.FirstOrDefault( x => x.Name == name );
	}

	public bool AddWeapon( Weapon weapon, bool makeActive = true )
	{
		if ( Weapons.Contains( weapon ) ) return false;

		Weapons.Add( weapon );

		OrderWeaponsBySlot();

		if ( makeActive )
			SetActiveWeapon( weapon );

		return true;
	}

	public bool RemoveWeapon( Weapon weapon, bool drop = false )
	{
		var success = Weapons.Remove( weapon );
		if ( success && drop )
		{
			// TODO - Drop the weapon on the ground
		}

		return success;
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		var currentWeapon = ActiveWeapon;
		if ( currentWeapon.IsValid() )
		{
			// Can reject holster if we're doing an action already
			if ( !currentWeapon.CanHolster( Entity ) )
			{
				return;
			}

			currentWeapon.OnHolster( Entity );

			if ( currentWeapon.IsValid() )
				LastWeaponSlot = ActiveSlot;

			ActiveWeapon = null;
		}

		// Can reject deploy if we're doing an action already
		if ( !weapon.CanDeploy( Entity ) )
		{
			return;
		}

		ActiveWeapon = weapon;

		weapon?.OnDeploy( Entity );
	}

	protected override void OnDeactivate()
	{
		if ( Game.IsServer )
		{
			Weapons.ToList()
				.ForEach( x => x.Delete() );
		}
	}

	public int GetSlotFromWeapon( Weapon weapon ) => Weapons.IndexOf( weapon );

	public int ActiveSlot => GetSlotFromWeapon( ActiveWeapon );

	public Weapon GetSlot( int slot )
	{
		return Weapons.ElementAtOrDefault( slot ) ?? null;
	}
	public void OrderWeaponsBySlot()
	{
		Weapons = Weapons.OrderBy( w => w.Slot ).ToList();
	}

	protected int GetSlotIndexFromInput( string slot )
	{
		return slot switch
		{
			"Slot1" => 0,
			"Slot2" => 1,
			"Slot3" => 2,
			"Slot4" => 3,
			"Slot5" => 4,
			"Slot6" => 5,
			_ => -1
		};
	}

	public void SetWeaponFromSlot( int slot )
	{
		if ( GetSlot( slot ) is Weapon weapon )
		{
			Entity.ActiveWeaponInput = weapon;
		}
	}

	protected void TrySlotFromInput( string slot )
	{
		if ( Input.Pressed( slot ) )
		{
			Input.ReleaseAction( slot );

			SetWeaponFromSlot( GetSlotIndexFromInput( slot ) );
		}
	}

	public void SwitchActiveSlot( int idelta, bool loop )
	{
		var count = Weapons.Count;
		if ( count == 0 ) return;

		var slot = ActiveSlot;
		var nextSlot = slot + idelta;

		if ( loop )
		{
			while ( nextSlot < 0 ) nextSlot += count;
			while ( nextSlot >= count ) nextSlot -= count;
		}
		else
		{
			if ( nextSlot < 0 ) return;
			if ( nextSlot >= count ) return;
		}

		if ( GetSlot( nextSlot ) is Weapon weapon )
		{
			Entity.ActiveWeaponInput = weapon;
		}
	}

	public void BuildInput()
	{
		TrySlotFromInput( "Slot1" );
		TrySlotFromInput( "Slot2" );
		TrySlotFromInput( "Slot3" );
		TrySlotFromInput( "Slot4" );
		TrySlotFromInput( "Slot5" );
		TrySlotFromInput( "Slot6" );

		var slotDirection = Input.MouseWheel;

		if ( Input.Pressed( "SlotPrev" ) )
			slotDirection = -1;
		else if ( Input.Pressed( "SlotNext" ) )
			slotDirection = 1;

		if ( slotDirection != 0 )
			SwitchActiveSlot( slotDirection, true );

		if ( Input.Pressed( "Menu" ) )
			SetWeaponFromSlot( LastWeaponSlot );

		/* Boomer specific weapon shortcuts */
		if ( Input.Pressed( "Use" ) )
		{
			var wpn = FindWeapon( "Lightning Gun" );
			if ( wpn.IsValid() ) SetWeaponFromSlot( GetSlotFromWeapon( wpn ) );
		}

		if ( Input.Pressed( "Reload" ) )
		{
			var wpn = FindWeapon( "Railgun" );
			if ( wpn.IsValid() ) SetWeaponFromSlot( GetSlotFromWeapon( wpn ) );
		}

		if ( Input.Pressed( "Flashlight" ) )
		{
			var wpn = FindWeapon( "Rocket Launcher" );
			if ( wpn.IsValid() ) SetWeaponFromSlot( GetSlotFromWeapon( wpn ) );
		}

		ActiveWeapon?.BuildInput();
	}

	public void Simulate( IClient cl )
	{
		if ( Entity.ActiveWeaponInput != null && ActiveWeapon != Entity.ActiveWeaponInput )
		{
			SetActiveWeapon( Entity.ActiveWeaponInput as Weapon );
			Entity.ActiveWeaponInput = null;
		}

		ActiveWeapon?.Simulate( cl );
	}

	public void FrameSimulate( IClient cl )
	{
		ActiveWeapon?.FrameSimulate( cl );
	}
}
