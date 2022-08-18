namespace Boomer;

public partial class BasePickup : AnimatedEntity
{
	public virtual Model WorldModel => null;

	public int RespawnTime { get; set; } = 30;
	[Property] public bool SpawnImmediate { get; set; } = true;

	[Net, Change( "OnAvailable" )] protected bool Available { get; set; } = false;
	public TimeUntil UntilRespawn { get; set; }

	public Action<BasePickup, BoomerPlayer> OnPickupAction;

	private bool disabled;
	public bool Disabled
	{
		get => disabled;
		set
		{
			disabled = value;

			if ( disabled )
				Consume();
			else
				SetAvailable( true );
		}
	}

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

		EnableDrawing = Available;
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

		if ( SpawnImmediate )
		{
			UntilRespawn = 0;
			SetAvailable( true );
		}
		else
			UntilRespawn = RespawnTime + 30;
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

	/// <summary>
	/// Called to consume the current pickup - meaning it becomes unavailable
	/// Stops drawing, and stop s being accessible
	/// </summary>
	protected void Consume()
	{
		UntilRespawn = RespawnTime;
		SetAvailable( false );
	}

	[Event.Tick.Server]
	protected void Tick()
	{
		if ( Disabled ) return;

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
		OnPickupAction?.Invoke( this, player );
	}

	public virtual bool CanPickup( BoomerPlayer player )
	{
		return Available && !Disabled;
	}
}
