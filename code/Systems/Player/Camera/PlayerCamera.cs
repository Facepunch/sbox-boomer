using Sandbox;
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
	private float cameraLean = 0;
	private float fovOffset = 0;
	protected virtual void AddCameraEffects( Player player )
	{
		if ( player.LifeState != LifeState.Alive ) return;

		var speed = player.Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = player.Velocity.Normal.Dot( Camera.Rotation.Forward );

		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;

		if ( player.Controller.GroundEntity.IsValid() )
		{
			walkBob += Time.Delta * 12.0f * speed;
		}

		Camera.Position += up * MathF.Cos( walkBob ) * speed * 2;
		Camera.Position += left * MathF.Cos( walkBob * 0.6f ) * speed * 1;

		// Camera lean
		cameraLean = cameraLean.LerpTo( player.Velocity.Dot( Camera.Rotation.Right ) * 0.01f, Time.Delta * 15.0f );

		var appliedLean = cameraLean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.3f;
		Camera.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fovOffset = fovOffset.LerpTo( speed * 5 * MathF.Abs( forwardspeed ), Time.Delta * 4.0f );

		Camera.FieldOfView += fovOffset;
	}
}
