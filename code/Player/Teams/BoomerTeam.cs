namespace Boomer;

// TODO - Use BaseNetworkable
public partial class BoomerTeam : Entity
{
	public virtual new string Name { get; set; }
	public virtual Color Color { get; set; } = Color.White;

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

	public int CompareTo( BoomerTeam y )
	{
		if ( this.Count > y.Count )
			return 1;

		if ( this.Count < y.Count )
			return -1;

		return 0;
	}


	public override bool Equals( object obj )
	{
		return (obj is BoomerTeam team) && team.Name == Name;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public bool IsFriend( BoomerTeam other )
	{
		if ( this.Equals( other ) ) return true;

		return false;
	}

	public override string ToString()
	{
		return $"Team {Name}";
	}

	// Preset Teams

	public partial class Red : BoomerTeam
	{
		public override string Name => "Red";
		public override Color Color => Color.Red;
	}

	public partial class Blue : BoomerTeam
	{
		public override string Name => "Blue";
		public override Color Color => Color.Blue;
	}

}
