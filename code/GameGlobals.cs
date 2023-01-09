using Sandbox;

namespace Facepunch.Boomer;

public partial class GameGlobals
{
	[ConVar.Replicated( "boomer_gravity", Help = "Sets the world gravity.", Max = 2000, Min = 0, Name = "Gravity" )]
	public static float Gravity { get; set; } = 700f;
}
