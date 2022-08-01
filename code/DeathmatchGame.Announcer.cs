global using Sandbox;
global using SandboxEditor;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
using Boomer.Movement;
using Boomer.UI;

/// <summary>
/// This is the heart of the gamemode. It's responsible for creating the player and stuff.
/// </summary>
partial class DeathmatchGame : Game
{

	[Net]
	private bool CountDownPlayed { get; set; } = false;

	[Net]
	private bool FiveWarnPlayed { get; set; } = false;

	[Net]
	private bool TwoWarnPlayed { get; set; } = false;

	[Net]
	private bool OneWarnPlayed { get; set; } = false;

	[Net]
	private bool TenWarnPlayed { get; set; } = false;

	[Event.Tick.Server]
	public void Tick()
	{
		if ( StateTimer <= 4 && !CountDownPlayed && CurrentState == GameStates.Warmup )
		{
			CountDownPlayed = true;
			Sound.FromScreen( "countdown" );
			_ = RoundPlay();
		}

		if ( StateTimer <= 300 && !FiveWarnPlayed && CurrentState == GameStates.Live )
		{
			FiveWarnPlayed = true;
			Sound.FromScreen( "5_minute_warning" );
		}

		if ( StateTimer <= 120 && !TwoWarnPlayed && CurrentState == GameStates.Live )
		{
			TwoWarnPlayed = true;
			Sound.FromScreen( "2_minutes_remain" );
		}

		if ( StateTimer <= 60 && !OneWarnPlayed && CurrentState == GameStates.Live )
		{
			OneWarnPlayed = true;
			Sound.FromScreen( "1_minute_remains" );
		}

		if ( StateTimer <= 11 && !TenWarnPlayed && CurrentState == GameStates.Live )
		{
			TenWarnPlayed = true;
			_ = TenCountDown();
		}
	}

	private async Task RoundPlay()
	{
		await Task.Delay( 5000 );
		Sound.FromScreen( "Play" );
	}

		private async Task TenCountDown()
	{
		Sound.FromScreen( "ten" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "nine" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "eight" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "seven" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "six" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "five" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "four" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "three" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "two" );
		await Task.Delay( 1000 );
		Sound.FromScreen( "one" );
		await Task.Delay( 2000 );
		Sound.FromScreen( "EndOfRound" );

	}
}
