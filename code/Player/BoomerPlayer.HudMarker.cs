using Boomer.UI;

namespace Boomer;

public partial class BoomerPlayer
{

	public string GetClass()
	{
		return "player";
	}

	bool CalculateVisibility()
	{
		var tr = Trace.Ray( CurrentView.Position, EyePosition )
			.WorldAndEntities()
			.Ignore( Local.Pawn )
			.Run();

		if ( tr.Hit && tr.Entity == this )
			return true;
		else
			return false;
	}

	public bool UpdateMarker( ref HudMarkerBuilder builder )
	{
		if ( !DeathmatchGame.IsTeamPlayEnabled ) 
			return false;

		if ( !this.IsValid() )
			return false;

		if ( LifeState != LifeState.Alive )
			return false;

		if ( !Client.IsFriend( Local.Client ) ) 
			return false;

		builder.Text = $"keyboard_double_arrow_down";
		builder.MaxDistance = 1000000f;
		builder.DistanceScale = 0.5f;
		builder.Position = Position + Vector3.Up * 100f;

		return true;
	}
}
