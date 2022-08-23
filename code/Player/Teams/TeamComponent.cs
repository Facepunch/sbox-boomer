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
}
