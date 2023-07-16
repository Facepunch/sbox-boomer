namespace Facepunch.Boomer;

public partial class TimeComponent : EntityComponent/*<IClient> // wtf */ 
{
	public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow - TimeSpan.FromSeconds( 1 );

	public void Reset()
	{
		StartTime = DateTimeOffset.UtcNow;
	}

	protected override void OnActivate()
	{
		if ( Entity.Client == Game.LocalClient )
		{
			Reset();
		}
	}

	protected override void OnDeactivate()
	{
		if ( Entity.Client == Game.LocalClient )
		{
			Reset();
		}
	}

	/// <summary>
	/// Get offset time in seconds
	/// </summary>
	/// <returns></returns>
	public double GetOffset()
	{
		var now = DateTimeOffset.UtcNow;
		var diff = now - StartTime;
		return diff.TotalSeconds;
	}

	public void Save()
	{
		var offset = GetOffset();

		// Something went wrong if the offset is pretty high, or below zero
		if ( offset > 30f || offset <= 0f )
		{
			Reset();
			return;
		}

		Stats.RpcSet( "time", offset );

		Reset();
	}

	// Stat saving
	static TimeSince Saved;

	[GameEvent.Tick.Server]
	public static void TickServer()
	{
		if ( Saved > 5f )
		{
			Saved = 0;

			foreach ( var cl in Game.Clients )
			{
				cl.Components.Get<TimeComponent>()?.Save();
			}
		}
	}
}
