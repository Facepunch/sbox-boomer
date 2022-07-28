
using System.ComponentModel.DataAnnotations;

[Library( "shooter_startingweapons", Description = "Starting Weapons" )]
[EditorSprite( "editor/ent_logic.vmat" )]
[Display( Name = "Starting Weapons", GroupName = "Shooter", Description = "Coin Pickup." ), Category( "Gameplay" ), Icon( "currency_bitcoin" )]
[HammerEntity]
partial class StartingWeapons : Entity
{
	public static StartingWeapons Instance;
	
	public StartingWeapons()
	{
		Instance = this;
	}

	[Property] public bool Crowbar { get; set; }
	[Property] public bool Shotgun { get; set; }
	[Property] public bool Nailgun { get; set; }
	[Property] public bool GrenadeLauncher { get; set; }
	[Property] public bool RocketLauncher { get; set; }
	[Property] public bool RailGun { get; set; }
	[Property] public bool LightningGun { get; set; }

	[Property] public int BuckshotAmmo { get; set; } = 32;
	[Property] public int NailsAmmo { get; set; } = 128;
	[Property] public int GrenadeAmmo { get; set; } = 8;
	[Property] public int RocketAmmo { get; set; } = 8;
	[Property] public int RailAmmo { get; set; } = 24;
	[Property] public int LightningAmmo { get; set; } = 300;

	public void SetupPlayer( BoomerPlayer player )
	{
		if ( Crowbar )
		{
			player.Inventory.Add( new Crowbar() );
		}

		if ( Shotgun )
		{
			player.Inventory.Add( new Shotgun() );
			player.GiveAmmo( AmmoType.Buckshot, BuckshotAmmo );
		}

		if ( Nailgun )
		{
			player.Inventory.Add( new NailGun() );
			player.GiveAmmo( AmmoType.Nails, NailsAmmo );
		}

		if ( GrenadeLauncher )
		{
			player.Inventory.Add( new GrenadeLauncher() );
			player.GiveAmmo( AmmoType.Grenade, GrenadeAmmo );
		}

		if ( RocketLauncher )
		{
			player.Inventory.Add( new RocketLauncher() );
			player.GiveAmmo( AmmoType.Rockets, RocketAmmo );
		}

		if ( RailGun )
		{
			player.Inventory.Add( new RailGun() );
			player.GiveAmmo( AmmoType.Rails, RailAmmo );
		}

		if ( LightningGun )
		{
			player.Inventory.Add( new LightningGun() );
			player.GiveAmmo( AmmoType.Lightning, LightningAmmo );
		}
	}
}
