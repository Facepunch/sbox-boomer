using Editor;
using Sandbox;

namespace Facepunch.Boomer;

/// <summary>
/// Gives 25 Armour
/// </summary>
[Library( "boomer_armour" ), HammerEntity]
[EditorModel( "models/dm_battery.vmdl" )]
[Title( "Armour" ), Category( "PickUps" )]
partial class ArmorPickup : BasePickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/armour/armourkit.vmdl" );
	public float ArmorGranted { get; set; } = 25f;

	public override void OnPickup( Player player )
	{
		//var newhealth = player.Armour + ArmorGranted;
		//newhealth = newhealth.Clamp( 0, 100 );
		//player.Armor = newhealth;

		PlayPickupSound();
		OnPickUpRpc( To.Single( player ) );

		base.OnPickup( player );
	}

	public override bool CanPickup( Player player )
	{
		//if ( player.Armour >= 100 ) return false;
		return base.CanPickup( player );
	}

	[ClientRpc]
	public void OnPickUpRpc()
	{
		// TODO - Implement
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "armour.pickup", Position );
	}
}

/// <summary>
/// Gives 5 Armour
/// </summary>
[Library( "boomer_armourshard" ), HammerEntity]
[EditorModel( "models/gameplay/armour_shard/dm_armour_shard.vmdl" )]
[Title( "Armour Shard" ), Category( "PickUps" )]
partial class ArmourShard : ArmorPickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/armour_shard/dm_armour_shard.vmdl" );

	public override void Spawn()
	{
		base.Spawn();

		RespawnTime = 15;
		ArmorGranted = 5f;
	}
}
