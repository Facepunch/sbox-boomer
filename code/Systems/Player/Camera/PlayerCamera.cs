using Sandbox;
using Sandbox.Utility;
using System;

namespace Facepunch.Boomer;

public partial class PlayerCamera
{
	public virtual void BuildInput( Player player )
	{
		//
	}

	public virtual void Update( Player player )
	{
		Camera.Position = player.EyePosition;
		Camera.Rotation = player.EyeRotation;
		Camera.FieldOfView = Game.Preferences.FieldOfView;
		Camera.FirstPersonViewer = player;
		Camera.ZNear = 0.5f;

		UpdatePostProcess();
		AddCameraEffects( player );
	}

	private float walkBob = 0;
	private float fovOffset = 0;
	protected virtual void AddCameraEffects( Player player )
	{
		if ( player.LifeState != LifeState.Alive ) return;
		if ( player.Controller == null ) return;

		var speed = player.Velocity.Length.LerpInverse( 0, 350 );
		var forwardspeed = player.Velocity.Normal.Dot( Camera.Rotation.Forward );

		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;

		if ( player.Controller.GroundEntity.IsValid() )
		{
			walkBob += Time.Delta * 18.0f * speed;
		}


		Camera.Position += up * Easing.QuadraticOut( MathF.Sin( walkBob ) ) * speed;
		Camera.Position += left * MathF.Cos( walkBob ) * speed * -0.5f;

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fovOffset = fovOffset.LerpTo( speed * 5 * MathF.Abs( forwardspeed ), Time.Delta * 4.0f );

		Camera.FieldOfView += fovOffset;
	}
}
