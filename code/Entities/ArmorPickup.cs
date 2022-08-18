namespace Boomer;

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

	public override void OnPickup( BoomerPlayer player )
	{
		var newhealth = player.Armour + ArmorGranted;
		newhealth = newhealth.Clamp( 0, 100 );
		player.Armour = newhealth;

		PlayPickupSound();
		PickupFeed.OnPickup( To.Single( player ), $"+25 Armour" );
		OnPickUpRpc( To.Single( player ) );

		base.OnPickup( player );
	}

	public override bool CanPickup( BoomerPlayer player )
	{
		if ( player.Armour >= 100 ) return false;

		return base.CanPickup( player );
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
