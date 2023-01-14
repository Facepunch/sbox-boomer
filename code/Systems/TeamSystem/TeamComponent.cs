using Sandbox;

namespace Facepunch.Boomer;

public partial class TeamComponent : EntityComponent
{
	[Net, Change( nameof( OnTeamChanged ) )] private Team team { get; set; }

	public Team Team
	{
		get => team;
		set
		{
			var old = team;
			team = value;
			OnTeamChanged( old, team );
		}
	}

	protected void OnTeamChanged( Team before, Team after )
	{
		//
	}
}
