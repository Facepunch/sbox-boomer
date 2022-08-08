namespace Boomer;

[Library( "boomer_startingweapons", Description = "Starting Weapons" )]
[EditorSprite( "editor/ent_logic.vmat" )]
[Display( Name = "Starting Weapons", GroupName = "Shooter", Description = "Coin Pickup." ), Category( "Gameplay" ), Icon( "currency_bitcoin" )]
[HammerEntity]
partial class StartingWeapons : Entity
{
	[Net]
	public static StartingWeapons Instance { get; set; }
	
	public StartingWeapons()
	{
		Instance = this;

	}
	[Property, Category( "Weapons" )] public bool Shotgun { get; set; }
	[Property, Category( "Weapons" )] public bool Nailgun { get; set; }
	[Property, Category( "Weapons" )] public bool GrenadeLauncher { get; set; }
	[Property, Category( "Weapons" )] public bool RocketLauncher { get; set; }
	[Property, Category( "Weapons" )] public bool RailGun { get; set; }
	[Property, Category( "Weapons" )] public bool LightningGun { get; set; }
	[Property, Category( "Ammo" )] public int BuckshotAmmo { get; set; } = 32;
	[Property, Category( "Ammo" )] public int NailsAmmo { get; set; } = 128;
	[Property, Category( "Ammo" )] public int GrenadeAmmo { get; set; } = 8;
	[Property, Category( "Ammo" )] public int RocketAmmo { get; set; } = 8;
	[Property, Category( "Ammo" )] public int RailAmmo { get; set; } = 24;
	[Property, Category( "Ammo" )] public int LightningAmmo { get; set; } = 300;
	[Property, Category( "Special" ), Net] public bool UnlimitedAmmo { get; set; }
	[Property, Category( "Special" ), Title ("No Rocket Self Damage"), Net] public bool NoRocketSelfDMG { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		if ( UnlimitedAmmo )
		{
			DeathmatchGame.UnlimitedAmmo = UnlimitedAmmo;
		}
		if ( NoRocketSelfDMG )
		{
			DeathmatchGame.NoRocketSelfDmg = NoRocketSelfDMG;
		}
	}

	public void SetupPlayer( BoomerPlayer player )
	{
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
