/// <summary>
/// Gives 25 Armour
/// </summary>
[Library( "dm_battery" ), HammerEntity]
[EditorModel( "models/dm_battery.vmdl" )]
[Title(  "Battery" )]
partial class Battery : AnimatedEntity, IRespawnableEntity
{
	public static readonly Model WorldModel = Model.Load( "models/dm_battery.vmdl" );
	
	public int RespawnTime = 30;
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;

		PhysicsEnabled = false;
		UsePhysicsCollision = true;

		Tags.Add( "trigger" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( IsServer )
		{
			if ( other is not BoomerPlayer player ) return;
			if ( player.Armour >= player.MaxArmour ) return;

			var newhealth = player.Armour + 25;

			newhealth = newhealth.Clamp( 0, 200 );

			player.Armour = newhealth;

			PlayPickupSound();

			PickupFeed.OnPickup( To.Single( player ), $"+25 Armour" );
			ItemRespawn.Taken( this, RespawnTime );

			Delete();
		}
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "armour.pickup", Position );
	}
}
