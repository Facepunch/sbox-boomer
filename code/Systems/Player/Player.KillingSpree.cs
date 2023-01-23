using Sandbox;

namespace Facepunch.Boomer;

public partial class Player
{
	[Net] public int ConsecutiveKills { get; set; }

	protected void CalculateKillingSpree()
	{
		switch ( ConsecutiveKills )
		{
			case 2: GiveAward( "Double Kill" ); break;
			case 3: GiveAward( "Triple Kill" ); break;
			case 4: GiveAward( "Mega Kill" ); break;
			case 5: GiveAward( "Monster Kill" ); break;
			case 6: GiveAward( "Ultra Kill" ); break;
			case 7: GiveAward( "Godlike" ); break;
			case 8: GiveAward( "Beyond Godlike", null, "At this point... I've lost count" ); break;

			// Do nothing
			default:
				break;
		};
	}

	[Event.Tick.Server]
	protected virtual void ServerTick()
	{
		if ( TimeSinceKill > 4f && ConsecutiveKills > 0 )
		{
			ConsecutiveKills = 0;
		}
	}
}
