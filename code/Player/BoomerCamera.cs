namespace Boomer;

internal class BoomerCamera : CameraMode
{
	private static BoomerPlayer target;
	public static BoomerPlayer Target
	{
		get => target;
		set
		{
			if ( target == value ) return;

			var oldTarget = target;
			target = value;

			Event.Run( "boomer.spectator.changedtarget", oldTarget, target );
		}
	}

	public static bool IsSpectator => Target.IsValid() && !Target.IsLocalPawn;
	public static bool IsLocal => !IsSpectator;

	public virtual IEnumerable<BoomerPlayer> GetPlayers()
	{
		return Entity.All.OfType<BoomerPlayer>();
	}

	public override void Update()
	{
		if ( Local.Pawn is BoomerPlayer player )
			Target = player;

		if ( !Target.IsValid() )
			Target = GetPlayers().FirstOrDefault();

		var target = Target;
		if ( !target.IsValid() )
			return;

		Position = target.EyePosition;

		if ( IsLocal )
			Rotation = target.EyeRotation;
		else
			Rotation = Rotation.Slerp( Rotation, target.EyeRotation, Time.Delta * 20f );

		Viewer = target;
	}
}
