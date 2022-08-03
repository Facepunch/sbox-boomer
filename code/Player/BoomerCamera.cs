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

			OnTargetChanged( oldTarget, target );
		}
	}

	public static bool IsSpectator => Target.IsValid() && !Target.IsLocalPawn;
	public static bool IsLocal => !IsSpectator;

	static BaseViewModel vm;
	static DeathmatchWeapon cachedWeapon;

	static void UpdateViewModel( DeathmatchWeapon weapon )
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

	static void OnTargetChanged( BoomerPlayer oldTarget, BoomerPlayer newTarget )
	{
		Event.Run( "boomer.spectator.changedtarget", oldTarget, newTarget );

		var curWeapon = newTarget?.ActiveChild as DeathmatchWeapon;
		cachedWeapon = curWeapon;

		UpdateViewModel( curWeapon );
	}
	
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

		var curWeapon = target?.ActiveChild as DeathmatchWeapon;
		if ( curWeapon.IsValid() && curWeapon != cachedWeapon )
		{
			cachedWeapon = curWeapon;
			UpdateViewModel( curWeapon );
		}

		Position = target.EyePosition;

		if ( IsLocal )
			Rotation = target.EyeRotation;
		else
			Rotation = Rotation.Slerp( Rotation, target.EyeRotation, Time.Delta * 20f );

		Viewer = target;
	}
}
