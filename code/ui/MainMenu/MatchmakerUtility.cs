using System.Threading.Tasks;

namespace Facepunch.Boomer.Utility;

public partial class MatchmakerUtility
{
	static Sandbox.Services.ServerList serverList;

	public async static Task<Sandbox.Services.ServerList.Entry?> FindServer( int reservedSlots = 1 )
	{
		serverList?.Dispose();
		serverList = new Sandbox.Services.ServerList();

		// Look for our game
		serverList.AddFilter( "gametagsand", $"game:{Game.Menu.Package.FullIdent}" );

		// Search
		serverList.Query();

		while ( serverList.IsQuerying )
		{
			await Task.Delay( 100 );
		}

		Log.Info( $"We found {serverList.Count} servers" );

		if ( serverList.Count == 0 )
		{
			return null;
		}

		// TODO - Ignore servers with 0 players?
		var server = serverList
			.Where( x => x.SteamId != 0 )
			.Where( x => x.Players + reservedSlots <= x.MaxPlayers )
			.OrderByDescending( x => x.Players )
			.First();

		return server;
	}
}
