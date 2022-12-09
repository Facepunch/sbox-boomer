namespace Boomer;

public partial class SpectatorPawn : AnimatedEntity
{
	[Net, Predicted] private BoomerSpectatorCamera Cam { get; set; }

	// Do nothing for now
	public void Respawn()
	{
		Cam = new BoomerSpectatorCamera();
	}

	public override void BuildInput()
	{
		Cam?.BuildInput();

		base.BuildInput();
	}

	public override void FrameSimulate( Client cl )
	{
		Cam?.Update();

		base.FrameSimulate( cl );
	}
}
