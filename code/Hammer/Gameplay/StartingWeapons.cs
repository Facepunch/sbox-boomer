using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

public enum WeaponType
{
	Crowbar,
	Shotgun,
	Nailgun,
	GrenadeLauncher,
	RocketLauncher,
	RailGun
}

[GameResource( "Starting Weapon Setup", "bsw", "Starter weapon setup, so maps can define what a player starts with." )]
public class StartWeaponGameResource : GameResource
{
	[Property]
	public Dictionary<WeaponType, int> Weapons { get; set; } = new();
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

	[Property, ResourceType( "bsw" )]
	public string AssetPath { get; set; }

	protected StartWeaponGameResource Resource;

	public override void Spawn()
	{
		base.Spawn();

		Resource = ResourceLibrary.Get<StartWeaponGameResource>( AssetPath );
	}

	public DeathmatchWeapon MakeWeapon( WeaponType type )
	{
		return type switch
		{
			WeaponType.Crowbar => new Crowbar(),
			WeaponType.Shotgun => new Shotgun(),
			WeaponType.Nailgun => new NailGun(),
			WeaponType.GrenadeLauncher => new GrenadeLauncher(),
			WeaponType.RocketLauncher => new RocketLauncher(),
			WeaponType.RailGun => new RailGun(),
			_ => null
		};
	}

	public void Give( BoomerPlayer player, WeaponType type, int ammo = 0 )
	{
		var weapon = MakeWeapon( type );
		if ( !weapon.IsValid() ) return;

		if ( ammo > 0 )
		{
			player.GiveAmmo( weapon.AmmoType, ammo );
		}
	}

	public void SetupPlayer( BoomerPlayer player )
	{
		if ( Resource is null ) return;

		foreach ( var kv in Resource.Weapons )
		{
			var type = kv.Key;
			var ammo = kv.Value;

			Give( player, type, ammo );
		}
	}
}
