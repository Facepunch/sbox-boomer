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

	public void ResetInterpolation()
	{
		// Force eye rotation to avoid lerping when switching targets
		if ( Target.IsValid() )
			Rotation = Target.EyeRotation;
	}

	protected void ToggleFree()
	{
		IsFree ^= true;

		if ( IsFree )
		{
			if ( Target.IsValid() )
				Position = Target.EyePosition;

			vm?.Delete();
			cachedWeapon = null;
			Viewer = null;
		}
		else
		{
			ResetInterpolation();
			Viewer = Target;
		}
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( input.Pressed( InputButton.Jump ) )
			ToggleFree();

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

	protected BaseViewModel vm;
	protected DeathmatchWeapon cachedWeapon;

	protected void UpdateViewModel( DeathmatchWeapon weapon )
	{
		if ( IsSpectator )
		{
			vm?.Delete();
			vm = null;

			if ( weapon.IsValid() )
			{
				weapon?.CreateViewModel();
				vm = weapon.ViewModelEntity;
			}
		}
		else
		{
			vm?.Delete();
		}
	}

	[Event( "boomer.spectator.changedtarget" )]
	protected void OnTargetChanged( BoomerPlayer oldTarget, BoomerPlayer newTarget )
	{
		var curWeapon = newTarget?.ActiveChild as DeathmatchWeapon;
		cachedWeapon = curWeapon;

		ResetInterpolation();
		UpdateViewModel( curWeapon );
	}

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
			var curWeapon = Target?.ActiveChild as DeathmatchWeapon;
			if ( curWeapon.IsValid() && curWeapon != cachedWeapon )
			{
				cachedWeapon = curWeapon;
				UpdateViewModel( curWeapon );
			}

			base.Update();
		}
	}
}
