using Sandbox;
using System;

namespace Facepunch.Boomer;

public partial class TeamSystem
{
	public static Team GetTeam( IClient cl )
	{
		return cl.Components.Get<TeamComponent>()?.Team ?? Team.None;
	}

	public static bool IsFriendly( Team one, Team two )
	{
		if ( one == Team.None || two == Team.None ) return false;
		return one == two;
	}

	public static Team GetLowestCount()
	{
		var currentTeam = Team.None;
		int lowestCount = 999;

		foreach ( var team in Enum.GetValues<Team>() )
		{
			if ( team == Team.None ) continue;

			var count = team.Count();
			if ( count < lowestCount )
			{
				currentTeam = team;
				lowestCount = count;
			}
		}

		return currentTeam;
	}
}
