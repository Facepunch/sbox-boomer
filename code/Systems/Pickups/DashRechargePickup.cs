using Editor;
using Facepunch.Boomer.Mechanics;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Boomer;

[Library( "boomer_dashrecharge", Description = "Dash Recharge" )]
[EditorModel( "models/gameplay/dashrecharge/dashrecharge.vmdl", FixedBounds = true )]
[Display( Name = "Dash Recharge", GroupName = "Shooter", Description = "Coin Pickup." ), Category( "Gameplay" ), Icon( "currency_bitcoin" )]
[HammerEntity]
internal class DashRechargePickUp : BasePickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/dashrecharge/dashrecharge.vmdl" );

	public override void Spawn()
	{
		OnRespawnAction += () =>
		{
			PlaySound( "dashrecharge.respawn" );
		};
	}

	public override void OnPickup( Player player )
	{
		Sound.FromEntity( "dashrecharge", this );

		var dash = player.Controller?.GetMechanic<DashMechanic>();
		if ( dash != null )
		{
			dash.DashCount++;
		}

		base.OnPickup( player );
	}

	public override bool CanPickup( Player player )
	{
		if ( player.Controller?.GetMechanic<DashMechanic>()?.DashCount >= 2 ) return false;
		return base.CanPickup( player );
	}
}
