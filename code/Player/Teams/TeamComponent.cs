namespace Boomer;

public partial class TeamComponent : EntityComponent
{
	[Net] public BoomerTeam Team { get; set; }
}

public static class ClientExtensions
{
	public static BoomerTeam GetTeam( this Client cl )
	{
		var comp = cl.Components.GetOrCreate<TeamComponent>();
		return comp.Team;
	}
	
	public static void SetTeam( this Client cl, BoomerTeam team )
	{
		var comp = cl.Components.GetOrCreate<TeamComponent>();
		comp.Team = team;
	}

	public static bool IsFriend( this Client cl, Client otherCl )
	{
		var team = cl.GetTeam();
		if ( team == null ) return false;

		var otherTeam = otherCl.GetTeam();
		return team.IsFriend( otherTeam );
	}
}
