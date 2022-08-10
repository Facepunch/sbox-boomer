using Boomer.Movement;

namespace Boomer;

/// <summary>
/// Gives 25 health points.
/// </summary>
[Library( "boomer_healthkit" ), HammerEntity]
[EditorModel( "models/gameplay/healthkit/healthkit.vmdl" )]
[Title( "Health Kit" ), Category( "PickUps" )]
partial class HealthKit : AnimatedEntity, IRespawnableEntity
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/healthkit/healthkit.vmdl" );

	[Property]
	public int RespawnTime { get; set; } = 30;
	
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
			if ( other is not BoomerPlayer pl ) return;
			if ( pl.Health >= 100 ) return;
			
			var newhealth = pl.Health + 25;

			newhealth = newhealth.Clamp( 0, 100 );

			pl.Health = newhealth;

			PickEffect( pl );
			PlayPickupSound();

			PickupFeed.OnPickup( To.Single( pl ), $"+25 Health" );
			ItemRespawn.Taken( this, RespawnTime );

			OnPickUpRpc( To.Single( other ) );

			if ( Host.IsServer )

			Delete();
		}
	}
	
	[ClientRpc]
	public void OnPickUpRpc()
	{
		Host.AssertClient();
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

	private void PickEffect( BoomerPlayer player )
	{
		if ( player.Controller is not BoomerController ctrl ) 
		return;

		if ( Host.IsServer || !player.IsLocalPawn )
		return;

		Particles.Create( "particles/gameplay/screeneffects/healthpickup/ss_healthpickup.vpcf",ctrl.Pawn);
	}
}
