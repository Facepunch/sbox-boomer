namespace Boomer;

public partial class BoomerPlayer
{
	[Net, Local]
	public IList<int> Ammo { get; set; }

	public void ClearAmmo()
	{
		Ammo.Clear();
	}

	public int AmmoCount( AmmoType type )
	{
		var iType = (int)type;
		if ( Ammo == null ) return 0;
		if ( Ammo.Count <= iType ) return 0;

		return Ammo[(int)type];
	}

	public bool SetAmmo( AmmoType type, int amount )
	{
		var iType = (int)type;
		if ( !Game.IsServer ) return false;
		if ( Ammo == null ) return false;

		while ( Ammo.Count <= iType )
		{
			Ammo.Add( 0 );
		}

		Ammo[(int)type] = amount;
		return true;
	}

	public int GiveAmmo( AmmoType type, int amount )
	{
		if ( !Game.IsServer ) return 0;
		if ( Ammo == null ) return 0;
		if ( type == AmmoType.None ) return 0;

		var total = AmmoCount( type ) + amount;
		var max = MaxAmmo( type );

		if ( total > max ) total = max;
		var taken = total - AmmoCount( type );

		SetAmmo( type, total );

		if ( Game.IsServer )
			OnAmmoChanged( type, true );

		return taken;
	}

	public bool Give( string weaponName )
	{
		// do we already have one?
		var existing = Children.Where( x => x.ClassName == weaponName ).FirstOrDefault();
		if ( existing != null ) return false;

		var weapon = Entity.CreateByName<DeathmatchWeapon>( weaponName );
		if ( Inventory.Add( weapon ) )
			return true;

		weapon?.Delete();
		return false;
	}

	public int TakeAmmo( AmmoType type, int amount )
	{
		if ( Ammo == null ) return 0;

		var available = AmmoCount( type );
		amount = Math.Min( available, amount );

		SetAmmo( type, available - amount );

		if ( Game.IsServer )
			OnAmmoChanged( type, false );

		return amount;
	}

	[ClientRpc]
	public void OnAmmoChanged( AmmoType type, bool positive )
	{
		Game.AssertClient();

		if ( ActiveChild is DeathmatchWeapon weapon )
		{
			if ( weapon.AmmoType != type ) return;

			if ( positive ) SinceAmmoWentUp = 0;
			if ( !positive ) SinceAmmoWentDown = 0;
		}
	}

	public TimeSince SinceAmmoWentUp;
	public TimeSince SinceAmmoWentDown;

	public int MaxAmmo( AmmoType ammo )
	{
		switch ( ammo )
		{
			case AmmoType.Pistol: return 250;
			case AmmoType.Buckshot: return 25;
			case AmmoType.Nails: return 200;
			case AmmoType.Rockets: return 20;
			case AmmoType.Grenade: return 7;
			case AmmoType.Rails: return 15;
			case AmmoType.Lightning: return 300;

		}

		return 0;
	}
}

public enum AmmoType
{
	None,
	Pistol,
	Buckshot,
	Nails,
	Rockets,
	Grenade,
	Rails,
	Lightning
}
