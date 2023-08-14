using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Boomer;

public partial class Player
{
	protected List<PlayerClothingEntity> ClothingEntities { get; set; } = new();

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

	public void AddClothing( string modelPath )
	{
		var entity = new PlayerClothingEntity();
		entity.SetModel( modelPath );
		entity.SetParent( this, true );
		entity.EntityColor = PlayerColor;

		ClothingEntities.Add( entity );
	}

	public void SetupClothing()
	{
		foreach ( var entity in ClothingEntities )
		{
			entity.Delete();
		}

		ClothingEntities.Clear();

		SetBodyGroup( "Hands", 1 );
		SetBodyGroup( "Feet", 1 );

		SetMaterialOverride( SkinMat, "skin" );
		SetMaterialOverride( EyeMat, "eyes" );

		AddClothing( "models/cosmetics/outfit/boomeroutfit_pants.vmdl" );
		AddClothing( "models/cosmetics/outfit/boomeroutfit_shoes.vmdl" );
		AddClothing( "models/cosmetics/outfit/boomeroutfit_helmet.vmdl" );
		AddClothing( "models/cosmetics/outfit/boomeroutfit_gloves.vmdl" );
		AddClothing( "models/cosmetics/outfit/boomeroutfit_chest.vmdl" );
	}
}
