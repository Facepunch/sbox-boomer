using Sandbox;
using System.Threading.Tasks;

namespace Facepunch.Boomer;

public partial class DamageModComponent : EntityComponent<Player>, ISingletonComponent
{
	[Net] public float IncomingScale { get; set; }
	[Net] public float OutgoingScale { get; set; }

	/// <summary>
	/// Set thhe lifetime for this damage mod component, and start this process immediately.
	/// </summary>
	public float Lifetime
	{
		set
		{
			_ = AsyncDestroy( value );
		}
	}

	protected async Task AsyncDestroy( float seconds )
	{
		await GameTask.DelaySeconds( seconds );
		Remove();
	}
}
