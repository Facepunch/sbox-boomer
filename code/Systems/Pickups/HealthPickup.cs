using Editor;
using Sandbox;

namespace Facepunch.Boomer;

/// <summary>
/// Gives 25 health points.
/// </summary>
[Library( "boomer_healthkit" ), HammerEntity]
[EditorModel( "models/gameplay/healthkit/healthkit.vmdl" )]
[Title( "Health Kit" ), Category( "PickUps" )]
partial class HealthKit : BasePickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/healthkit/healthkit.vmdl" );
	public float HealthGranted { get; set; } = 25f;

	public override void OnPickup( Player player )
	{
		player.Health = (player.Health + HealthGranted).Clamp( 0, 100 );

		PlayPickupSound();
		OnPickUpRpc( To.Single( player ) );

		base.OnPickup( player );
	}

	public override bool CanPickup( Player player )
	{
		if ( player.Health >= 100 ) return false;
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
		Sound.FromWorld( "health.pickup", Position );
	}
}

/// <summary>
/// Gives 5 health points.
/// </summary>
[Library( "boomer_healthvial" ), HammerEntity]
[EditorModel( "models/gameplay/healthvial/healthvial.vmdl" )]
[Title( "Health Vial" ), Category( "PickUps" )]
partial class HealthVial : HealthKit
{
	public override Model WorldModel => Model.Load( "models/gameplay/healthvial/healthvial.vmdl" );

	public override void Spawn()
	{
		base.Spawn();

		RespawnTime = 15;
		HealthGranted = 5f;
	}
}

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

		if ( other is not Player player )
			return;

		if ( CanPickup( player ) )
		{
			OnPickup( player );
			Timer = Particles.Create( "particles/gameplay/respawnvisual/respawn_timer.vpcf", Position + new Vector3( 0, 0, 32 ) );
			Timer.SetPosition( 1, new Vector3( RespawnTime, 1, 0 ) );
			Timer.SetPosition( 2, new Vector3( 0, 255, 255 ) );
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
		player.Health = (player.Health + HealthGranted).Clamp( 0, 200 );

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
