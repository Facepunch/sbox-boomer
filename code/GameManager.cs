using Facepunch.Boomer.UI;
using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Boomer;

public partial class GameManager : Sandbox.GameManager
{
	public GameManager()
	{
		if ( Game.IsServer )
		{
			_ = new Hud();
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var player = new Player();
		client.Pawn = player;
		player.Respawn();

		Chat.AddChatEntry( To.Everyone, client.Name, "joined the game", client.SteamId, true );

		GamemodeSystem.Current?.OnClientJoined( client );
	}

	public void MoveToSpawnpoint( Player player )
	{
		var gamemode = GamemodeSystem.Current;

		gamemode?.PreSpawn( player );

		var transform = gamemode?.GetDefaultSpawnPoint( player );
		if ( transform is null )
		{
			// Grab an available spawnpoint as a fallback
			transform = Entity.All.OfType<SpawnPoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault()?.Transform;
		}

		// Did we fuck up?
		if ( transform is null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {player}" );
			return;
		}

		player.Transform = transform.Value;
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );
		Chat.AddChatEntry( To.Everyone, client.Name, "left the game", client.SteamId, true );

		GamemodeSystem.Current?.OnClientDisconnected( client, reason );
	}

	[Event.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		// Try to set up the active gamemode
		GamemodeSystem.SetupGamemode();
	}

	public override void BuildInput()
	{
		base.BuildInput();
		if ( Input.StopProcessing ) return;

		GamemodeSystem.Current?.BuildInput();
	}
}
