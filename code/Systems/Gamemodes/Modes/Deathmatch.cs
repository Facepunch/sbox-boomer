using Sandbox;
using System.Threading.Tasks;

namespace Facepunch.Boomer.Gamemodes;

public partial class Deathmatch : Gamemode
{
	/// <summary>
	/// The current game state.
	/// </summary>
	[Net] public GameState CurrentState { get; set; }

	/// <summary>
	/// How long the countdown is after we've got enough players in-game.
	/// </summary>
	protected float GameCountdown => 10f;

	/// <summary>
	/// How long the game lasts, after that we'll decide the winner.
	/// </summary>
	protected float GameDuration => 300f;

	internal override void Initialize()
	{
		_ = GameLoop();
	}

	private async Task WaitForPlayers()
	{
		while ( PlayerCount < MinimumPlayers )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		// We're ready to start the game
	}

	private async Task WaitAsync( float time )
	{
		await Task.DelayRealtimeSeconds( time );
	}

	/// <summary>
	/// The core game loop for Deathmatch.
	/// </summary>
	protected async Task GameLoop()
	{
		// Wait until we've got players in the game.
		CurrentState = GameState.Warmup;
		Log.Info( "Waiting for players." );
		await WaitForPlayers();

		CurrentState = GameState.Countdown;

		// Make sure all the players respawn.
		RespawnAllPlayers();

		Chat.AddInformation( To.Everyone, $"The game will start in {GameCountdown} seconds." );
		await WaitAsync( GameCountdown );

		// The game's now active.
		CurrentState = GameState.GameActive;
		Chat.AddInformation( To.Everyone, $"The game begins." );

		await WaitAsync( GameDuration );

		// The game's over.
		CurrentState = GameState.GameOver;
		Chat.AddInformation( To.Everyone, $"Game over." );

		// TODO - Decide Winner
		// TODO - Map Vote

		// For now, we'll just restart the current map.
		Game.ChangeLevel( Game.Server.MapIdent );
	}

	/// <summary>
	/// Only allow damage if we're in warmup / active game.
	/// </summary>
	public override bool AllowDamage => CurrentState == GameState.Warmup || CurrentState == GameState.GameActive;

	/// <summary>
	/// Disallow movement while we're counting down.
	/// </summary>
	public override bool AllowMovement => CurrentState != GameState.Countdown;

	public enum GameState
	{
		Warmup,
		Countdown,
		GameActive,
		GameOver
	}
}
