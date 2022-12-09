using Boomer.UI;

namespace Boomer;

/// <summary>
/// This is the heart of the gamemode. It's responsible for creating the player and stuff.
/// </summary>
partial class DeathmatchGame : GameManager
{
	[ConCmd.Server( "boomer_togglespectator", Help = "Toggles spectator mode" )]
	public static void ToggleSpectator()
	{
		var cl = ConsoleSystem.Caller;

		var spectator = cl.Pawn is SpectatorPawn; 
		if ( spectator )
		{
			cl.Pawn.Delete();
			cl.Pawn = null;

			var player = new BoomerPlayer();
			cl.Pawn = player;

			player.RpcSetClothes( To.Single( cl ) );
			player.Respawn();

			BoomerChatBox.AddInformation( To.Everyone, $"{cl.Name} stopped spectating", $"avatar:{cl.SteamId}" );
		}
		else
		{
			(cl.Pawn as Entity).TakeDamage( DamageInfo.Generic( 5000f ) );

			cl.Pawn.Delete();
			cl.Pawn = null;

			var pawn = new SpectatorPawn();
			cl.Pawn = pawn;
			pawn.Respawn();

			BoomerChatBox.AddInformation( To.Everyone, $"{cl.Name} started spectating", $"avatar:{cl.SteamId}" );
		}
	}
}
