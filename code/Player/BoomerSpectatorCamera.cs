namespace Boomer;

internal class BoomerSpectatorCamera : BoomerCamera
{
	public bool IsFree { get; set; } = false;

	int playerIndex = 0;
	public BoomerPlayer SelectPlayerIndex( int index )
	{
		var players = GetPlayers()
			.ToList();

		playerIndex = index;

		if ( playerIndex >= players.Count )
			playerIndex = 0;
		
		if ( playerIndex < 0 )
			playerIndex = players.Count - 1;

		var player = players[playerIndex];
		Target = player;

		return player;
	}
	
	public BoomerPlayer SpectateNextPlayer( bool asc = true )
	{
		return SelectPlayerIndex( asc ? playerIndex + 1 : playerIndex - 1 );
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( input.Pressed( InputButton.Jump ) )
			IsFree ^= true;

		if ( input.Pressed( InputButton.Left ) )
			SpectateNextPlayer( false ); 

		if ( input.Pressed( InputButton.Right ) )
			SpectateNextPlayer();

		base.BuildInput( input );
	}
}
