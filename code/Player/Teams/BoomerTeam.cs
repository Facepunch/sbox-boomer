namespace Boomer;

// TODO - Use BaseNetworkable
public partial class BoomerTeam : Entity
{
	public Color Color { get; set; }

	[Net] public IList<Client> Members { get; set; }

	public bool AddMember( Client cl )
	{
		Members.Add( cl );
		return true;
	}

	public bool RemoveMember( Client cl )
	{
		return Members.Remove( cl );
	}
}
