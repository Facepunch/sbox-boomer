namespace Facepunch.Boomer;

public static partial class Stats
{
	/// <summary>
	/// Gets an identifier for the stat - saving to gamemode data by default
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="gamemode"></param>
	/// <returns></returns>
	public static string GetIdent( string identifier, bool gamemode = true )
	{
		var ident = identifier;
		if ( gamemode ) ident = $"{ident}-{GamemodeSystem.Current?.GamemodeIdent ?? "tdm"}";
		return ident;
	}

	/// <summary>
	/// Increments a stat
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="gamemode"></param>
	public static void Increment( string identifier, bool gamemode = true )
	{
		Game.AssertClient( "Can't increment stats serverside" );
		Sandbox.Services.Stats.Increment( GetIdent( identifier, gamemode ), 1 );
	}

	/// <summary>
	/// [Rpc]
	/// <inheritdoc cref="Increment(string, bool)"/>
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="gamemode"></param>
	[ClientRpc]
	public static void RpcIncrement( string identifier, bool gamemode = true )
	{
		Increment( identifier, gamemode );
	}

	/// <summary>
	/// Sets a stat to a certain value
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="amount"></param>
	/// <param name="gamemode"></param>
	public static void Set( string identifier, double amount, bool gamemode = true )
	{
		Game.AssertClient( "Can't set stats serverside" );
		Sandbox.Services.Stats.SetValue( GetIdent( identifier, gamemode ), amount );
	}

	/// <summary>
	/// [Rpc]
	/// <inheritdoc cref="Set(string, double, bool)"/>
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="amount"></param>
	/// <param name="gamemode"></param>
	[ClientRpc]
	public static void RpcSet( string identifier, double amount, bool gamemode = true )
	{
		Set( identifier, amount, gamemode );
	}

	internal static Sandbox.Services.Stats.GlobalStat Get( string identifier, bool gamemode = true )
	{
		return Sandbox.Services.Stats.Global.Get( GetIdent( identifier, gamemode ) );
	}

	public static string GetGlobal( string identifier, bool gamemode = true )
	{
		var stat = Get( identifier );
		return $"{stat.Value}{stat.Unit}";
	}
}
