namespace Boomer;

partial class AmmoPickup : BasePickup
{
	public virtual AmmoType AmmoType => AmmoType.None;
	public virtual int AmmoAmount => 17;
	public override Model WorldModel => Model.Load( "models/dm_battery.vmdl" );

	public override void OnPickup( BoomerPlayer player )
	{
		var ammoTaken = player.GiveAmmo( AmmoType, AmmoAmount );
		if ( ammoTaken == 0 )
			return;

		PlayPickupSound();
		PickupFeed.OnPickup( To.Single( player ), $"+{ammoTaken} {AmmoType}" );
		base.OnPickup( player );
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "dm.pickup_ammo", Position );
	}
}

[Library( "boomer_nails" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/nails/bm_nails.vmdl" )]
[Title( "Nail Clip" ), Category( "Ammo" )]
partial class AmmoNails : AmmoPickup
{
	public override AmmoType AmmoType => AmmoType.Nails;
	public override int AmmoAmount => 100;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/nails/bm_nails.vmdl" );
}

[Library( "boomer_grenades" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/grenades/bm_grenades.vmdl" )]
[Title( "Grenades" ), Category( "Ammo" )]
partial class AmmoGrenades : AmmoPickup
{
	public override AmmoType AmmoType => AmmoType.Grenade;
	public override int AmmoAmount => 5;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/grenades/bm_grenades.vmdl" );
}

[Library( "boomer_ammobuckshot" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/buckshot/bm_buckshot.vmdl" )]
[Title( "Buckshot" ), Category( "Ammo" )]
partial class AmmoBuckshot : AmmoPickup
{
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int AmmoAmount => 5;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/buckshot/bm_buckshot.vmdl" );
}

[Library( "boomer_rockets" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/rockets/bm_rockets.vmdl" )]
[Title( "Rockets" ), Category( "Ammo" )]
partial class AmmoRockets : AmmoPickup
{
	public override AmmoType AmmoType => AmmoType.Rockets;
	public override int AmmoAmount => 5;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/rockets/bm_rockets.vmdl" );
}

[Library( "boomer_rails" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/rails/bm_rails.vmdl" )]
[Title( "Rails" ), Category( "Ammo" )]
partial class AmmoRails: AmmoPickup
{
	public override AmmoType AmmoType => AmmoType.Rails;
	public override int AmmoAmount => 5;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/rails/bm_rails.vmdl" );
}

[Library( "boomer_lightning" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/lightning/bm_lightning.vmdl" )]
[Title( "Lightning" ), Category( "Ammo" )]
partial class AmmoLightning : AmmoPickup
{
	public override AmmoType AmmoType => AmmoType.Lightning;
	public override int AmmoAmount => 100;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/lightning/bm_lightning.vmdl" );
}
