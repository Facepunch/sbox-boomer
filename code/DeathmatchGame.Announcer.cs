namespace Boomer;

partial class DeathmatchGame : Game
{
	[Net]
	private bool CountDownPlayed { get; set; } = false;
	[Net]
	private bool RoundBeginsPlayed { get; set; } = false;

	[Net]
	private bool FiveWarnPlayed { get; set; } = false;
	
	[Net]
	private bool TenMinWarnPlayed { get; set; } = false;

	[Net]
	private bool TwoWarnPlayed { get; set; } = false;

	[Net]
	private bool OneWarnPlayed { get; set; } = false;

	[Net]
	private bool TenWarnPlayed { get; set; } = false;

	[Event.Tick.Server]
	public void Tick()
	{
		if ( StateTimer <= 6 && !RoundBeginsPlayed && CurrentState == GameStates.Warmup )
		{
			RoundBeginsPlayed = true;
			Sound.FromScreen( "roundbeginsin" );
		}
			if ( StateTimer <= 4 && !CountDownPlayed && CurrentState == GameStates.Warmup )
		{
			CountDownPlayed = true;
			Sound.FromScreen( "countdown" );
			_ = RoundPlay();
		}

		if ( StateTimer <= 600 && !TenMinWarnPlayed && CurrentState == GameStates.Live )
		{
			TenMinWarnPlayed = true;
			Sound.FromScreen( "10_minute_warning" );
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
		Sound.FromScreen( "Fight" );
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
