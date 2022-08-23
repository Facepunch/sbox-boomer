namespace Boomer;

public partial class TeamManager : Entity
{
	public static TeamManager Current { get; set; }

	[Net] public IList<BoomerTeam> Teams { get; set; }
	[Net] public bool IsTeamPlayEnabled { get; set; }
	public bool AutoJoinTeam { get; set; } = true;

	public TeamManager() => Current = this;

	public void SetupTeam<T>() where T : BoomerTeam, new()
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

	protected Comparison<BoomerTeam> TeamSorter;
	public void SetTeamSorter( Comparison<BoomerTeam> comparison )
	{
		TeamSorter = comparison;
	}

	// TODO - Allow gamemodes to specify different balancing rules
	public BoomerTeam FindBalancedTeam()
	{
		var teams = Teams.ToList();

		if ( TeamSorter != null )
			teams.Sort( TeamSorter );
		else
			teams.OrderBy( x => x.Count );

		return teams.First();
	}

	public void OnClientJoined( Client cl )
	{
		if ( !IsTeamPlayEnabled ) return;

		if ( AutoJoinTeam )
		{
			var team = FindBalancedTeam();
			if ( !AddMember( team, cl ) )
			{
				Log.Error( $"Something went wrong while adding {cl} to {team}" );
			}
		}
	}

	public void OnClientDisconnect( Client cl )
	{
		if ( !IsTeamPlayEnabled ) return;

		var team = cl.GetTeam();
		if ( !RemoveMember( team, cl ) )
		{
			Log.Error( $"Something went wrong while removing {cl} from {team}" );
		}
	}

	public void Clear()
	{
		// Nulling out a [Net]'d IList will clear the list.
		Teams = null;
		IsTeamPlayEnabled = false;
	}
}
