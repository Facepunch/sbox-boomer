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
