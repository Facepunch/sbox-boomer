using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace boomer.Hammer.Gameplay
{
	[Library( "shooter_startingweapons", Description = "Starting Weapons" )]
	[EditorSprite( "editor/ent_logic.vmat" )]
	[Display( Name = "Starting Weapons", GroupName = "Shooter", Description = "Coin Pickup." ), Category( "Gameplay" ), Icon( "currency_bitcoin" )]
	[HammerEntity]
	partial class StartingWeapons : Entity
	{
		
	}
}
