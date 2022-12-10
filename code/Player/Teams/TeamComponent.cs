namespace Boomer;

public partial class TeamComponent : EntityComponent
{
	[Net] public BoomerTeam Team { get; set; }
}

public static class ClientExtensions
{
	public static BoomerTeam GetTeam( this IClient cl )
	{
		var comp = cl.Components.GetOrCreate<TeamComponent>();
		return comp.Team;
	}
	
	public static void SetTeam( this IClient cl, BoomerTeam team )
	{
		var comp = cl.Components.GetOrCreate<TeamComponent>();
		comp.Team = team;
	}

	public static bool IsFriend( this IClient cl, IClient otherCl )
	{
		var team = cl.GetTeam();
		if ( team == null ) return false;

		var otherTeam = otherCl.GetTeam();
		return team.IsFriend( otherTeam );
	}
}
