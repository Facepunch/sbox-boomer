﻿using Boomer.Movement;
partial class DmViewModel : BaseViewModel
{
	bool ShouldBob = true;
	float TargetRoll = 0f;
	float TargetFOV = 0f;
	float TargetPos = 0f;
	float WalkBob = 0f;
	float MyRoll = 0f;

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );
		//camSetup.ViewModel.FieldOfView = 75f;
		if ( Local.Pawn is BoomerPlayer pl && pl.Controller is BoomerController ctrl )
		{
			ShouldBob = !ctrl.IsSliding && !ctrl.IsDashing;
			TargetRoll = ctrl.IsSliding ? -45f : 0f;
			
			TargetFOV = ctrl.IsSliding ? 80f : 75f;
			camSetup.ViewModel.FieldOfView = TargetFOV;

			TargetPos = ctrl.IsSliding ? -15f : 0f;
			Position += Vector3.Up * TargetPos;
		}
		

		AddCameraEffects( ref camSetup );
	}

	private void AddCameraEffects( ref CameraSetup camSetup )
	{
		if ( Local.Pawn.LifeState == LifeState.Dead ) return;
		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd ) return;

		// Shifts viewmodel to center of screen (temporary til we do it in model)
		Position += Rotation.Right * -17f;
		Position += Vector3.Up * -10f;

		// Slide Tilt
		MyRoll = MyRoll.LerpTo( TargetRoll, Time.Delta * 10f );
		Rotation *= Rotation.From( 0, 0, MyRoll );


		//
		// Bob up and down based on our walk movement
		//
		var speed = Owner.Velocity.Length.LerpInverse( 0, 400 );
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;

		if ( ShouldBob && Owner.GroundEntity != null )
		{
			WalkBob += Time.Delta * 25.0f * speed;
			
		}

		Position += up * MathF.Sin( WalkBob ) * speed * -1;
		Position += left * MathF.Sin( WalkBob * 0.5f ) * speed * -0.5f;

		var uitx = new Sandbox.UI.PanelTransform();
		uitx.AddTranslateY( MathF.Sin( WalkBob * 1.0f ) * speed * -4.0f );
		uitx.AddTranslateX( MathF.Sin( WalkBob * 0.5f ) * speed * -3.0f );

		HudRootPanel.Current.Style.Transform = uitx;
	}
}
