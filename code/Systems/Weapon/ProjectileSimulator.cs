using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Boomer.WeaponSystem;

public partial class ProjectileSimulator
{
	public List<Projectile> Projectiles { get; set; }

	public ProjectileSimulator()
	{
		Projectiles = new();
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
		using ( Entity.LagCompensation() )
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
					projectile.Simulate( cl );
			}
		}
	}
}
