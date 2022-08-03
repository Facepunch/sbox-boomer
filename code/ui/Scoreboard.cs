
using Boomer;
using Sandbox.UI;

[UseTemplate]
public class Scoreboard : Panel
{

	bool Cursor;
	RealTimeSince timeSinceSorted;
	Dictionary<Client, ScoreboardEntry> Rows = new();

	public Panel Canvas { get; protected set; }
	public Panel Header { get; protected set; }

	public override void Tick()
	{
		base.Tick();

		SetClass( "open", ShouldBeOpen() );

		if ( !IsVisible )
			return;

		//
		// Clients that were added
		//
		foreach ( var client in Client.All.Except( Rows.Keys ) )
		{
			var entry = AddClient( client );
			Rows[client] = entry;
		}

		foreach ( var client in Rows.Keys.Except( Client.All ) )
		{
			if ( Rows.TryGetValue( client, out var row ) )
			{
				row?.Delete();
				Rows.Remove( client );
			}
		}

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

	private bool ShouldBeOpen()
	{
		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd )
			return true;

		if ( Input.Down( InputButton.Score ) )
			return true;

		return false;
	}

	private ScoreboardEntry AddClient( Client entry )
	{
		var p = Canvas.AddChild<ScoreboardEntry>();
		p.Client = entry;
		return p;
	}

	public void OpenSettings()
	{
		SettingsMenu.SetOpen( true );
	}

	public void ToggleSpectator()
	{
		DeathmatchGame.ToggleSpectator();
	}

}

public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{
	public ScoreboardEntry()
	{
		AddEventListener( "onclick", OnClick );
	}

	public void OnClick()
	{
		if ( Client == Local.Client ) return;

		if ( BoomerCamera.IsSpectator )
		{
			BoomerCamera.Target = Client.Pawn as BoomerPlayer;
		}
	}
}
