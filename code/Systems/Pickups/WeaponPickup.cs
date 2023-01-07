using Editor;
using Facepunch.Boomer.WeaponSystem;
using Sandbox;
using System.Linq;

namespace Facepunch.Boomer;

[Library( "boomer_weaponpickup" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/buckshot/bm_buckshot.vmdl" )]
[Title( "Weapon Pickup" ), Category( "Pickups" )]
public partial class WeaponPickup : BasePickup
{
	[Property, ResourceType( "weapon" )]
	public string WeaponPath { get; set; }

	public WeaponData GetData()
	{
		return WeaponData.All.FirstOrDefault( x => x.ResourcePath == WeaponPath );
	}

	public override bool CanPickup( Player player )
	{
		var data = GetData();
		return data != null && player.Inventory.FindWeapon( data ) == null;
	}

	public override void OnPickup( Player player )
	{
		player.Inventory.AddWeapon( WeaponData.CreateInstance( GetData() ) );

		base.OnPickup( player );
	}
}
