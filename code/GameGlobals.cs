global using System;
global using System.Linq;
global using System.Collections.Generic;

//
global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;

namespace Facepunch.Boomer;

public partial class GameGlobals
{
	[ConVar.Replicated( "boomer_gravity", Help = "Sets the world gravity.", Max = 2000, Min = 0, Name = "Gravity" )]
	public static float Gravity { get; set; } = 700f;
}
