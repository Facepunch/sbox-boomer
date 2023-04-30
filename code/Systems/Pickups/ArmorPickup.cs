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
		player.ArmorComponent.Give( ArmorGranted );

		PlayPickupSound();
		OnPickUpRpc( To.Single( player ) );

		base.OnPickup( player );
	}

	public override bool CanPickup( Player player )
	{
		if ( player.ArmorComponent.Current >= player.ArmorComponent.Max ) return false;
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

/// <summary>
/// Gives 25 health points.
/// </summary>
[Library( "boomer_megaarmour" ), HammerEntity]
[EditorModel( "models/gameplay/mega_armour/mega_armour.vmdl" )]
[Title( "Mega Armour" ), Category( "PickUps" )]
partial class MegaArmour : BasePickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/mega_armour/mega_armour.vmdl" );
	public float HealthGranted { get; set; } = 200f;
	public Particles Timer { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		SetupModel();

		UntilRespawn = 60;
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		RespawnTime = 60;

		Timer = Particles.Create( "particles/gameplay/respawnvisual/respawn_timer.vpcf", Position + new Vector3( 0, 0, 16 ) );
		Timer.SetPosition( 1, new Vector3( RespawnTime - UntilRespawn, 2, 0 ) );
		Timer.SetPosition( 2, new Vector3( 0, 255, 0 ) );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not Player player )
			return;

		if ( CanPickup( player ) )
		{
			OnPickup( player );
			Timer = Particles.Create( "particles/gameplay/respawnvisual/respawn_timer.vpcf", Position + new Vector3( 0, 0, 16 ) );
			Timer.SetPosition( 1, new Vector3( RespawnTime, 2, 0 ) );
			Timer.SetPosition( 2, new Vector3( 0, 255, 0 ) );
		}
	}

	[GameEvent.Tick.Client]
	public void DestroyTimer()
	{
		if ( Available )
		{
			Timer.SetPosition( 1, new Vector3( 0, 2, 1 ) );
			Timer.Destroy();
		}
	}

	public override void OnPickup( Player player )
	{
		player.ArmorComponent.Current = 200;

		PlayPickupSound();
		Timer.Destroy( true );

		base.OnPickup( player );
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "megahealth.pickup", Position );
	}
}
