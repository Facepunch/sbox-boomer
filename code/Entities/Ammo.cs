namespace Boomer;

partial class BaseAmmo : ModelEntity, IRespawnableEntity
{
	//public static Model WorldModel = Model.Load( "models/dm_battery.vmdl" );

	
	public virtual AmmoType AmmoType => AmmoType.None;
	public virtual int AmmoAmount => 17;
	public virtual Model WorldModel => Model.Load( "models/dm_battery.vmdl" );

	[Property]
	public int RespawnTime { get; set; } = 30;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		PhysicsEnabled = true;
		UsePhysicsCollision = true;

		Tags.Add( "trigger" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( IsServer )
		{
			if ( other is not BoomerPlayer player )
				return;

			if ( other.LifeState != LifeState.Alive )
				return;

			var ammoTaken = player.GiveAmmo( AmmoType, AmmoAmount );

			if ( ammoTaken == 0 )
				return;

			PlayPickupSound();

			PickupFeed.OnPickup( To.Single( player ), $"+{ammoTaken} {AmmoType}" );
			ItemRespawn.Taken( this, RespawnTime );

			Delete();
		}
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
partial class AmmoNails : BaseAmmo
{
	public override AmmoType AmmoType => AmmoType.Nails;
	public override int AmmoAmount => 100;
	public override Model WorldModel => Model.Load( "models/gameplay/ammo/nails/bm_nails.vmdl" );

}

[Library( "boomer_grenades" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/grenades/bm_grenades.vmdl" )]
[Title( "Grenades" ), Category( "Ammo" )]
partial class AmmoGrenades : BaseAmmo
{
	public override AmmoType AmmoType => AmmoType.Grenade;
	public override int AmmoAmount => 5;

	public override Model WorldModel => Model.Load( "models/gameplay/ammo/grenades/bm_grenades.vmdl" );
}



[Library( "boomer_ammobuckshot" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/buckshot/bm_buckshot.vmdl" )]
[Title( "Buckshot" ), Category( "Ammo" )]
partial class AmmoBuckshot : BaseAmmo
{
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int AmmoAmount => 5;

	public override Model WorldModel => Model.Load( "models/gameplay/ammo/buckshot/bm_buckshot.vmdl" );
}

[Library( "boomer_rockets" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/rockets/bm_rockets.vmdl" )]
[Title( "Rockets" ), Category( "Ammo" )]
partial class AmmoRockets : BaseAmmo
{
	public override AmmoType AmmoType => AmmoType.Rockets;
	public override int AmmoAmount => 5;

	public override Model WorldModel => Model.Load( "models/gameplay/ammo/rockets/bm_rockets.vmdl" );
}

[Library( "boomer_rails" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/rails/bm_rails.vmdl" )]
[Title( "Rails" ), Category( "Ammo" )]
partial class AmmoRails: BaseAmmo
{
	public override AmmoType AmmoType => AmmoType.Rails;
	public override int AmmoAmount => 5;

	public override Model WorldModel => Model.Load( "models/gameplay/ammo/rails/bm_rails.vmdl" );
}

[Library( "boomer_lightning" ), HammerEntity]
[EditorModel( "models/gameplay/ammo/lightning/bm_lightning.vmdl" )]
[Title( "Lightning" ), Category( "Ammo" )]
partial class AmmoLightning : BaseAmmo
{
	public override AmmoType AmmoType => AmmoType.Lightning;
	public override int AmmoAmount => 100;

	public override Model WorldModel => Model.Load( "models/gameplay/ammo/lightning/bm_lightning.vmdl" );
}
