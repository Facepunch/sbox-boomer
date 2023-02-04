using Editor;
using Facepunch.Boomer.WeaponSystem;
using Sandbox;
using System.Linq;

namespace Facepunch.Boomer;

[Library( "boomer_weaponpickup" ), HammerEntity]
[Model]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
public partial class WeaponPickup : BasePickup
{
	[Property, ResourceType( "prefab" )]
	public string Prefab { get; set; }


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
