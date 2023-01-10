using Sandbox;

namespace Facepunch.Boomer;

public partial class Player
{
	public ClothingContainer Clothing { get; protected set; }

	/// <summary>
	/// Set the clothes to whatever the player is wearing
	/// </summary>
	public void UpdateClothes()
	{
		Clothing = new();

		Clothing.ClearEntities();
		Clothing.LoadFromClient( Client );
		
		PlayerColor = Color.White;
		SetupClothing();
	}

	[Net] public Color PlayerColor { get; set; }

	private static Material SkinMat = Material.Load( "models/gameplay/citizen/textures/citizen_skin.vmat" );
	private static Material EyeMat = Material.Load( "models/gameplay/citizen/textures/eyes/citizen_eyes_advanced.vmat" );

	public void SetupClothing()
	{
		SetBodyGroup( "Hands", 1 );
		SetBodyGroup( "Feet", 1 );

		SetMaterialOverride( SkinMat, "skin" );
		SetMaterialOverride( EyeMat, "eyes" );

		var pants = new ModelEntity();
		pants.SetModel( "models/cosmetics/outfit/boomeroutfit_pants.vmdl" );
		pants.SetParent( this, true );
		pants.EnableHideInFirstPerson = true;
		//pants.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		pants.EnableShadowInFirstPerson = true;
		pants.Tags.Add( "clothes" );

		var shoes = new ModelEntity();
		shoes.SetModel( "models/cosmetics/outfit/boomeroutfit_shoes.vmdl" );
		shoes.SetParent( this, true );
		shoes.EnableHideInFirstPerson = true;
		//shoes.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		shoes.EnableShadowInFirstPerson = true;
		shoes.Tags.Add( "clothes" );

		var helmet = new ModelEntity();
		helmet.SetModel( "models/cosmetics/outfit/boomeroutfit_helmet.vmdl" );
		helmet.SetParent( this, true );
		helmet.EnableHideInFirstPerson = true;
		//helmet.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		helmet.EnableShadowInFirstPerson = true;
		helmet.Tags.Add( "clothes" );

		var gloves = new ModelEntity();
		gloves.SetModel( "models/cosmetics/outfit/boomeroutfit_gloves.vmdl" );
		gloves.SetParent( this, true );
		gloves.EnableHideInFirstPerson = true;
		//gloves.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		gloves.EnableShadowInFirstPerson = true;
		gloves.Tags.Add( "clothes" );

		var chest = new ModelEntity();
		chest.SetModel( "models/cosmetics/outfit/boomeroutfit_chest.vmdl" );
		chest.SetParent( this, true );
		chest.EnableHideInFirstPerson = true;
		//chest.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		chest.EnableShadowInFirstPerson = true;
		chest.Tags.Add( "clothes" );
	}
}
