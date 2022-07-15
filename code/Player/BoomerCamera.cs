
namespace Boomer;

internal class BoomerCamera : CameraMode
{

	public override void Update()
	{
		var target = Local.Pawn;

		if ( !target.IsValid() ) 
			return;

		Position = target.EyePosition;
		Rotation = target.EyeRotation;

		Viewer = target;
	}

}
