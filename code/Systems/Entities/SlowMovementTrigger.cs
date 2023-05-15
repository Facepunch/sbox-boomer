using Editor;
using System.ComponentModel.DataAnnotations;
using Facepunch.Boomer.Mechanics;

namespace Facepunch.Boomer;

[Library( "boomer_slowmovement" )]
[Display( Name = "Slow Movement", GroupName = "Shooter", Description = "A pad that launches players toward a target entity" ), Category( "Gameplay" ), Icon( "sports_gymnastics" )]
[AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[HammerEntity]
public partial class SlowMovementTrigger : BaseTrigger
{
	[Net, Property]
	[MinMax( 0.0f, 20.0f )]
	public float friction { get; set; } = 1f;

	public override void Spawn()
	{
		EnableTouchPersists = true;
		Tags.Add( "trigger" );
		base.Spawn();
	}

	public override void Touch( Entity other )
	{
		if ( !Game.IsServer ) return;
		if ( other is not Player pl ) return;

		if ( pl.Controller != null )
			pl.Controller.GetMechanic<WalkMechanic>().SurfaceFriction = friction;

		base.Touch( other );
	}
}
