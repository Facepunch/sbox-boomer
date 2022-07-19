using Boomer.Movement;
/// <summary>
/// Gives 25 health points.
/// </summary>
[Library( "bm_megahealth" ), HammerEntity]
[EditorModel( "models/gameplay/healthkit/healthkit.vmdl" )]
[Title( "Mega Health" )]
partial class MegaHealth : ModelEntity, IRespawnableEntity
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/healthkit/healthkit.vmdl" );

	public int RespawnTime = 240;
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;

		PhysicsEnabled = true;
		UsePhysicsCollision = true;

		Tags.Add( "weapon" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not BoomerPlayer pl ) return;
		if ( pl.Health >= pl.MaxHealth ) return;

		var newhealth = pl.Health + 200;

		newhealth = newhealth.Clamp( 0, pl.MaxHealth );

		pl.Health = newhealth;

		PickEffect( pl );

		Sound.FromWorld( "dm.item_health", Position );
		PickupFeed.OnPickup( To.Single( pl ), $"+Mega Health" );
		ItemRespawn.Taken( this , RespawnTime);
		Delete();
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
