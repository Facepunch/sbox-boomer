using Boomer.Movement;

namespace Boomer;

/// <summary>
/// Gives 25 health points.
/// </summary>
//[Library( "boomer_quaddamage" ), HammerEntity]
//[EditorModel( "models/gameplay/healthkit/healthkit.vmdl" )]
//[Title( "Quad Damage" ), Category( "PickUps" )]
partial class QuadDamage : BasePickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/healthkit/healthkit.vmdl" );


	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;

		PhysicsEnabled = true;
		UsePhysicsCollision = true;

		UntilRespawn = 60;

		Tags.Add( "trigger" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( Game.IsServer )
		{
			if ( other is not BoomerPlayer pl ) return;
			if ( pl.HasQuadDamage ) return;
			
			pl.HasQuadDamage = true;

			PickEffect( pl );
			PlayPickupSound();

			PickupFeed.OnPickup( To.Single( pl ), $"QUAD DAMAGE!" );
			ItemRespawn.Taken( this, RespawnTime );

			Delete();
		}
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

		if ( Game.IsServer || !player.IsLocalPawn )
		return;

		Particles.Create( "particles/gameplay/screeneffects/healthpickup/ss_healthpickup.vpcf",ctrl.Pawn);
	}
}
