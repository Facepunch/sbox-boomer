namespace Boomer;

internal class BoomerSpectatorCamera : BoomerCamera
{
	public bool IsFree { get; set; } = false;

	protected virtual float BaseMoveSpeed => 800f;

	// TODO - Input modifiers
	protected float MoveMultiplier = 1f;

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

		// Force freecam off
		IsFree = false;

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

		if ( input.Pressed( InputButton.Menu ) )
			SpectateNextPlayer( false ); 

		if ( input.Pressed( InputButton.Use ) )
			SpectateNextPlayer();

		if ( IsFree )
		{
			MoveInput = input.AnalogMove;
			LookAngles += input.AnalogLook;
			LookAngles.roll = 0;

			input.Clear();
			input.StopProcessing = true;
		}
		else
		{
			base.BuildInput( input );
		}
	}

	Angles LookAngles;
	Vector3 MoveInput;

	public override void Update()
	{
		if ( IsFree )
		{
			var mv = MoveInput.Normal * BaseMoveSpeed * RealTime.Delta * Rotation * MoveMultiplier;
			Position += mv;
			Rotation = Rotation.From( LookAngles );
		}
		else
		{
			base.Update();
		}
	}
}
