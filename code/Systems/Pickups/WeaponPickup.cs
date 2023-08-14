using Editor;
using Facepunch.Boomer.WeaponSystem;
using Sandbox;
using System.Linq;

namespace Facepunch.Boomer;

public partial class WeaponPickup : BasePickup
{
	public virtual string Prefab { get; set; }

	public override bool CanPickup( Player player )
	{
		return Prefab != null && player.Inventory.FindWeapon( Prefab ) == null;
	}

	public override void OnPickup( Player player )
	{
		var prefabs = PrefabSystem.GetPrefabsOfType<Weapon>();
		if ( prefabs.FirstOrDefault() is Prefab wpnPrefab )
		{
			player.Inventory.AddWeapon( PrefabLibrary.Spawn<Weapon>( wpnPrefab ) );
		}

		base.OnPickup( player );
	}
}

[Library( "boomer_shotgun" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/shotgun/pickup_shotgun.vmdl" )]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
partial class WeaponShotGun : WeaponPickup
{
	public override string Prefab => "resources/weapons/shotgun.prefab";

	public override Model WorldModel => Model.Load( "models/gameplay/weapons/shotgun/pickup_shotgun.vmdl" );
}

[Library( "boomer_nailgun" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/nailgun/pickup_nailgun.vmdl" )]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
partial class WeaponNailGun : WeaponPickup
{
	public override string Prefab => "resources/weapons/nailgun.prefab";

	public override Model WorldModel => Model.Load( "models/gameplay/weapons/nailgun/pickup_nailgun.vmdl" );
}

[Library( "boomer_grenadelauncher" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/grenadelauncher/pickup_grenadelauncher.vmdl" )]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
partial class WeaponGrenadeLauncher : WeaponPickup
{
	public override string Prefab => "resources/weapons/grenadelauncher.prefab";

	public override Model WorldModel => Model.Load( "models/gameplay/weapons/grenadelauncher/pickup_grenadelauncher.vmdl" );
}

[Library( "boomer_rocketlauncher" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/rocketlauncher/pickup_rocketlauncher.vmdl" )]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
partial class WeaponRocketLauncher : WeaponPickup
{
	public override string Prefab => "resources/weapons/rocketlauncher.prefab";

	public override Model WorldModel => Model.Load( "models/gameplay/weapons/rocketlauncher/pickup_rocketlauncher.vmdl" );
}


[Library( "boomer_railgun" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/railgun/pickup_railgun.vmdl" )]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
partial class WeaponRailGun : WeaponPickup
{
	public override string Prefab => "resources/weapons/sniper.prefab";

	public override Model WorldModel => Model.Load( "models/gameplay/weapons/railgun/pickup_railgun.vmdl" );
}

[Library( "boomer_lightninggun" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/lightninggun/pickup_lightninggun.vmdl" )]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
partial class WeaponLightningGun : WeaponPickup
{
	public override string Prefab => "resources/weapons/lightninggun.prefab";

	public override Model WorldModel => Model.Load( "models/gameplay/weapons/lightninggun/pickup_lightninggun.vmdl" );
}
