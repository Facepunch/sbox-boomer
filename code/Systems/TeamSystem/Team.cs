namespace Facepunch.Boomer;

public enum Team
{
	None,
	//
	Blue,
	Red,
	Green,
	Purple
}

public static class TeamExtensions
{
	public static Color GetColor( this Team team )
	{
		return team switch
		{
			Team.Blue => new Color( 5, 142, 217 ),
			Team.Red => new Color( 242, 66, 54 ),
			Team.Green => new Color( 106, 241, 144 ),
			Team.Purple => new Color( 147, 95, 167 ),
			_ => Color.Gray // incl None
		};
	}

	public static string GetName( this Team team )
	{
		return team switch
		{
			Team.Blue => "Blue Team",
			Team.Red => "Red Team",
			Team.Green => "Green Team",
			Team.Purple => "Purple Team",
			_ => null
		};
	}
}
