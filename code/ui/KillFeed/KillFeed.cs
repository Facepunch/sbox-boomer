using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.Boomer.UI;

public partial class Kills : Panel
{
    [Event( "boomer.kill" )]
    public void OnPlayerKilled( string attacker, string victim, string weapon )
    {
        AddEntry( attacker, victim, weapon );
    }

    public void AddEntry( string attacker, string victim, string weapon )
    {
		var e = new KillEntry()
		{
			AttackerName = attacker,
			VictimName = victim,
			WeaponIdent = weapon
		};

		AddChild( e );
    }
}
