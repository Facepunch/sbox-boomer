using Sandbox;
using System.Collections.Generic;

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
	public Vector2 InitialForce { get; set; }

	[Category( "General" )]
	public float Gravity { get; set; } = 0f;

	[Category( "General" )]
	public float Radius { get; set; } = 8f;

	[Category( "General" )]
	public float Lifetime { get; set; } = 5f;

	[Category( "Explosion" )]
	public bool ExplodeOnDeath { get; set; }

	[Category( "Explosion" )]
	/// <summary>
	/// If we hit something that has any of these tags, we'll count it as a trigger to explode our projectile.
	/// </summary>
	public List<string> ExplodeHitTags { get; set; } = new()
	{
		"player"
	};

	[Category( "Explosion" )]
	public float ExplosionRadius { get; set; } = 512f;

	[Category( "Explosion" )]
	public float ExplosionDamage { get; set; } = 100f;

	[Category( "Explosion" ), ResourceType( "vpcf" )]
	public string ExplosionParticlePath { get; set; }

	[Category( "Explosion" ), ResourceType( "sound" )]
	public string ExplosionSoundPath { get; set; }

	[Category( "Explosion" ), Range( 0, 1 )]
	public float SelfDamageScale { get; set; } = 0f;

	[Category( "Explosion" )]
	public bool NoDeleteOnExplode { get; set; } = false;

	[Category( "Bounce" ), Range( 0, 1 )]
	public float Bounciness { get; set; }

	[Category( "Bounce" ), ResourceType( "sound" )]
	public string BounceSoundPath { get; set; }

	[Category( "Bounce" ), Range( 0, 1000 )]
	public float BounceSoundMinVelocity { get; set; } = 300f;
}
