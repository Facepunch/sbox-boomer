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
			PlayAnnouncerSound( To.Everyone, "roundbeginsin" );
		}
			if ( StateTimer <= 4 && !CountDownPlayed && CurrentState == GameStates.Warmup )
		{
			CountDownPlayed = true;
			PlayAnnouncerSound( To.Everyone, "countdown" );
			_ = RoundPlay();
		}

		if ( StateTimer <= 600 && !TenMinWarnPlayed && CurrentState == GameStates.Live )
		{
			TenMinWarnPlayed = true;
			PlayAnnouncerSound( To.Everyone, "10_minute_warning" );
		}

		if ( StateTimer <= 300 && !FiveWarnPlayed && CurrentState == GameStates.Live )
		{
			FiveWarnPlayed = true;
			PlayAnnouncerSound( To.Everyone, "5_minute_warning" );
		}

		if ( StateTimer <= 120 && !TwoWarnPlayed && CurrentState == GameStates.Live )
		{
			TwoWarnPlayed = true;
			PlayAnnouncerSound( To.Everyone, "2_minutes_remain" );
		}

		if ( StateTimer <= 60 && !OneWarnPlayed && CurrentState == GameStates.Live )
		{
			OneWarnPlayed = true;
			PlayAnnouncerSound( To.Everyone, "1_minute_remains" );
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
		PlayAnnouncerSound( To.Everyone, "Fight" );
	}

	[ClientRpc]
	private void PlayAnnouncerSound( string sound )
	{
		Log.Error( ClientSettings.Current.AnnouncerVolume );
		Sound.FromScreen( sound ).SetVolume( ClientSettings.Current.AnnouncerVolume );
	}

	private async Task TenCountDown()
	{
		PlayAnnouncerSound( To.Everyone, "ten" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "nine" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "eight" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "seven" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "six" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "five" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "four" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "three" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "two" );
		await Task.Delay( 1000 );
		PlayAnnouncerSound( To.Everyone, "one" );
		await Task.Delay( 2000 );
		PlayAnnouncerSound( To.Everyone, "EndOfRound" );
	}
}
