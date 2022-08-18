namespace Boomer;

public partial class BasePickup : AnimatedEntity
{
	public virtual Model WorldModel => null;

	[Property] public int RespawnTime { get; set; } = 15;
	[Property] public bool RespawnImmediately { get; set; } = true;

	[Net, Change( "OnAvailable" )] protected bool Available { get; set; } = false;
	public TimeUntil UntilRespawn { get; set; }

	protected void OnAvailable( bool before, bool after )
	{
		EnableDrawing = after;
	}

	public void SetupModel()
	{
		if ( WorldModel != null )
		{
			Model = WorldModel;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;

			Tags.Add( "trigger" );
		}
	}

	public void SetAvailable( bool available )
	{
		var last = Available;
		Available = available;
		OnAvailable( last, available );
	}

	public override void Spawn()
	{
		base.Spawn();

		SetupModel();

		if ( RespawnImmediately )
		{
			UntilRespawn = 0;
			SetAvailable( true );
		}
		else
			UntilRespawn = RespawnTime;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );
		if ( other is not BoomerPlayer player )
			return;

		if ( CanPickup( player ) )
		{
			OnPickup( player );
		}
	}

	protected void Consume()
	{
		Model = null;
		UntilRespawn = RespawnTime;
		SetAvailable( false );

	}

	[Event.Tick.Server]
	protected void Tick()
	{
		if ( !Available && UntilRespawn )
		{
			SetAvailable( true );
			UntilRespawn = RespawnTime;
			SetupModel();
		}
	}

	public virtual void OnPickup( BoomerPlayer player )
	{
		Consume();
	}

	public virtual bool CanPickup( BoomerPlayer player )
	{
		return Available;
	}
}
