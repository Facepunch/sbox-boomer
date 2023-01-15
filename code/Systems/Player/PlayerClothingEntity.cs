using Sandbox;

namespace Facepunch.Boomer;

public partial class PlayerClothingEntity : ModelEntity
{
	[Net, Change( nameof( OnEntityColorChanged ) )] public Color EntityColor { get; set; }

	readonly static string ColorAttribute = "RimColor1";

	public override void Spawn()
	{
		base.Spawn();

		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		Tags.Add( "clothes" );
	}

	protected void OnEntityColorChanged( Color before, Color after )
	{
		Game.AssertClient();

		SceneObject?.Attributes.Set( ColorAttribute, after );
	}

	public override void OnNewModel( Model model )
	{
		SceneObject?.Attributes.Set( ColorAttribute, EntityColor );
	}
}
