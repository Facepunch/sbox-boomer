namespace Boomer;

public partial class BoomerPlayer
{
	// TODO - make ragdolls one per entity
	// TODO - make ragdolls dissapear after a load of seconds
	static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 20 };

	

	[ClientRpc]
	void BecomeRagdollOnClient( Vector3 force, Color PlyColor )
	{
		// TODO - lets not make everyone write this shit out all the time
		// maybe a CreateRagdoll<T>() on ModelEntity?
		var ent = new ModelEntity();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.UsePhysicsCollision = true;
		Tags.Add( "debris" );
		ent.PhysicsEnabled = true;
		
		ent.CopyFrom( this );
		ent.CopyBonesFrom( this );
		ent.SetRagdollVelocityFrom( this );
		ent.DeleteAsync( 20.0f );


		//
		//Dress and Colour the player.
		//

		ent.SceneObject.Attributes.Set( "RimColor", PlyColor );
		ent.SetBodyGroup( "Hands", 1 );
		ent.SetBodyGroup( "Feet", 1 );

		ModelEntity pants = new ModelEntity();
		pants.SetModel( "models/cosmetics/outfit/boomeroutfit_pants.vmdl" );
		pants.SetParent( ent, true );
		pants.SceneObject.Attributes.Set( "RimColor1", PlyColor );
		
		ModelEntity shoes = new ModelEntity();
		shoes.SetModel( "models/cosmetics/outfit/boomeroutfit_shoes.vmdl" );
		shoes.SetParent( ent, true );
		shoes.SceneObject.Attributes.Set( "RimColor1", PlyColor );

		ModelEntity helmet = new ModelEntity();
		helmet.SetModel( "models/cosmetics/outfit/boomeroutfit_helmet.vmdl" );
		helmet.SetParent( ent, true );
		helmet.SceneObject.Attributes.Set( "RimColor1", PlyColor );

		ModelEntity gloves = new ModelEntity();
		gloves.SetModel( "models/cosmetics/outfit/boomeroutfit_gloves.vmdl" );
		gloves.SetParent( ent, true );
		gloves.SceneObject.Attributes.Set( "RimColor1", PlyColor );

		ModelEntity chest = new ModelEntity();
		chest.SetModel( "models/cosmetics/outfit/boomeroutfit_chest.vmdl" );
		chest.SetParent( ent, true );
		chest.SceneObject.Attributes.Set( "RimColor1", PlyColor );

		ent.PhysicsGroup.AddVelocity( force );

		Corpse = ent;
		RagdollLimit.Watch( ent );
	}
}
