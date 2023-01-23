using System.Collections.Generic;
using Sandbox;
using System.Linq;

namespace Facepunch.Boomer;

public enum Team
{
	None,
	//
	Cyan,
	Yellow,
	Green,
	Purple
}

public static class TeamExtensions
{
	public static Color GetColor( this Team team )
	{
		return team switch
		{
			Team.Cyan => new Color32( 20, 240, 240 ),
			Team.Yellow => new Color32( 240, 240, 20 ),
			Team.Green => new Color32( 106, 241, 144 ),
			Team.Purple => new Color32( 147, 95, 167 ),
			_ => new Color32( 200, 200, 200 ) // incl None
		};
	}

	public static string GetName( this Team team )
	{
		return team switch
		{
			Team.Cyan => "Team Cyan",
			Team.Yellow => "Team Yellow",
			Team.Green => "Team Green",
			Team.Purple => "Team Purple",
			_ => null
		};
	}

	public static IEnumerable<IClient> GetClients( this Team team )
	{
		return Game.Clients.Where( x => TeamSystem.GetTeam( x ) == team );
	}

	public static int Count( this Team team )
	{
		return GetClients( team )
			.Count();
	}
}
