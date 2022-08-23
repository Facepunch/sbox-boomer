namespace Boomer;

// TODO - Use BaseNetworkable
public partial class BoomerTeam : Entity
{
	public virtual new string Name { get; set; }

	public Color Color { get; set; }
	[Net] public IList<Client> Members { get; set; }

	public int Count => Members.Count;

	public bool AddMember( Client cl )
	{
		Members.Add( cl );
		// Inform the team component
		cl.SetTeam( this );

		return true;
	}

	public bool RemoveMember( Client cl )
	{
		var result = Members.Remove( cl );

		if ( result )
		{
			// Inform the team component
			cl.SetTeam( null );
		}

		return result;
	}

	public partial class Red : BoomerTeam
	{
		public override string Name => "Red";
	}

	public partial class Blue : BoomerTeam
	{
		public override string Name => "Blue";
	}

	public int CompareTo( BoomerTeam y )
	{
		if ( this.Count > y.Count )
			return 1;

		if ( this.Count < y.Count )
			return -1;

		return 0;
	}
}
