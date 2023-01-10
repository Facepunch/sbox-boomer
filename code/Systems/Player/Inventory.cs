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

	public Weapon FindWeapon( WeaponData data )
	{
		return Weapons.FirstOrDefault( x => x.WeaponData == data );
	}

	public Weapon FindWeapon( string name )
	{
		return FindWeapon( WeaponData.FindResource( name ) );
	}

	public bool AddWeapon( Weapon weapon, bool makeActive = true )
	{
		if ( Weapons.Contains( weapon ) ) return false;

		Weapons.Add( weapon );

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

	protected int GetSlotIndexFromInput( InputButton slot )
	{
		return slot switch
		{
			InputButton.Slot1 => 0,
			InputButton.Slot2 => 1,
			InputButton.Slot3 => 2,
			InputButton.Slot4 => 3,
			InputButton.Slot5 => 4,
			InputButton.Slot6 => 5,
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

	protected void TrySlotFromInput( InputButton slot )
	{
		if ( Input.Pressed( slot ) )
		{
			Input.SuppressButton( slot );

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
		TrySlotFromInput( InputButton.Slot1 );
		TrySlotFromInput( InputButton.Slot2 );
		TrySlotFromInput( InputButton.Slot3 );
		TrySlotFromInput( InputButton.Slot4 );
		TrySlotFromInput( InputButton.Slot5 );
		TrySlotFromInput( InputButton.Slot6 );

		var slotDirection = Input.MouseWheel;

		if ( Input.Pressed( InputButton.SlotPrev ) )
			slotDirection = -1;
		else if ( Input.Pressed( InputButton.SlotNext ) )
			slotDirection = 1;

		if ( slotDirection != 0 )
			SwitchActiveSlot( slotDirection, true );

		if ( Input.Pressed( InputButton.Menu ) )
			SetWeaponFromSlot( LastWeaponSlot );

		/* Boomer specific weapon shortcuts */
		if ( Input.Pressed( InputButton.Use ) )
		{
			var wpn = FindWeapon( "lightning" );
			if ( wpn.IsValid() ) SetWeaponFromSlot( GetSlotFromWeapon( wpn ) );
		}

		if ( Input.Pressed( InputButton.Reload ) )
		{
			var wpn = FindWeapon( "sniper" );
			if ( wpn.IsValid() ) SetWeaponFromSlot( GetSlotFromWeapon( wpn ) );
		}

		if ( Input.Pressed( InputButton.Flashlight ) )
		{
			var wpn = FindWeapon( "rl" );
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
