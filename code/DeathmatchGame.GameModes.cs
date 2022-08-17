namespace Boomer;

partial class DeathmatchGame : Game
{
	[ConVar.Replicated( "bm_instagib" )]
	public static bool InstaGib { get; set; } = false;

	[ConVar.Replicated( "bm_mastertrio" )]
	public static bool MasterTrio { get; set; } = false;

	[ConVar.Replicated( "bm_railtag" )]
	public static bool RailTag { get; set; } = false;

	[ConVar.Replicated( "bm_rocketarena" )]
	public static bool RocketArena { get; set; } = false;

	[ConVar.Replicated( "bm_masterball" )]
	public static bool MasterBall { get; set; } = false;

	[Net]
	public static bool NotUsingStartingGuns { get; set; } = false;

	[Net]
	public static bool InstaKillRail { get; set; } = false;

	[Event.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		Gamemode();
	}
	
	public void Gamemode()
	{
		//UnlimitedAmmo = false;
		
		if ( InstaGib )
		{
			GameModeInstaGib();
		}
		if ( MasterTrio )
		{
			GameModeMasterTrio();
		}
		if ( RailTag )
		{
			GameModeRailTag();
		}
		if ( RocketArena )
		{
			GameModeRocketArena();
		}
		if ( MasterBall )
		{
			GameModeMasterBall();

		}
	}
	
	//
	//GAMEMODES
	//
	
	//
	//InstaGib
	//
	public void GameModeInstaGib()
	{
		DeleteAllPickUps();
		DeleteAllGuns();
		InstaKillRail = true;
		UnlimitedAmmo = true;
		NotUsingStartingGuns = true;
		RailGun = true;
		StartingGuns();
	}
	
	//
	//MASTER TRIO
	//
	public void GameModeMasterTrio()
	{
		DeleteAllGuns();
		UnlimitedAmmo = true;
		NoRocketSelfDmg = true;
		NotUsingStartingGuns = true;
		RailGun = true;
		RocketLauncher = true;
		LightningGun = true;
		StartingGuns();
	}
	
	//
	//RAIL TAG
	//
	public void GameModeRailTag()
	{
		DeleteAllGuns();
		DeleteAllPickUps();
		InstaKillRail = true;
		UnlimitedAmmo = true;
		NotUsingStartingGuns = true;
		StartingGuns();
	}
	public void StartTag()
	{
		var allplayers = Entity.All.OfType<BoomerPlayer>();

		var randomplayer = allplayers.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		randomplayer.TaggedPlayer = true;
		randomplayer.Inventory.Add( new RailGun() );
		randomplayer.GiveAmmo( AmmoType.Rails, 100 );
	}

	//
	//ROCKET ARENA
	//
	public void GameModeRocketArena()
	{
		DeleteAllGuns();
		DeleteAllPickUps();
		UnlimitedAmmo = true;
		NoRocketSelfDmg = true;
		NotUsingStartingGuns = true;
		RocketLauncher = true;
		StartingGuns();
	}
	
	//
	//MASTERBALL
	//
	public void GameModeMasterBall()
	{
		UnlimitedAmmo = false;
	}
	public void StartMasterBall()
	{
		var masterball = new MasterBall();
		masterball.Position = new Vector3( 0, 0, 2096 );
	}

	//
	//PlayerStatingGuns
	//
	public bool Shotgun { get; set; } = false;
	public bool Nailgun { get; set; } = false;
	public bool GrenadeLauncher { get; set; } = false;
	public bool RocketLauncher { get; set; } = false;
	public bool RailGun { get; set; } = false;
	public bool LightningGun { get; set; } = false;
	public void StartingGuns( )
	{
		StartingWeapons.Instance.Shotgun = Shotgun;
		StartingWeapons.Instance.Nailgun = Nailgun;
		StartingWeapons.Instance.GrenadeLauncher = GrenadeLauncher;
		StartingWeapons.Instance.RocketLauncher = RocketLauncher;
		StartingWeapons.Instance.LightningGun = LightningGun;
		StartingWeapons.Instance.RailGun = RailGun;
	}

