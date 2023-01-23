using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Boomer;

public struct PlayerAward
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string Icon { get; set; }
}

public partial class Player
{
	public List<PlayerAward> GrantedAwards { get; set; } = new();

	[ClientRpc]
	public void ShowAward( string name, string icon = null, string description = null )
	{
		Event.Run( "boomer.giveaward", name, icon ?? "/ui/vitals/ammo.png", description ?? string.Empty );
	}

	public void GiveAward( string name, string icon = null, string description = null )
	{
		GrantedAwards.Add( new PlayerAward
		{
			Name = name,
			Description = description,
			Icon = icon
		} );

		ShowAward( To.Single( Client ), name, icon, description );
	}
}
