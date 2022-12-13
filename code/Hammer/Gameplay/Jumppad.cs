
using Sandbox;
using Editor;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Boomer;

[Library( "boomer_jumppad" )]
[Display( Name = "Jump Pad", GroupName = "Shooter", Description = "A pad that launches players toward a target entity" ), Category( "Gameplay" ), Icon( "sports_gymnastics" )]
[AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[Line( "targetname", "targetentity" )]
[HammerEntity]
public partial class Jumppad : BaseTrigger
{
	[Net, Property, FGDType( "target_destination" )] public string TargetEntity { get; set; } = "";
	[Net, Property] public float VerticalBoost { get; set; } = 200f;
	[Net, Property] public float Force { get; set; } = 1000f;

	/// <summary>
	/// Name of the sound to play.
	/// </summary>
	[Property( "JumppadSound" ), Title( "Jump Sound" ), FGDType( "sound" )]
	[Net] public string JumppadSound { get; set; }

	public override void Spawn()
	{
		if ( Force == 0f )
		{
			Force = 1000f;
		}
		Tags.Add( "trigger" );

		base.Spawn();
	}

	public override void Touch( Entity other )
	{
		if ( !Game.IsServer ) return;
		if ( other is not BoomerPlayer pl ) return;
		var target = FindByName( TargetEntity );

		if ( target.IsValid() )
		{
			if ( JumppadSound != null )
			{
				_ = Sound.FromWorld( JumppadSound,Position );
			}
			var direction = (target.Position - other.Position).Normal;
			pl.ApplyForce( new Vector3( 0f, 0f, VerticalBoost ) );
			pl.ApplyForce( direction * Force );
		}

		base.Touch( other );
	}
}
