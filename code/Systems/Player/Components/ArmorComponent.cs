using Sandbox;

namespace Facepunch.Boomer;

public partial class ArmorComponent : EntityComponent<Player>, ISingletonComponent
{
	[Net] public float Current { get; set; }
	[Net] public float Max { get; set; }
	[Net] public TimeSince Tick { get; set; }

	public float TickFrequency => 0.5f;

	protected override void OnActivate()
	{
		Current = 0f;
		Max = 100f;
	}

	public void Give( float amount, bool ignoreMax = false )
	{
		if ( ignoreMax )
		{
			Current += amount;
			return;
		}

		Current = (Current + amount).Clamp( 0, Max );
	}

	public void Simulate( IClient cl )
	{
		if ( Current > Max )
		{
			if ( Tick > TickFrequency )
			{
				Current--;
				Tick = 0;
			}
		}
	}
}