	//
	//ALL GUNS
	//
	public void DeleteAllGuns()
	{
		DeleteNailGun();
		DeleteGrenadeLauncher();
		DeleteLightningGun();
		DeleteRailGun();
		DeleteRocketLauncher();
		DeleteShotgun();
	}
	//
	//GUNS
	//
	public void DeleteNailGun()
	{
		foreach ( var NailGuns in Entity.All.OfType<NailGun>() )
		{
			NailGuns.Delete();
		}
	}
	public void DeleteGrenadeLauncher()
	{
		foreach ( var GrenadeLaunchers in Entity.All.OfType<GrenadeLauncher>() )
		{
			GrenadeLaunchers.Delete();
		}
	}
	public void DeleteLightningGun()
	{
		foreach ( var LightningGuns in Entity.All.OfType<LightningGun>() )
		{
			LightningGuns.Delete();
		}
	}
	public void DeleteRailGun()
	{
		foreach ( var RailGuns in Entity.All.OfType<RailGun>() )
		{
			RailGuns.Delete();
		}
	}
	public void DeleteRocketLauncher()
	{
		foreach ( var RocketLaunchers in Entity.All.OfType<RocketLauncher>() )
		{
			RocketLaunchers.Delete();
		}
	}
	public void DeleteShotgun()
	{
		foreach ( var Shotguns in Entity.All.OfType<Shotgun>() )
		{
			Shotguns.Delete();
		}
	}

	//
	//ALL PICKUPS
	//
	public void DeleteAllPickUps()
	{
		DeleteHealth();
		DeleteArmour();
		DeleteAmmo();
		DeleteArmourShard();
		DeleteHealthVials();
		DeleteMegaArmour();
		DeleteMegaHealth();
	}
	//
	//PICKUPS
	//
	public void DeleteHealth()
	{
		foreach ( var HealthKits in Entity.All.OfType<HealthKit>() )
		{	
			HealthKits.Delete();
		}
	}
	public void DeleteHealthVials()
	{
		foreach ( var HealthVials in Entity.All.OfType<HealthVial>() )
		{
			HealthVials.Delete();
		}
	}
	public void DeleteMegaHealth()
	{
		foreach ( var MegaHealth in Entity.All.OfType<MegaHealth>() )
		{
			MegaHealth.Delete();
		}
	}
	public void DeleteArmour()
	{
		foreach ( var ArmourKits in Entity.All.OfType<Armour>() )
		{
			ArmourKits.Delete();
		}
	}
	public void DeleteArmourShard()
	{
		foreach ( var ArmourShards in Entity.All.OfType<ArmourShard>() )
		{
			ArmourShards.Delete();
		}
	}
	public void DeleteMegaArmour()
	{
		foreach ( var MegaArmour in Entity.All.OfType<MegaArmour>() )
		{
			MegaArmour.Delete();
		}
	}
	public void DeleteAmmo()
	{
		foreach ( var AmmoNails in Entity.All.OfType<AmmoNails>() )
		{
			AmmoNails.Delete();
		}
		foreach ( var AmmoRockets in Entity.All.OfType<AmmoRockets>() )
		{
			AmmoRockets.Delete();
		}
		foreach ( var AmmoBuckshot in Entity.All.OfType<AmmoBuckshot>() )
		{
			AmmoBuckshot.Delete();
		}
		foreach ( var AmmoGrenades in Entity.All.OfType<AmmoGrenades>() )
		{
			AmmoGrenades.Delete();
		}
		foreach ( var AmmoRails in Entity.All.OfType<AmmoRails>() )
		{
			AmmoRails.Delete();
		}
		foreach ( var AmmoLightning in Entity.All.OfType<AmmoLightning>() )
		{
			AmmoLightning.Delete();
		}
	}
}
