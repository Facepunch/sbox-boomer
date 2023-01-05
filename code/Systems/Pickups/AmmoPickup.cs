using Editor;
using Facepunch.Boomer.WeaponSystem;
using Sandbox;

namespace Facepunch.Boomer;

partial class AmmoPickup : BasePickup
{
	public virtual string ResourceIdent => "";
	public virtual string AmmoName => "Ammo";
	public virtual int AmmoAmount => 17;
	public override Model WorldModel => Model.Load( "models/dm_battery.vmdl" );

	public override void OnPickup( Player player )
	{
		var wpn = player.Inventory.FindWeapon( ResourceIdent );
		if ( !wpn.IsValid() ) return;

		var ammoComponent = wpn.GetComponent<Ammo>();
		if ( ammoComponent.IsFull ) return;

		ammoComponent.Fill();
		PlayPickupSound();

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
	public override string ResourceIdent => "nailgun";
	public override string AmmoName => "#Ammo.Nails";
	public override int AmmoAmount => 100;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/nails/bm_nails.vmdl" );
}

[Library( "boomer_grenades" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/grenades/bm_grenades.vmdl" )]
[Title( "Grenades" ), Category( "Ammo" )]
partial class AmmoGrenades : AmmoPickup
{
	public override string ResourceIdent => "gl";
	public override string AmmoName => "#Ammo.Grenade";
	public override int AmmoAmount => 5;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/grenades/bm_grenades.vmdl" );
}

[Library( "boomer_rockets" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/rockets/bm_rockets.vmdl" )]
[Title( "Rockets" ), Category( "Ammo" )]
partial class AmmoRockets : AmmoPickup
{
	public override string ResourceIdent => "rl";
	public override string AmmoName => "#Ammo.Rockets";
	public override int AmmoAmount => 5;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/rockets/bm_rockets.vmdl" );
}
