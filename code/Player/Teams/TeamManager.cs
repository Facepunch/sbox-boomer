namespace Boomer;

public partial class TeamManager : Entity
{
	public static TeamManager Current { get; set; }

	[Net] public IList<BoomerTeam> Teams { get; set; }
	[Net] public bool IsTeamPlayEnabled { get; set; }

	public TeamManager() => Current = this;

	public void NewTeam<T>() where T : BoomerTeam, new()
	{
		BoomerTeam team = new T();

		var sameTeam = Teams.FirstOrDefault( x => x.Name == team.Name );
		// We don't want two teams with the same identity
		if ( sameTeam != null )
		{
			return;
		}

		Teams.Add( team );

		// Let the game know that team play is enabled above 1 teams
		if ( Teams.Count > 1 )
		{
			IsTeamPlayEnabled = true;
		}
	}

	public BoomerTeam Get( string name ) => Teams.FirstOrDefault( x => x.Name == name );

	public bool AddMember( BoomerTeam team, Client cl ) => team?.AddMember( cl ) ?? false;
	public bool AddMember( string name, Client cl ) => AddMember( Get( name ), cl );

	public bool RemoveMember( BoomerTeam team, Client cl ) => team?.RemoveMember( cl ) ?? false;
	public bool RemoveMember( string name, Client cl ) => RemoveMember( Get( name ), cl );

	public void Clear()
	{
		// Nulling out a [Net]'d IList will clear the list.
		Teams = null;
		IsTeamPlayEnabled = false;
	}
}
