using Facepunch.Boomer.UI;
using Sandbox;
using Sandbox.Utility;
using System;

namespace Facepunch.Boomer;

public partial class PlayerDeathCamera : PlayerCamera
{
	public Entity FocusEntity { get; set; }
	Vector3 FocusPoint => FocusEntity?.AimRay.Position ?? Camera.Position;
	Rotation FocusRotation => Rotation.LookAt( FocusEntity?.AimRay.Forward ?? Vector3.Forward );

	public virtual Vector3 GetViewOffset()
	{
		return FocusRotation.Forward * 100f + Vector3.Down * 10f;
	}

	public override void Update( Player player )
	{
		var delta = Time.Delta * 20f;
		Camera.Position = Camera.Position.LerpTo( FocusPoint + GetViewOffset(), delta );
		Camera.Rotation = Rotation.Lerp( Camera.Rotation, Rotation.LookAt( -FocusRotation.Forward, Vector3.Up ), delta );
		Camera.FirstPersonViewer = null;
		Camera.FieldOfView = Camera.FieldOfView.LerpTo( 40, Time.Delta * 3f );
		Camera.ZNear = 0.5f;

		UpdatePostProcess();
	}
}
