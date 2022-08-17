namespace Boomer;

/// <summary>
/// Gives 25 Armour
/// </summary>
[Library( "boomer_armourshard" ), HammerEntity]
[EditorModel( "models/gameplay/armour_shard/dm_armour_shard.vmdl" )]
[Title( "Armour Shard" ), Category( "PickUps" )]
partial class ArmourShard : AnimatedEntity, IRespawnableEntity
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/armour_shard/dm_armour_shard.vmdl" );

	[Property]
	public int RespawnTime { get; set; } = 15;
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;

		PhysicsEnabled = true;
		UsePhysicsCollision = true;

		Tags.Add( "trigger" );

		if ( DeathmatchGame.InstaGib )
		{
			Delete();
		}
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( IsServer )
		{
			if ( other is not BoomerPlayer player ) return;
			if ( player.Armour >= 100 ) return;

			var newhealth = player.Armour + 5;

			newhealth = newhealth.Clamp( 0, 100 );

			player.Armour = newhealth;

			PlayPickupSound();

			PickupFeed.OnPickup( To.Single( player ), $"+5 Armour" );
			ItemRespawn.Taken( this, RespawnTime );

			OnPickUpRpc( To.Single( other ) );

			Delete();
		}
	}

	[ClientRpc]
	public void OnPickUpRpc()
	{
		Host.AssertClient();
		_ = ChangedArmourAnim();
	}

	protected static async Task ChangedArmourAnim()
	{
		ArmourHud.Current.Value.SetClass( "gained", true );
		await GameTask.DelaySeconds( 0.25f );
		ArmourHud.Current.Value.SetClass( "gained", false );
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "armour.pickup", Position );
	}
}
