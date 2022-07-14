using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

[Flags]
public enum StarterWeapons
{
	Crowbar = (1 << 0),
	Shotgun = (1 << 1),
	Nailgun = (1 << 2),
	GrenadeLauncher = (1 << 3),
	RocketLauncher = (1 << 4),
	RailGun = (1 << 5)
}

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

	[Property]
	public StarterWeapons Weapons { get; set; }

	public void SetupPlayer( BoomerPlayer player )
	{
		if ( Weapons.HasFlag( StarterWeapons.Crowbar ) )
			player.Inventory.Add( new Crowbar() );
		if ( Weapons.HasFlag( StarterWeapons.Shotgun ) )
			player.Inventory.Add( new Shotgun() );
		if ( Weapons.HasFlag( StarterWeapons.Nailgun ) )
			player.Inventory.Add( new NailGun() );
		if ( Weapons.HasFlag( StarterWeapons.GrenadeLauncher ) )
			player.Inventory.Add( new GrenadeLauncher() );
		if ( Weapons.HasFlag( StarterWeapons.RocketLauncher ) )
			player.Inventory.Add( new RocketLauncher() );
		if ( Weapons.HasFlag( StarterWeapons.RailGun ) )
			player.Inventory.Add( new RailGun() );
	}
}
