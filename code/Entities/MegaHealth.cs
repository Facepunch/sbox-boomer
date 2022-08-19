using Boomer.Movement;

namespace Boomer;

/// <summary>
/// Gives 200 health points.
/// </summary>
[Library( "boomer_megahealth" ), HammerEntity]
[EditorModel( "models/gameplay/mega_health/mega_health.vmdl" )]
[Title( "Mega Health" ), Category( "PickUps" )]
partial class MegaHealth : BasePickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/mega_health/mega_health.vmdl" );

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

		Timer = Particles.Create( "particles/gameplay/respawnvisual/respawn_timer.vpcf", Position + new Vector3( 0, 0, 32 ) );
		Timer.SetPosition( 1, new Vector3( RespawnTime - UntilRespawn, 1, 0 ) );
		Timer.SetPosition( 2, new Vector3( 0, 255, 255 ) );
	}
	
	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );
		if ( other is not BoomerPlayer player )
			return;

		if ( CanPickup( player ) )
		{
			OnPickup( player );
			Timer = Particles.Create( "particles/gameplay/respawnvisual/respawn_timer.vpcf", Position + new Vector3(0,0,32) );
			Timer.SetPosition( 1, new Vector3( RespawnTime, 1, 0 ) );
			Timer.SetPosition( 2, new Vector3( 0, 255, 255 ) );
		}
	}
	
	[Event.Tick.Client]
	public void DestroyTimer()
	{
		if ( Available )
		{
			Timer.SetPosition( 1, new Vector3( 0, 2, 1 ) );
			Timer.Destroy();
		}
	}
	public override void OnPickup( BoomerPlayer player )
	{
		var newhealth = player.Health + HealthGranted;
		newhealth = newhealth.Clamp( 0, 200 );
		player.Health = newhealth;

		PlayPickupSound();
		PickupFeed.OnPickup( To.Single( player ), $"+Mega Health" );

		Timer.Destroy( true );

		base.OnPickup( player );
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "megahealth.pickup", Position );
	}
}
