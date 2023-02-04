using Sandbox;

namespace Facepunch.Boomer.WeaponSystem;


[Prefab]
public partial class Ammo : WeaponComponent, ISingletonComponent
{
	[Net] public int AmmoCount { get; set; }

	[Prefab] public int DefaultAmmo { get; set; } = 8;
	[Prefab] public int MaximumAmmo { get; set; } = 8;
	[Prefab] public bool AllowChamber { get; set; } = true;

	public bool IsFull
	{
		get => AmmoCount >= ( AllowChamber ? MaximumAmmo + 1 : MaximumAmmo ); 
	}

	public override void OnGameEvent( string eventName )
	{
		if ( eventName == "shoot.fire" || eventName == "secondaryshoot.fire" )
		{
			TakeAmmo();
		}
	}

	// If we want to refill the ammo, here's a nice utility method for it.
	public void Fill()
	{
		if ( AmmoCount == MaximumAmmo && AllowChamber )
		{
			++AmmoCount;
			return;
		}

		AmmoCount = DefaultAmmo;
	}

	public bool HasEnoughAmmo( int amount = 1 )
	{
		return AmmoCount >= amount;
	}

	public bool TakeAmmo( int amount = 1 )
	{
		if ( AmmoCount >= amount )
		{
			AmmoCount -= amount;
			return true;
		}

		return false;
	}
}
