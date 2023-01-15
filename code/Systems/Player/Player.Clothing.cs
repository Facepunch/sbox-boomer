using Sandbox;
using System.Linq;

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
		
		PlayerColor = Color.Random;
		SetupClothing();
	}

	[Net] private Color playerColor { get; set; }
	public Color PlayerColor
	{
		get => playerColor;
		set
		{
			playerColor = value;
			UpdateClothing();
		}
	}

	private static Material SkinMat = Material.Load( "models/gameplay/citizen/textures/citizen_skin.vmat" );
	private static Material EyeMat = Material.Load( "models/gameplay/citizen/textures/eyes/citizen_eyes_advanced.vmat" );

	public void UpdateClothing()
	{
		foreach ( var ent in Children.OfType<PlayerClothingEntity>() )
		{
			ent.EntityColor = PlayerColor;
		}
	}

	public void SetupClothing()
	{
		SetBodyGroup( "Hands", 1 );
		SetBodyGroup( "Feet", 1 );

		SetMaterialOverride( SkinMat, "skin" );
		SetMaterialOverride( EyeMat, "eyes" );

		var pants = new PlayerClothingEntity();
		pants.SetModel( "models/cosmetics/outfit/boomeroutfit_pants.vmdl" );
		pants.SetParent( this, true );
		pants.EntityColor = PlayerColor;

		var shoes = new PlayerClothingEntity();
		shoes.SetModel( "models/cosmetics/outfit/boomeroutfit_shoes.vmdl" );
		shoes.SetParent( this, true );
		shoes.EntityColor = PlayerColor;

		var helmet = new PlayerClothingEntity();
		helmet.SetModel( "models/cosmetics/outfit/boomeroutfit_helmet.vmdl" );
		helmet.SetParent( this, true );
		helmet.EntityColor = PlayerColor;

		var gloves = new PlayerClothingEntity();
		gloves.SetModel( "models/cosmetics/outfit/boomeroutfit_gloves.vmdl" );
		gloves.SetParent( this, true );
		gloves.EntityColor = PlayerColor;

		var chest = new PlayerClothingEntity();
		chest.SetModel( "models/cosmetics/outfit/boomeroutfit_chest.vmdl" );
		chest.SetParent( this, true );
		chest.EntityColor = PlayerColor;
	}
}
