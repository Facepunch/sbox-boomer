namespace Boomer;

/// <summary>
/// Gives 25 Armour
/// </summary>
[Library( "boomer_armourshard" ), HammerEntity]
[EditorModel( "models/gameplay/armour_shard/dm_armour_shard.vmdl" )]
[Title( "Armour Shard" ), Category( "PickUps" )]
partial class ArmourShard : ArmorPickup
{
	public override void Spawn()
	{
		base.Spawn();

		ArmorGranted = 5f;
	}
}
