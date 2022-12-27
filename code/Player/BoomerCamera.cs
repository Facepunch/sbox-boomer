namespace Boomer;

internal class BoomerCamera : BaseCamera
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

	public override void BuildInput()
	{
		Input.AnalogLook *= FOVCurrent / Game.Preferences.FieldOfView;
	}

	private float FOVCurrent;
	private float FOVCurrentVM = 45;

	public override void Update()
	{
		if ( Game.LocalPawn is BoomerPlayer player )
			Target = player;

		if ( !Target.IsValid() )
			Target = GetPlayers().FirstOrDefault();

		var target = Target;
		if ( !target.IsValid() )
			return;

		Camera.Position = target.EyePosition;

		if ( IsLocal )
			Camera.Rotation = target.EyeRotation;
		else
			Camera.Rotation = Rotation.Slerp( Camera.Rotation, target.EyeRotation, Time.Delta * 20f );

		Camera.FirstPersonViewer = target;

		var wpn = Target.ActiveChild as DeathmatchWeapon;
		var zoomed = false;
		var zoomFov = 90f;
		var zoomViewModelFov = Game.Preferences.FieldOfView;
		if ( wpn.IsValid() )
		{
			// Set values if we have them
			zoomed = wpn.Zoomed;
			zoomFov = wpn.ZoomedFov;
			zoomViewModelFov = wpn.ZoomedViewmodelFov;
		}

		var targetVMFoV = zoomed ? zoomViewModelFov : 90f;
		var targetFoV = zoomed ? zoomFov : Game.Preferences.FieldOfView;
		FOVCurrent = FOVCurrent.LerpTo( targetFoV, 15f * Time.Delta );
		FOVCurrentVM = FOVCurrentVM.LerpTo( targetVMFoV, 15f * Time.Delta );

		Camera.FieldOfView = FOVCurrent;
		Camera.Main.SetViewModelCamera( FOVCurrentVM, 0.1f, 1000f );
	}
}
