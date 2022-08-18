namespace Boomer;

/// <summary>
/// Gives 25 Armour
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
