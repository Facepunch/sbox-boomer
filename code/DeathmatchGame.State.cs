namespace Boomer;

partial class DeathmatchGame : Game
{
	public static GameStates CurrentState => (Current as DeathmatchGame)?.GameState ?? GameStates.Warmup;

	[Net]
	public RealTimeUntil StateTimer { get; set; } = 0f;

	[Net]
	public GameStates GameState { get; set; } = GameStates.Warmup;
	[Net]
	public string NextMap { get; set; } = "facepunch.bm_dockyard";

	[ConCmd.Admin]
	public static void SkipStage()
	{
		if ( Current is not DeathmatchGame dmg ) return;

		dmg.StateTimer = 1;
	}

	private async Task WaitStateTimer()
	{
		while ( StateTimer > 0 )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		// extra second for fun
		await Task.DelayRealtimeSeconds( 1.0f );
	}

	[ConVar.Replicated( "bm_warmuptime", Min = 0, Max = 600 )]
	public static float WarmupTime { get; set; } = 30f;

	[ConVar.Replicated( "bm_gametime", Min = 0, Max = 1800 )]
	public static float GameTime { get; set; } = 600f;

	[ConVar.Replicated( "bm_roundendtime", Min = 0, Max = 1800 )]
	public static float RoundEndTime { get; set; } = 10f;

	[ConVar.Replicated( "bm_mapvotetime", Min = 0, Max = 1800 )]
	public static float MapVoteTime { get; set; } = 10f;

	private async Task GameLoopAsync()
	{
		GameState = GameStates.Warmup;
		StateTimer = WarmupTime;
		CountDownPlayed = false;
		PreMapCheck();
		await WaitStateTimer();

		GameState = GameStates.Live;
		MapCheck();
		StateTimer = GameTime;
		CountDownPlayed = false;
		FreshStart();
		if ( RailTag )
		{
			StartTag();
		}
		if ( MasterBall )
		{
			StartMasterBall();
		}
		await WaitStateTimer();

		GameState = GameStates.GameEnd;
		StateTimer = RoundEndTime;
		_ = SubmitScore();
		await WaitStateTimer();

		GameState = GameStates.MapVote;
		var mapVote = new MapVoteEntity();
		mapVote.VoteTimeLeft = MapVoteTime;
		StateTimer = mapVote.VoteTimeLeft;
		await WaitStateTimer();

		Global.ChangeLevel( mapVote.WinningMap );
	}

	private bool HasEnoughPlayers()
	{
		if ( All.OfType<BoomerPlayer>().Count() < 2 )
			return false;
		
		return true;
	}

	private async Task SubmitScore()
	{
		if ( ScoreSystemDisabled || DeathmatchGame.RailTag || DeathmatchGame.MasterBall) return;

		foreach ( var cl in Client.All )
		{
			var Kills = cl.GetInt( "kills" );
			var Deaths = cl.GetInt( "deaths" );

			var ScoreToSubmit = Kills - Deaths;
			
			var scoreResult = await GameServices.SubmitScore( cl.PlayerId, ScoreToSubmit );
			
		}
		
	}

	private void FreshStart(){
		foreach ( var cl in Client.All )
		{
			cl.SetInt( "kills", 0 );
			cl.SetInt( "deaths", 0 );
		}

		All.OfType<BoomerPlayer>().ToList().ForEach( x =>
		{
			x.Respawn();
		} );

		HasFirstPlayerDied = false;
	}

	public enum GameStates
	{
		Warmup,
		Live,
		GameEnd,
		MapVote
	}

}
