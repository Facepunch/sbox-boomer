using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Boomer;

public record PlayerAward( string Name, string Description, string Icon );

public partial class Player
{
	public List<PlayerAward> GrantedAwards { get; set; } = new();

	[ClientRpc]
	public static void ShowAward( string name, string icon = null, string description = null )
	{
		Event.Run( "boomer.giveaward", new PlayerAward( name, description ?? string.Empty, icon ?? "/ui/vitals/ammo.png" ) );
	}

	public void GiveAward( string name, string icon = null, string description = null )
	{
		GrantedAwards.Add( new PlayerAward( name, description, icon ) );

		ShowAward( To.Single( Client ), name, icon, description );
	}
}
