namespace Boomer;

partial class DeathmatchGame : Game
{
	[ConVar.Replicated( "bm_gamemode" )]
	public static string BoommerGameMode { get; set; }
	
	public static bool DeathMatch { get; set; } = false;
	public static bool TeamDeathMatch { get; set; } = false;
	public static bool InstaGib { get; set; } = false;
	public static bool TeamInstaGib { get; set; } = false;
	public static bool MasterTrio { get; set; } = false;
	public static bool RailTag { get; set; } = false;
	public static bool RocketArena { get; set; } = false;
	public static bool MasterBall { get; set; } = false;

	[Net]
	public bool NotUsingStartingGuns { get; set; } = false;

	[Net]
	public bool InstaKillRail { get; set; } = false;

	[Event.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		Gamemode();
	}
	
	public void Gamemode()
	{
		if ( BoommerGameMode == "Deathmatch" )
		{
			DeathMatch = true;
			GameModeDeathMatch();
		}
		else if ( BoommerGameMode == "TeamDeathmatch" )
		{
			TeamDeathMatch = true;
			GameModeTeamDeathMatch();
		}
		else if ( BoommerGameMode == "InstaGib" )
		{
			InstaGib = true;
			GameModeInstaGib();
		}
		else if ( BoommerGameMode == "TeamInstaGib" )
		{
			InstaGib = true;
			GameModeTeamInstaGib();
		}
		else if ( BoommerGameMode == "MasterTrio" )
		{
			MasterTrio = true;
			GameModeMasterTrio();
		}
		else if ( BoommerGameMode == "TeamMasterTrio" )
		{
			MasterTrio = true;
			GameModeTeamMasterTrio();
		}
		else if ( BoommerGameMode == "RailTag" )
		{
			RailTag = true;
			GameModeRailTag();
		}
		else if ( BoommerGameMode == "RocketArena" )
		{
			RocketArena = true;
			GameModeRocketArena();
		}
		else if ( BoommerGameMode == "TeamRocketArena" )
		{
			RocketArena = true;
			GameModeTeamRocketArena();
		}
		else if ( BoommerGameMode == "MasterBall" )
		{
			MasterBall = true;
			GameModeMasterBall();
		}
		else if ( BoommerGameMode == "TeamMasterBall" )
		{
			MasterBall = true;
			GameModeTeamMasterBall();

		}
	}

	//
	//GAMEMODES
	//
	
	//
	//DeathMatch
	//
	public void GameModeDeathMatch()
	{
		foreach ( var check in Entity.All.OfType<StartingWeapons>() )
		{
			UnlimitedAmmo = check.UnlimitedAmmo;
			NoRocketSelfDmg = check.NoRocketSelfDMG;
		}
	}
	//
	//TeamDeathMatch
	//
	public void GameModeTeamDeathMatch()
	{
		foreach ( var check in Entity.All.OfType<StartingWeapons>() )
		{
			UnlimitedAmmo = check.UnlimitedAmmo;
			NoRocketSelfDmg = check.NoRocketSelfDMG;
		}
		
		// Set up teams
		TeamManager.SetupTeam<BoomerTeam.Red>();
		TeamManager.SetupTeam<BoomerTeam.Blue>();
	}

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
	//TeamInstaGib
	//
	public void GameModeTeamInstaGib()
	{
		DeleteAllPickUps();
		DeleteAllGuns();
		InstaKillRail = true;
		UnlimitedAmmo = true;
		NotUsingStartingGuns = true;
		RailGun = true;
		StartingGuns();

		// Set up teams
		TeamManager.SetupTeam<BoomerTeam.Red>();
		TeamManager.SetupTeam<BoomerTeam.Blue>();
	}

	//
	//MASTER TRIO
	//
	public void GameModeMasterTrio()
	{
		DeleteAllGuns();
		DeleteAmmo();
		UnlimitedAmmo = true;
		NoRocketSelfDmg = true;
		NotUsingStartingGuns = true;
		RailGun = true;
		RocketLauncher = true;
		LightningGun = true;
		StartingGuns();
	}

	//
	//Team MASTER TRIO
	//
	public void GameModeTeamMasterTrio()
	{
		DeleteAllGuns();
		DeleteAmmo();
		UnlimitedAmmo = true;
		NoRocketSelfDmg = true;
		NotUsingStartingGuns = true;
		RailGun = true;
		RocketLauncher = true;
		LightningGun = true;
		StartingGuns();

		// Set up teams
		TeamManager.SetupTeam<BoomerTeam.Red>();
		TeamManager.SetupTeam<BoomerTeam.Blue>();
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
	//Team ROCKET ARENA
	//
	public void GameModeTeamRocketArena()
	{
		DeleteAllGuns();
		DeleteAllPickUps();
		UnlimitedAmmo = true;
		NoRocketSelfDmg = true;
		NotUsingStartingGuns = true;
		RocketLauncher = true;
		StartingGuns();

		// Set up teams
		TeamManager.SetupTeam<BoomerTeam.Red>();
		TeamManager.SetupTeam<BoomerTeam.Blue>();
	}

	//
	//MASTERBALL
	//
	public void GameModeMasterBall()
	{
		UnlimitedAmmo = false;
	}

	//
	//Team MASTERBALL
	//
	public void GameModeTeamMasterBall()
	{
		UnlimitedAmmo = false;

		// Set up teams
		TeamManager.SetupTeam<BoomerTeam.Red>();
		TeamManager.SetupTeam<BoomerTeam.Blue>();
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
		Entity.All.OfType<DeathmatchWeapon>().ToList().ForEach( x => x.Delete() );
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
		foreach ( var ArmourKits in Entity.All.OfType<ArmorPickup>() )
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
		foreach ( var Ammos in Entity.All.OfType<AmmoPickup>() )
		{
			Ammos.Delete();
		}
	}
}
