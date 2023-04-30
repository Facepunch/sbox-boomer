using Sandbox;
using System;

namespace Facepunch.Boomer;

public abstract partial class BasePickup : AnimatedEntity
{
	public virtual Model WorldModel => null;

	[Property, ResourceType( "vmdl" )] public string ModelPath { get; set; }

	[Property] public int RespawnTime { get; set; } = 30;
	[Property] public bool SpawnImmediate { get; set; } = true;

	[Net, Change( "OnAvailable" )] protected bool Available { get; set; } = false;
	public TimeUntil UntilRespawn { get; set; }

	public Action<BasePickup, Player> OnPickupAction;

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

		if ( !string.IsNullOrEmpty( ModelPath ) )
		{
			SetModel( ModelPath );

			PhysicsEnabled = true;
			UsePhysicsCollision = true;

			Tags.Add( "trigger" );
		}

		Sound.FromWorld( "armour.respawn", Position );
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
		if ( other is not Player player )
			return;

		if ( CanPickup( player ) )
		{
			OnPickup( player );
		}
	}

	/// <summary>
	/// Called to consume the current pickup - meaning it becomes unavailable
	/// Stops drawing, and stops being accessible
	/// </summary>
	protected void Consume()
	{
		UntilRespawn = RespawnTime;
		SetAvailable( false );
	}

	[GameEvent.Tick.Server]
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

	public virtual void OnPickup( Player player )
	{
		Consume();
		OnPickupAction?.Invoke( this, player );
	}

	public virtual bool CanPickup( Player player )
	{
		return Available && !Disabled;
	}
}
