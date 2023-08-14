using Facepunch.Boomer.WeaponSystem;

namespace Facepunch.Boomer;

public partial class Player
{
	public ProjectileSimulator ProjectileSimulator { get; set; }

	protected override void OnDestroy()
	{
		// Clear projectiles on destroy since they're not being simulated by its owner anymore
		ProjectileSimulator.Clear();
	}
}
