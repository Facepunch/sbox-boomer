namespace Facepunch.Boomer;

public static partial class Stats
{
	/// <summary>
	/// Gets an identifier for the stat - saving to mode data by default
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="mode"></param>
	/// <returns></returns>
	public static string GetIdent( string identifier, string mode = null )
	{
		var ident = identifier;
		if ( string.IsNullOrEmpty( mode ) )
		{
			mode = GamemodeSystem.Current?.GamemodeIdent ?? "tdm";
		}

		return $"{ident}-{mode}";
	}

	/// <summary>
	/// Increments a stat
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="mode"></param>
	public static void Increment( string identifier, string mode = null )
	{
		Game.AssertClient( "Can't increment stats serverside" );
		Sandbox.Services.Stats.Increment( GetIdent( identifier, mode ), 1 );
	}

	/// <summary>
	/// [Rpc]
	/// <inheritdoc cref="Increment(string, string)"/>
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="mode"></param>
	[ClientRpc]
	public static void RpcIncrement( string identifier, string mode = null )
	{
		Increment( identifier, mode );
	}

	/// <summary>
	/// Sets a stat to a certain value
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="amount"></param>
	/// <param name="mode"></param>
	public static void Set( string identifier, double amount, string mode = null )
	{
		Game.AssertClient( "Can't set stats serverside" );
		Sandbox.Services.Stats.SetValue( GetIdent( identifier, mode ), amount );
	}

	/// <summary>
	/// [Rpc]
	/// <inheritdoc cref="Set(string, double, string)"/>
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="amount"></param>
	/// <param name="mode"></param>
	[ClientRpc]
	public static void RpcSet( string identifier, double amount, string mode = null )
	{
		Set( identifier, amount, mode );
	}

	public static Sandbox.Services.Stats.GlobalStat Get( string identifier, string mode = null )
	{
		return Sandbox.Services.Stats.Global.Get( GetIdent( identifier, mode ) );
	}

	public static string GetGlobal( string identifier, string mode = null )
	{
		var stat = Get( identifier, mode );
		return $"{stat.Value}{stat.Unit}";
	}
}
