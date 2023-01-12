using Facepunch.Boomer;
using Sandbox;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Facepunch.Boomer;

public partial class GamemodeSystem
{
	[ConVar.Replicated( "boomer_gamemode" )]
	public static string SelectedGamemode { get; set; } = "";

	private static Gamemode current;
	public static Gamemode Current
	{
		get
		{
			if ( Game.IsServer ) return current;

			if ( !current.IsValid() )
				current = Entity.All.FirstOrDefault( x => x is Gamemode ) as Gamemode;

			return current;
		}
		set
		{
			current = value;
		}
	}

	protected static Gamemode FetchGamemodeEntity()
	{
		// First, see if the map has a gamemode we want to use already
		var gamemode = Entity.All.FirstOrDefault( x => x is Gamemode ) as Gamemode;

		// If not, use game preferences to create one.
		if ( !gamemode.IsValid() && !string.IsNullOrEmpty( SelectedGamemode ) )
		{
			var gamemodeEntity = TypeLibrary.Create<Gamemode>( SelectedGamemode );
			if ( gamemodeEntity.IsValid() )
			{
				Log.Info( $"Boomer: Found gamemode from TypeLibrary - {SelectedGamemode}" );
				gamemode = gamemodeEntity;
			}
			else
			{
				Log.Warning( "Boomer: No gamemode found while fetching." );
			}
		}

		return gamemode;
	}

	public static void SetupGamemode()
	{
		Current = FetchGamemodeEntity();

		if ( Current.IsValid() )
		{
			Current.Initialize();
		}
	}
}
