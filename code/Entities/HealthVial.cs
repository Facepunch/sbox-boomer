using Boomer.Movement;

namespace Boomer;

/// <summary>
/// Gives 5 health points.
/// </summary>
[Library( "boomer_healthvial" ), HammerEntity]
[EditorModel( "models/gameplay/healthvial/healthvial.vmdl" )]
[Title( "Health Vial" ), Category( "PickUps" )]
partial class HealthVial : HealthKit
{
	public override Model WorldModel => Model.Load( "models/gameplay/healthvial/healthvial.vmdl" );
	
	public override void Spawn()
	{
		base.Spawn();

		RespawnTime = 15;

		HealthGranted = 5f;
	}

}
