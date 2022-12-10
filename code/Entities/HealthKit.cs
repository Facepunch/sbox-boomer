using Boomer.Movement;

namespace Boomer;

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
	
	public override void OnPickup( BoomerPlayer player )
	{
		var newhealth = player.Health + HealthGranted;
		newhealth = newhealth.Clamp( 0, 100 );
		player.Health = newhealth;

		PlayPickupSound();
		PickupFeed.OnPickup( To.Single( player ), $"+{HealthGranted} Health" );
		OnPickUpRpc( To.Single( player ) );

		base.OnPickup( player );
	}

	public override bool CanPickup( BoomerPlayer player )
	{
		if ( player.Health >= 100 ) return false;

		return base.CanPickup( player );
	}

	[ClientRpc]
	public void OnPickUpRpc()
	{
		Game.AssertClient();
		_ = ChangedHealthAnim();
	}

	protected static async Task ChangedHealthAnim()
	{
		HealthHud.Current.Value.SetClass( "gained", true );
		await GameTask.DelaySeconds( 0.25f );
		HealthHud.Current.Value.SetClass( "gained", false );
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "health.pickup", Position );
	}
}
