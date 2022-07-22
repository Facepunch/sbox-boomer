using Sandbox.UI;

public class InventoryBar : Panel
{
	public bool IsOpen { get; private set; }

	private DeathmatchWeapon SelectedWeapon { get; set; }
	private List<DeathmatchWeapon> Weapons { get; set; } = new();
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

		var sortedWeapons = Weapons.OrderBy( x => x.Order ).ToList();
		var oldSelected = SelectedWeapon;
		var selectedIndex = sortedWeapons.IndexOf( SelectedWeapon );

		selectedIndex = SlotPressInput( input, selectedIndex, sortedWeapons );
		selectedIndex -= input.MouseWheel;

		if ( input.Pressed( InputButton.Menu ) )
			selectedIndex++;

		selectedIndex = selectedIndex.UnsignedMod( Weapons.Count );

		SelectedWeapon = sortedWeapons[selectedIndex];

		var icons = Container.ChildrenOfType<InventoryIcon>();

		foreach ( var icon in icons )
		{
			icon.TickSelection( SelectedWeapon );
		}

		input.ActiveChild = SelectedWeapon;
		input.MouseWheel = 0;

		if ( oldSelected != SelectedWeapon )
		{
			Sound.FromScreen( "weapon.swap" );
		}
	}

	private int SlotPressInput( InputBuilder input, int index, List<DeathmatchWeapon> sortedWeapons )
	{
		if ( input.Pressed( InputButton.Slot1 ) ) index = 0;
		if ( input.Pressed( InputButton.Slot2 ) ) index = 1;
		if ( input.Pressed( InputButton.Slot3 ) ) index = 2;
		if ( input.Pressed( InputButton.Slot4 ) ) index = 3;
		if ( input.Pressed( InputButton.Slot5 ) ) index = 4;
		if ( input.Pressed( InputButton.Slot6 ) ) index = 5;

		return index;
	}
}
