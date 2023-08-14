using Sandbox;

namespace Facepunch.Boomer;

public partial class Player
{
	[ConCmd.Admin( "boomer_debug_overchargehp" )]
	public static void OverchargeHP()
	{
		(ConsoleSystem.Caller.Pawn as Player).Health = 200f;
	}
}
