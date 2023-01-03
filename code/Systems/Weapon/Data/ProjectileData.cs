using Sandbox;

namespace Facepunch.Boomer.WeaponSystem;

[GameResource( "Projectile", "ple", "Projectile data for a weapon projectile", Icon = "sports_volleyball" )]
public partial class ProjectileData : GameResource
{
	[Category( "General" ), ResourceType( "vmdl" )]
	public string ModelPath { get; set; }

	[Category( "General" ), ResourceType( "vpcf" )]
	public string ParticlePath { get; set; }

	[Category( "General" ), ResourceType( "sound" )]
	public string ActiveSoundPath { get; set; }

	[Category( "General" )]
	public Vector3 InitialForce { get; set; }

	[Category( "General" )]
	public float Lifetime { get; set; } = 5f;

	[Category( "Explosion" )]
	public bool ExplodeOnDeath { get; set; }

	[Category( "Explosion" )]
	public float ExplosionRadius { get; set; } = 512f;

	[Category( "Explosion" )]
	public float ExplosionDamage { get; set; } = 100f;

	[Category( "Explosion" ), ResourceType( "vpcf" )]
	public string ExplosionParticlePath { get; set; }

	[Category( "Explosion" ), ResourceType( "sound" )]
	public string ExplosionSoundPath { get; set; }

	[Category( "Bounce" ), Range( 0, 1 )]
	public float Bounciness { get; set; }

	[Category( "Bounce" ), ResourceType( "sound" )]
	public string BounceSoundPath { get; set; }
}
