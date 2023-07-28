
using Sandbox;
using Sandbox.Html;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Xml.Linq;

public class MainMenuScene : ScenePanel
{
	public class Player
	{
		Friend Friend { get; set; }
		SceneModel CharacterModel { get; set; }
		List<SceneModel> Clothing { get; set; }
		Color Color { get; set; }

		public void SetupClothing( MainMenuScene sc )
		{
			AddClothing( sc, "models/cosmetics/outfit/boomeroutfit_pants.vmdl" );
			AddClothing( sc, "models/cosmetics/outfit/boomeroutfit_shoes.vmdl" );
			AddClothing( sc, "models/cosmetics/outfit/boomeroutfit_helmet.vmdl" );
			AddClothing( sc, "models/cosmetics/outfit/boomeroutfit_gloves.vmdl" );
			AddClothing( sc, "models/cosmetics/outfit/boomeroutfit_chest.vmdl" );

			CharacterModel.SetBodyGroup( "Hands", 1 );
			CharacterModel.SetBodyGroup( "Feet", 1 );
		}

		void AddClothing( MainMenuScene sc, string path )
		{
			var e = new SceneModel( sc.World, Model.Load( path ), CharacterModel.Transform );
			Clothing.Add( e );

			CharacterModel.AddChild( "clothing", e );


			e.Attributes.Set( "RimColor1", Color );
		}

		public Player( MainMenuScene sc, Friend friend, string model, Vector3 worldPos, Color color )
		{
			Friend = friend;
			Color = color;

			Log.Info( $"Hi {Friend.Name}" );
	
			CharacterModel = new SceneModel( 
				sc.World, Model.Load( model ), 
				Transform.Zero.WithPosition( worldPos ) );

			CharacterModel.SetAnimGraph( "animgraphs/mainmenu/citizen_mainmenu.vanmgrph" );

			Clothing = new();
			SetupClothing( sc );
		}

		~Player()
		{
			CharacterModel.Delete();
			Clothing.ForEach( x => x.Delete() );
			Clothing = null;
		}

		public void Tick()
		{
			var delta = Time.Delta;
			CharacterModel.Update( delta );
			Clothing.ForEach( x => x.Update( delta ) );
		}
	}

	public List<Player> Players { get; protected set; } = new();

	public List<Vector3> Slots => new()
	{
		new( 50f, 0, 0 ),
		new( 15f, -45f, 0 ),
		new( 15f, 45f, 0 ),
		new( 0f, -90f, 0 ),
		new( 0f, 90f, 0 )
	};

	public List<Color> Colors => new()
	{
		Color.Cyan,
		Color.Yellow,
		Color.Yellow,
		Color.Yellow,
		Color.Yellow
	};

	public Vector3 TargetPosition { get; set; }
	public Rotation TargetRotation { get; set; }

	public MainMenuScene()
	{
		World = new SceneWorld();

		new SceneSunLight( World, Rotation.From( 45, -180, 0 ), Color.Blue * 0.2f );

		//new SceneLight( World, Vector3.Up * 150.0f, 200.0f, Color.White * 2.0f );
		new SceneLight( World, Vector3.Up * 150.0f + Vector3.Forward * 100.0f, 512, new Color( 0.8f, 0.8f, 1 ) * 8.0f );
		//new SceneLight( World, Vector3.Up * 10.0f + Vector3.Right * 100.0f, 200, Color.White * 4.0f );
		// new SceneLight( World, Vector3.Up * 10.0f + Vector3.Left * 100.0f, 200, Color.White * 4.0f );
	}

	public override void OnDeleted()
	{
		Reset();

		World?.Delete();
		World = null;
	}

	public override void Tick()
	{
		if ( !IsVisible ) return;

		foreach ( var player in Players )
		{
			player.Tick();
		}

		Camera.Position = Camera.Position.LerpTo( TargetPosition, Time.Delta * 10f );
		Camera.Rotation = Rotation.Slerp( Camera.Rotation, TargetRotation, Time.Delta );
	}

	public void AddPlayer( Friend friend )
	{
		Players.Add( new Player( this, friend, "models/citizen/citizen.vmdl", Slots[Players.Count], Colors[Players.Count] ) );
	}

	public void Reset()
	{
		if ( Players.Count < 1 ) return;
		Players = new();
	}
}
