using Sandbox.UI.Construct;

public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
{
	protected override void AddHeader()
	{
		Header = Add.Panel( "header" );
		Header.Add.Label( "Player", "name" );
		Header.Add.Label( "Kills", "kills" );
		Header.Add.Label( "Deaths", "deaths" );
		Header.Add.Label( "Ping", "ping" );
	}

	bool Cursor;
	RealTimeSince timeSinceSorted;

	public override void Tick()
	{
		base.Tick();

		Style.PointerEvents = Cursor ? "all" : "none";

		if ( !HasClass( "open" ) ) Cursor = false;
		if ( !IsVisible ) return;

		if ( Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack ) )
		{
			Cursor = true;
		}

		if ( timeSinceSorted > 0.1f )
		{
			timeSinceSorted = 0;

			//
			// Sort by number of kills, then number of deaths
			//
			Canvas.SortChildren<ScoreboardEntry>( ( x ) => (-x.Client.GetInt( "kills" ) * 1000) + x.Client.GetInt( "deaths" ) );
		}
	}

	public override bool ShouldBeOpen()
	{
		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd )
			return true;

		return base.ShouldBeOpen();
	}

	[Event.BuildInput]
	public void OnBuildInput( InputBuilder bn )
	{
		if ( Cursor )
		{
			bn.ClearButton( InputButton.PrimaryAttack );
			bn.ClearButton( InputButton.SecondaryAttack );
		}
	}

}

public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{

}
