using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Boomer.WeaponSystem;

public partial class ProjectileData
{
	public static Model CachedModel;

	/// <summary>
	/// A list of all projectile data.
	/// </summary>
	public static List<ProjectileData> All = new();


	/// <summary>
	/// Find projectile data from its name.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static ProjectileData Find( string name )
	{
		return All.FirstOrDefault( x => x.ResourceName.ToLower() == name.ToLower() );
	}

	public static Projectile Create( ProjectileData data, Player owner = null )
	{
		if ( data == null ) return null;

		var projectile = new Projectile
		{
			Data = data,
			Owner = owner
		};

		if ( data.InitialForce != Vector3.Zero )
		{
			// Do initial force stuff
		}

		return projectile;
	}

	public static Projectile Create( string name )
	{
		return Create( Find( name ) );
	}

	protected override void PostLoad()
	{
		CachedModel = Model.Load( ModelPath );

		Log.Info( $"Registering projectile data ({ResourcePath}, {ResourceName})" );

		if ( !All.Contains( this ) )
			All.Add( this );
	}
}
