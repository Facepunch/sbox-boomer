namespace Boomer;

public class SpectatorPawn : AnimatedEntity
{
	// Do nothing for now
	public void Respawn()
	{
		Components.Add( new BoomerCamera() );
	}
}
