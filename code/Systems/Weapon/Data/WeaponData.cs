using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Boomer.WeaponSystem;

[GameResource( "Weapon", "weapon", "A data asset for a weapon.",
	Icon = "track_changes", IconBgColor = "#4953a7", IconFgColor = "#2a3060" )]
public partial class WeaponData : GameResource
{
	[Category( "Basic Information" )]
	public string Name { get; set; } = "My weapon name";

	[Category( "Basic Information" ), ResourceType( "vmdl" )]
	public string Model { get; set; }

	[Category( "Basic Information" ), ResourceType( "vmdl" )]
	public string ViewModel { get; set; }

	[Category( "Basic Information" ), ResourceType( "image" )]
	public string CrosshairIcon { get; set; }

	[Category( "Basic Information" ), ResourceType( "color" )]
	public Color WeaponColor { get; set; } = Color.White;

	internal Model CachedModel;
	internal Model CachedViewModel;

	[Category( "Basic Information" ), ResourceType( "jpg" )]
	public string Icon { get; set; }

	[Category( "Basic Information" ), ResourceType( "jpg" )]
	public string AmmoIcon { get; set; }

	[Category( "Animation" )]
	public HoldType HoldType { get; set; } = HoldType.Pistol;

	[Category( "Animation" )]
	public float HoldTypePose { get; set; } = 0;

	[Category( "Animation" )]
	public Handedness Handedness { get; set; } = Handedness.Both;

	[Category( "Basic Information" )]
	public List<string> Components { get; set; }

	public List<string> KillMessages { get; set; }

	public ViewModelData ViewModelData { get; set; }

	protected override void PostLoad()
	{
		base.PostLoad();

		Log.Info( $"Registering weapon ({ResourcePath}, {Name})" );

		if ( !All.Contains( this ) )
			All.Add( this );

		if ( !string.IsNullOrEmpty( Model ) )
			CachedModel = Sandbox.Model.Load( Model );

		if ( !string.IsNullOrEmpty( ViewModel ) )
			CachedViewModel = Sandbox.Model.Load( ViewModel );
	}

	public string GetRandomKillMessage()
	{
		return Game.Random.FromList( KillMessages, "killed" );
	}
}
