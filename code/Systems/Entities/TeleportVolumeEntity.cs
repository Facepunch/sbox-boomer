using Editor;
using Sandbox;
using System.Linq;

namespace Facepunch.Boomer;

/// <summary>
/// A simple trigger volume that teleports entities that touch it.
/// </summary>
[Library( "boomer_teleport" )]
[HammerEntity, Solid]
[Title( "Teleport Volume" ), Category( "Triggers" ), Icon( "auto_fix_normal" )]
public partial class TeleportVolumeEntity : BaseTrigger
{

	/// <summary>
	/// Name of the sound to play.
	/// </summary>
	[Property( "entersoundName" ), FGDType( "sound" )]
	[Net] public string EnterSoundName { get; set; }

	/// <summary>
	/// Name of the sound to play.
	/// </summary>
	[Property( "exitsoundName" ), FGDType( "sound" )]
	[Net] public string ExitSoundName { get; set; }

	/// <summary>
	/// The entity specifying a location to which entities should be teleported to.
	/// </summary>
	[Property( "target" ), Title( "Remote Destination" )]
	public EntityTarget TargetEntity { get; set; }

	/// <summary>
	/// If set, teleports the entity with an offset depending on where the entity was in the trigger teleport. Think world portals. Place the target entity accordingly.
	/// </summary>
	[Property( "teleport_relative" ), Title( "Teleport Relatively" )]
	public bool TeleportRelative { get; set; }

	/// <summary>
	/// If set, the teleported entity will not have it's velocity reset to 0.
	/// </summary>
	[Property( "keep_velocity" ), Title( "Keep Velocity" )]
	public bool KeepVelocity { get; set; }

	/// <summary>
	/// Fired when the trigger teleports an entity
	/// </summary>
	protected Output OnTriggered { get; set; }

	public override void OnTouchStart( Entity other )
	{
		if ( !Enabled ) return;

		var Targetent = TargetEntity.GetTargets( null ).FirstOrDefault();

		if ( other is not Player pl ) return;

		if ( Targetent != null )
		{
			Sound.FromWorld( EnterSoundName, Position );

			Vector3 offset = Vector3.Zero;
			if ( TeleportRelative )
			{
				offset = other.Position - Position;
			}

			if ( !KeepVelocity ) pl.Velocity = Vector3.Zero;

			// Fire the output, before actual teleportation so entity IO can do things like disable a trigger_teleport we are teleporting this entity into
			OnTriggered.Fire( other );

			pl.SetViewAngles( Targetent.Rotation.Angles() );

			pl.Velocity = Targetent.Rotation.Forward * 850;

			Sound.FromEntity( ExitSoundName, Targetent );

			other.Transform = Targetent.Transform;
			other.Position += offset;
		}
	}
}
