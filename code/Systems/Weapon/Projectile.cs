using Sandbox;

namespace Facepunch.Boomer.WeaponSystem;

public partial class Projectile : ModelEntity
{
	/// <summary>
	/// Projectile data asset
	/// </summary>
	[Net] public ProjectileData Data { get; set; }
}
