
using Boomer.Movement;
using Sandbox;
using Editor;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Boomer;

[Library( "boomer_slowmovement" )]
[Display( Name = "Slow Movement", GroupName = "Shooter", Description = "A pad that launches players toward a target entity" ), Category( "Gameplay" ), Icon( "sports_gymnastics" )]
[AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[HammerEntity]
public partial class SlowMovement : BaseTrigger
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
		if ( other is not BoomerPlayer pl ) return;

		if ( pl.Controller is BoomerController ctrl )

		ctrl.GetMechanic<Walk>().SurfaceFriction = friction;
		
		base.Touch( other );
	}
}
