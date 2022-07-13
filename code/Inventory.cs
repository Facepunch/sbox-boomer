partial class DmInventory : BaseInventory
{


	public DmInventory( Player player ) : base( player )
	{

	}

	public override bool Add( Entity ent, bool makeActive = false )
	{
		var player = Owner as BoomerPlayer;
		var weapon = ent as DeathmatchWeapon;
		var notices = !player.SupressPickupNotices;

		if ( weapon == null )
			return false;

		if ( !base.Add( ent, makeActive ) )
			return false;

		if ( weapon != null && notices )
		{
			var display = DisplayInfo.For( ent );

			Sound.FromWorld( "dm.pickup_weapon", ent.Position );
			PickupFeed.OnPickupWeapon( To.Single( player ), display.Name );
		}

		ItemRespawn.Taken( ent );

		return true;
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x.IsValid() && x.GetType() == t );
	}
}
