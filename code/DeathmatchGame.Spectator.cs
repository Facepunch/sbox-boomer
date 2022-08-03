using Boomer.UI;

namespace Boomer;

/// <summary>
/// This is the heart of the gamemode. It's responsible for creating the player and stuff.
/// </summary>
partial class DeathmatchGame : Game
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

			player.UpdateClothes( To.Single( cl ) );
			player.Respawn();
			player.PlayerColor = Color.Random;

			BoomerChatBox.AddInformation( To.Everyone, $"{cl.Name} stopped spectating", $"avatar:{cl.PlayerId}" );
		}
		else
		{
			cl.Pawn?.TakeDamage( DamageInfo.Generic( 5000f ) );

			cl.Pawn.Delete();
			cl.Pawn = null;

			var pawn = new SpectatorPawn();
			cl.Pawn = pawn;
			pawn.Respawn();

			BoomerChatBox.AddInformation( To.Everyone, $"{cl.Name} started spectating", $"avatar:{cl.PlayerId}" );
		}
	}
}
