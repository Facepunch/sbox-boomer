using Boomer;

public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{
	public ScoreboardEntry()
	{
		AddEventListener( "onclick", OnClick );
	}

	public void OnClick()
	{
		if ( Client == Game.LocalPawn ) return;

		if ( BoomerCamera.IsSpectator )
		{
			BoomerCamera.Target = Client.Pawn as BoomerPlayer;
		}
	}
}
