﻿namespace Boomer;

partial class DeathmatchGame : GameManager
{
	public static GameStates CurrentState => Current?.GameState ?? GameStates.Warmup;

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

	[ConVar.Replicated( "bm_gametime" )]
	public static int GameTime { get; set; } = 10;

	[ConVar.Replicated( "bm_roundendtime", Min = 0, Max = 1800 )]
	public static float RoundEndTime { get; set; } = 10f;

	[ConVar.Replicated( "bm_mapvotetime", Min = 0, Max = 1800 )]
	public static float MapVoteTime { get; set; } = 10f;

	private async Task GameLoopAsync()
	{
		GameState = GameStates.Warmup;
		StateTimer = WarmupTime;
		CountDownPlayed = false;
		await WaitStateTimer();

		GameState = GameStates.Live;
		PreMapCheck();
		StateTimer = GameTime * 60;
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

		MapCheck();
		GameState = GameStates.GameEnd;
		StateTimer = RoundEndTime;
		_ = SubmitScore();
		await WaitStateTimer();

		GameState = GameStates.MapVote;
		var mapVote = new MapVoteEntity();
		mapVote.VoteTimeLeft = MapVoteTime;
		StateTimer = mapVote.VoteTimeLeft;
		await WaitStateTimer();

		Game.ChangeLevel( mapVote.WinningMap );
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

		foreach ( var cl in Game.Clients )
		{
			var Kills = cl.GetInt( "kills" );
			var Deaths = cl.GetInt( "deaths" );

			var ScoreToSubmit = Kills - Deaths;
			
			// TODO - Replace me
			//var scoreResult = await GameServices.RecordScore( cl.PlayerId, ScoreToSubmit );
		}
		
	}

	private void FreshStart(){
		foreach ( var cl in Game.Clients )
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

	[ConCmd.Server]
	private static void AddTime()
	{
		DeathmatchGame.Current.StateTimer += 9999;
	}

	public enum GameStates
	{
		Warmup,
		Live,
		GameEnd,
		MapVote
	}

}
