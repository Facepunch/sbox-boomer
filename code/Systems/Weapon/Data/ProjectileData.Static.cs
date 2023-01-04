using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Boomer.WeaponSystem;

public partial class ProjectileData
{
	public Model CachedModel;

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
		return All.FirstOrDefault( x => x.ResourcePath.ToLower() == name.ToLower() );
	}

	public static Projectile Create( ProjectileData data, Player owner = null )
	{
		Game.AssertServer( "Can only create projectiles serverside." );

		if ( data == null )
		{
			Log.Warning( $"Couldn't create projectile. (data: {data}, owner: {owner})" );

			return null;
		}

		var projectile = new Projectile
		{
			Owner = owner
		};

		projectile.Initialize( data );

		if ( data.InitialForce != Vector2.Zero )
		{
			// Do initial force stuff
		}

		return projectile;
	}

	public static Projectile Create( string name, Player owner = null )
	{
		Log.Info( $"Trying to create projectile with name: {name}" );
		return Create( Find( name ), owner );
	}

	protected override void PostLoad()
	{
		CachedModel = Model.Load( ModelPath );

		Log.Info( $"Registering projectile data ({ResourcePath}, {ResourceName})" );

		if ( !All.Contains( this ) )
			All.Add( this );

		// Precache
		if ( !string.IsNullOrEmpty( ActiveSoundPath ) ) Precache.Add( ActiveSoundPath );
		if ( !string.IsNullOrEmpty( ParticlePath ) ) Precache.Add( ParticlePath );
		if ( !string.IsNullOrEmpty( ExplosionParticlePath ) ) Precache.Add( ExplosionParticlePath );
		if ( !string.IsNullOrEmpty( ExplosionSoundPath ) ) Precache.Add( ExplosionSoundPath );
		if ( !string.IsNullOrEmpty( BounceSoundPath ) ) Precache.Add( BounceSoundPath );
	}
}
