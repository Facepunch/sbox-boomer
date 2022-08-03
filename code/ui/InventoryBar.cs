using Sandbox.UI;

namespace Boomer.UI;

public class InventoryBar : Panel
{
	public bool IsOpen { get; private set; }

	private DeathmatchWeapon SelectedWeapon { get; set; }
	private List<DeathmatchWeapon> Weapons { get; set; } = new();
	private DeathmatchWeapon LastWeapon { get; set; }
	private Panel Container { get; set; }

	public InventoryBar()
	{
		Container = Add.Panel( "container" );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "active", IsOpen );

		var player = Local.Pawn as Player;
		if ( player == null ) return;

		Weapons.Clear();
		Weapons.AddRange( player.Children.Select( x => x as DeathmatchWeapon ).Where( x => x.IsValid() && x.IsUsable() ) );

		if ( Weapons.Count != Container.ChildrenCount )
		{
			Container.DeleteChildren( true );

			foreach ( var weapon in Weapons.OrderBy( x => x.Order ) )
			{
				var icon = new InventoryIcon( weapon );

				Container.AddChild( icon );
			}
		}
	}

	[Event.BuildInput]
	public void ProcessClientInput( InputBuilder input )
	{
		if ( DeathmatchGame.CurrentState != DeathmatchGame.GameStates.Live ) return;

		bool wantOpen = IsOpen;
		var localPlayer = Local.Pawn as Player;

		wantOpen = wantOpen || input.MouseWheel != 0;
		wantOpen = wantOpen || input.Pressed( InputButton.Menu );
		wantOpen = wantOpen || input.Pressed( InputButton.Slot1 );
		wantOpen = wantOpen || input.Pressed( InputButton.Slot2 );
		wantOpen = wantOpen || input.Pressed( InputButton.Slot3 );
		wantOpen = wantOpen || input.Pressed( InputButton.Slot4 );
		wantOpen = wantOpen || input.Pressed( InputButton.Slot5 );
		wantOpen = wantOpen || input.Pressed( InputButton.Slot6 );

		if ( Weapons.Count == 0 )
		{
			IsOpen = false;
			return;
		}

		if ( IsOpen != wantOpen )
		{
			SelectedWeapon = localPlayer?.ActiveChild as DeathmatchWeapon;
			IsOpen = true;
		}

		if ( !IsOpen ) return;

		var oldSelected = SelectedWeapon;
		var wantedIndex = SlotPressInput( input );

		// If the wanted index is -1, it means we're not pressing any slot keys
		if ( wantedIndex == -1 )
		{
			// Let's check other ways of input
			var sortedWeapons = Weapons.OrderBy( x => x.Bucket ).ToList();

			// Support switching to our last used weapon
			if ( input.Pressed( InputButton.Menu ) )
			{
				if ( LastWeapon.IsValid() && sortedWeapons.Contains( LastWeapon ) )
				{
					SelectedWeapon = LastWeapon;
					input.ActiveChild = SelectedWeapon;
				}
			}
			// Otherwise, check to see if we're using the scroll wheel to switch.
			else if ( input.MouseWheel != 0 )
			{
				var currentIndex = sortedWeapons.IndexOf( SelectedWeapon );
				currentIndex -= input.MouseWheel;
				// Wrap around the array of weapons if we go too far
				currentIndex = currentIndex.UnsignedMod( Weapons.Count );

				var wishedWeapon = sortedWeapons.ElementAtOrDefault( currentIndex );
				if ( wishedWeapon.IsValid() && wishedWeapon != SelectedWeapon )
				{
					SelectedWeapon = wishedWeapon;
					input.ActiveChild = SelectedWeapon;
				}
			}
		}
		else
		{
			// We want to change weapon with slot keys
			var chosenWeapon = Weapons.FirstOrDefault( x => x.Bucket == wantedIndex );
			if ( chosenWeapon != SelectedWeapon && chosenWeapon.IsValid() )
			{
				SelectedWeapon = chosenWeapon;
				input.ActiveChild = SelectedWeapon;
			}
		}

		if ( oldSelected != SelectedWeapon )
		{
			Sound.FromScreen( "weapon.swap" );
			LastWeapon = oldSelected;
		}

		var icons = Container.ChildrenOfType<InventoryIcon>();
		foreach ( var icon in icons )
			icon.TickSelection( SelectedWeapon );

		input.MouseWheel = 0;
	}

	private int SlotPressInput( InputBuilder input )
	{
		int index = -1;

		if ( input.Pressed( InputButton.Slot1 ) ) index = 0;
		if ( input.Pressed( InputButton.Slot2 ) ) index = 1;
		if ( input.Pressed( InputButton.Slot3 ) ) index = 2;
		if ( input.Pressed( InputButton.Slot4 ) ) index = 3;
		if ( input.Pressed( InputButton.Slot5 ) ) index = 4;
		if ( input.Pressed( InputButton.Slot6 ) ) index = 5;

		return index;
	}
}
