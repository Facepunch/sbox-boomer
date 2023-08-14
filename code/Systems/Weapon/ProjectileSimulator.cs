using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Boomer.WeaponSystem;

public partial class ProjectileSimulator : IValid
{
	public List<Projectile> Projectiles { get; private set; }
	public Entity Owner { get; private set; }
	public bool IsValid => Owner.IsValid();

	public ProjectileSimulator( Entity owner )
	{
		Projectiles = new();
		Owner = owner;
	}

	public void Add( Projectile projectile )
	{
		Projectiles.Add( projectile );
	}

	public void Remove( Projectile projectile )
	{
		Projectiles.Remove( projectile );
	}

	public void Clear()
	{
		foreach ( var projectile in Projectiles )
		{
			projectile.Delete();
		}

		Projectiles.Clear();
	}

	public void Simulate( IClient cl )
	{
		for ( int i = Projectiles.Count - 1; i >= 0; i-- )
		{
			var projectile = Projectiles[i];

			if ( !projectile.IsValid() )
			{
				Projectiles.RemoveAt( i );
				continue;
			}

			if ( Prediction.FirstTime )
				projectile.Simulate();
		}
	}
}

public static class ProjectileSimulatorExtensions
{
	public static bool IsValid( this ProjectileSimulator simulator )
	{
		return simulator != null && (simulator.Owner?.IsValid() ?? false);
	}
}
