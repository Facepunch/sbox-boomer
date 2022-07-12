using Boomer.Movement;
partial class DmViewModel : BaseViewModel
{
	float walkBob = 0;

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		Position += Rotation.Right * -17f;
		Position += Vector3.Up * -10f;

		//camSetup.ViewModelFieldOfView = camSetup.FieldOfView + (FieldOfView - 80);

		AddCameraEffects( ref camSetup );
	}

	protected Rotation NewRotation;

	protected Vector3 Acceleration;

	float MyRoll = 0f;

	Rotation MyRotation;

	private void AddCameraEffects( ref CameraSetup camSetup )
	{
		//Rotation = Local.Pawn.EyeRotation;

		if ( Local.Pawn.LifeState == LifeState.Dead )
			return;

		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd )
			return;
		var player = Local.Pawn as DeathmatchPlayer;
		var ctrl = player.Controller as BoomerController;

		if ( ctrl.IsSliding || ctrl.IsDashing )
		{
			walkBob = 0f;
		}

		// Slide Tilt
		var targetRoll = ctrl.IsSliding ? -30f : 0f;
		MyRoll = MyRoll.LerpTo( targetRoll, Time.Delta * 10f );
		Rotation *= Rotation.From( 0, 0, MyRoll );

		//
		// Bob up and down based on our walk movement
		//
		var speed = Owner.Velocity.Length.LerpInverse( 0, 400 );
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;

		if ( Owner.GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;

		}

		Position += up * MathF.Sin( walkBob ) * speed * -1;
		Position += left * MathF.Sin( walkBob * 0.5f ) * speed * -0.5f;

		var uitx = new Sandbox.UI.PanelTransform();
		uitx.AddTranslateY( MathF.Sin( walkBob * 1.0f ) * speed * -4.0f );
		uitx.AddTranslateX( MathF.Sin( walkBob * 0.5f ) * speed * -3.0f );

		HudRootPanel.Current.Style.Transform = uitx;

	}
}
