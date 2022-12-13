using Boomer.Movement;

namespace Boomer;

[Library( "boomer_dashrecharge", Description = "Dash Recharge" )]
[EditorModel( "models/gameplay/dashrecharge/dashrecharge.vmdl", FixedBounds = true )]
[Display( Name = "Dash Recharge", GroupName = "Shooter", Description = "Coin Pickup." ), Category( "Gameplay" ), Icon( "currency_bitcoin" )]
[HammerEntity]
internal class DashRechargePickUp : ModelEntity
{
	public float RespawnTime { get; set; } = 5f;
	public bool Collected { get; set; }

	private Color DefaultColor;
	
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		
		DefaultColor = RenderColor;

		SetupPhysicsFromModel( PhysicsMotionType.Static );
		Tags.Add( "trigger" );
		EnableAllCollisions = true;
		EnableSolidCollisions = false;
		EnableTouch = true;

		SetModel( "models/gameplay/dashrecharge/dashrecharge.vmdl" );

	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !Game.IsServer ) return;

		if ( other is not BoomerPlayer pl ) return;
		
		if ( pl.Controller is BoomerController ctrl )
		{
			if ( ctrl.DashCount != 2 )
			{
				ctrl.DashCount++;
				Collect();
			}
		}
	}

	private void Collect()
	{
		Collected = true;
		EnableDrawing = false;
		EnableAllCollisions = false;

		Sound.FromEntity( "dashrecharge", this ).SetVolume( 1f );

		UnCollect( RespawnTime );
	}

	private async void UnCollect( float delay )
	{
		if ( !Collected )
			return;
		
		await GameTask.DelaySeconds( delay );

		PlaySound( "dashrecharge.respawn" );
		EnableDrawing = true;
		EnableAllCollisions = true;
		RenderColor = DefaultColor;
		Collected = false;
	}

}
