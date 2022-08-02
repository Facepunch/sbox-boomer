namespace Boomer;

/// <summary>
/// Gives 25 Armour
/// </summary>
[Library( "dm_battery" ), HammerEntity]
[EditorModel( "models/dm_battery.vmdl" )]
[Title(  "Battery" )]
partial class Battery : AnimatedEntity, IRespawnableEntity
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/armour/armourkit.vmdl" );
	
	public int RespawnTime = 30;
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;

		PhysicsEnabled = true;
		UsePhysicsCollision = true;

		Tags.Add( "trigger" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( IsServer )
		{
			if ( other is not BoomerPlayer player ) return;
			if ( player.Armour >= 100 ) return;

			var newhealth = player.Armour + 25;

			newhealth = newhealth.Clamp( 0, 100 );

			player.Armour = newhealth;

			PlayPickupSound();

			PickupFeed.OnPickup( To.Single( player ), $"+25 Armour" );
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
