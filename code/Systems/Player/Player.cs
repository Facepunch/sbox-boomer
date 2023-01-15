using Facepunch.Boomer.Mechanics;
using Facepunch.Boomer.WeaponSystem;
using Sandbox;
using System.Linq;

namespace Facepunch.Boomer;

public partial class Player : AnimatedEntity
{
	/// <summary>
	/// The controller is responsible for player movement and setting up EyePosition / EyeRotation.
	/// </summary>
	[BindComponent] public PlayerController Controller { get; }

	/// <summary>
	/// The animator is responsible for animating the player's current model.
	/// </summary>
	[BindComponent] public PlayerAnimator Animator { get; }

	/// <summary>
	/// The inventory is responsible for storing weapons for a player to use.
	/// </summary>
	[BindComponent] public Inventory Inventory { get; }

	/// <summary>
	/// Armor component, responsible for controlling player armor.
	/// </summary>
	[BindComponent] public ArmorComponent ArmorComponent { get; }

	/// <summary>
	/// Time since the player last took damage.
	/// </summary>
	[Net, Predicted] public TimeSince TimeSinceDamage { get; set; }

	/// <summary>
	/// Accessor for getting a player's active weapon.
	/// </summary>
	public Weapon ActiveWeapon => Inventory?.ActiveWeapon;

	/// <summary>
	/// A camera is known only to the local client. This cannot be used on the server.
	/// </summary>
	public PlayerCamera PlayerCamera { get; protected set; }

	/// <summary>
	/// The information for the last piece of damage this player took.
	/// </summary>
	public DamageInfo LastDamage { get; protected set; }

	/// <summary>
	/// How long since the player last played a footstep sound.
	/// </summary>
	TimeSince TimeSinceFootstep = 0;

	/// <summary>
	/// A cached model used for all players.
	/// </summary>
	public static Model PlayerModel = Model.Load( "models/citizen/citizen.vmdl" );

	public Player()
	{
		ProjectileSimulator = new();
	}

	/// <summary>
	/// When the player is first created. This isn't called when a player respawns.
	/// </summary>
	public override void Spawn()
	{
		Model = PlayerModel;
		Predictable = true;

		// Default properties
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableLagCompensation = true;
		EnableHitboxes = true;

		Tags.Add( "player" );
	}

	/// <summary>
	/// Called when a player respawns, think of this as a soft spawn - we're only reinitializing transient data here.
	/// </summary>
	public void Respawn()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		Health = 100;
		LifeState = LifeState.Alive;
		EnableAllCollisions = true;
		EnableDrawing = true;

		// Re-enable all children.
		Children.OfType<ModelEntity>()
			.ToList()
			.ForEach( x => x.EnableDrawing = true );

		Components.Create<PlayerController>();

		// Remove old mechanics.
		Components.RemoveAny<PlayerControllerMechanic>();

		// Add mechanics.
		Components.Create<WalkMechanic>();
		Components.Create<JumpMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<CrouchMechanic>();
		Components.Create<UnstuckMechanic>();
		Components.Create<SlideMechanic>();
		Components.Create<DashMechanic>();

		Components.Create<PlayerAnimator>();
		Components.Create<ArmorComponent>();

		var inventory = Components.Create<Inventory>();

		var isOverriden = GamemodeSystem.Current?.PlayerLoadout( this ) ?? false;
		if ( !isOverriden )
		{
			foreach ( var wpn in WeaponData.All )
			{
				inventory.AddWeapon( WeaponData.CreateInstance( wpn ) );
			}
		}

		ResetInterpolation();

		ClientRespawn( To.Single( Client ) );

		SetupClothing();
	}

	/// <summary>
	/// Called clientside when the player respawns. Useful for adding components like the camera.
	/// </summary>
	[ClientRpc]
	public void ClientRespawn()
	{
		PlayerCamera = new PlayerCamera();
	}

	/// <summary>
	/// Called every server and client tick.
	/// </summary>
	/// <param name="cl"></param>
	public override void Simulate( IClient cl )
	{
		ProjectileSimulator.Simulate( cl );

		Rotation = LookInput.WithPitch( 0f ).ToRotation();

		Controller?.Simulate( cl );
		Animator?.Simulate( cl );

		// Simulate our active weapon if we can.
		Inventory?.Simulate( cl );
	}

	/// <summary>
	/// Entrypoint to update the player's camera.
	/// </summary>
	[Event.Client.PostCamera]
	protected void PostCameraUpdate()
	{
		PlayerCamera?.Update( this );

		// Apply camera modifiers after a camera update.
		CameraModifier.Apply();
	}

	/// <summary>
	/// Called every frame clientside.
	/// </summary>
	/// <param name="cl"></param>
	public override void FrameSimulate( IClient cl )
	{
		Rotation = LookInput.WithPitch( 0f ).ToRotation();

		Controller?.FrameSimulate( cl );
		Animator?.FrameSimulate( cl );

		// Simulate our active weapon if we can.
		Inventory?.FrameSimulate( cl );
	}

	[ClientRpc]
	public void SetAudioEffect( string effectName, float strength, float velocity = 20f, float fadeOut = 4f )
	{
		Audio.SetEffect( effectName, strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( GamemodeSystem.Current is Gamemode gamemode )
		{
			if ( !gamemode.AllowDamage )
				return;

			// Friendly fire
			if ( !gamemode.AllowFriendlyFire && info.Attacker.IsValid() && info.Attacker.Client.IsValid() )
			{
				var victimTeam = TeamSystem.GetTeam( Client );
				var attackerTeam = TeamSystem.GetTeam( info.Attacker.Client );

				// Prevent damage if they're both on the same team.
				if ( victimTeam == attackerTeam ) return;
			}
		}

		var attackerComponent = info.Attacker?.Components.Get<DamageModComponent>();
		var victimComponent = Components.Get<DamageModComponent>();

		// Check for headshot damage
		var isHeadshot = info.Hitbox.HasTag( "head" );
		if ( isHeadshot )
		{
			info.Damage *= 2.5f;
		}

		// If the attacker has a damage modifier component, we can apply a scale to outgoing damage.
		if ( attackerComponent != null )
		{
			info.Damage *= attackerComponent.OutgoingScale;
		}

		// If the victim has a damage modifier component, we can apply a scale to incoming damage.
		if ( victimComponent != null )
		{
			info.Damage *= victimComponent.IncomingScale;
		}

		// Check if we got hit by a bullet, if we did, play a sound.
		if ( info.HasTag( "bullet" ) )
		{
			Sound.FromScreen( To.Single( Client ), "sounds/player/damage_taken_shot.sound" );
		}

		// Play a deafening effect if we get hit by blast damage.
		if ( info.HasTag( "blast" ) )
		{
			SetAudioEffect( To.Single( Client ), "flasthbang", info.Damage.LerpInverse( 0, 60 ) );
		}

		if ( ArmorComponent != null )
		{
			ArmorComponent.Current -= info.Damage;

			if ( ArmorComponent.Current < 0 )
			{
				info.Damage = ArmorComponent.Current * -1;
				ArmorComponent.Current = 0;
			}
			else
			{
				info.Damage = 0;
			}
		}

		if ( Health > 0 && info.Damage > 0 )
		{
			TimeSinceDamage = 0;
			Health -= info.Damage;

			if ( Health <= 0 )
			{
				Health = 0;
				OnKilled();
			}
		}

		this.ProceduralHitReaction( info, 0.05f );
	}

	private async void AsyncRespawn()
	{
		await GameTask.DelaySeconds( 3f );
		Respawn();
	}

	public override void OnKilled()
	{
		if ( LifeState == LifeState.Alive )
		{
			// Default life state is Respawning, this means the player will handle respawning after a few seconds
			LifeState newLifeState = LifeState.Respawning;

			// Inform the active gamemode
			GamemodeSystem.Current?.OnPlayerKilled( this, LastDamage, out newLifeState );

			CreateRagdoll( Controller.Velocity, LastDamage.Position, LastDamage.Force,
				LastDamage.BoneIndex, LastDamage.HasTag( "bullet" ), LastDamage.HasTag( "blast" ) );

			EnableAllCollisions = false;
			EnableDrawing = false;

			Controller.Remove();
			Animator.Remove();
			Inventory.Remove();

			// Disable all children as well.
			Children.OfType<ModelEntity>()
				.ToList()
				.ForEach( x => x.EnableDrawing = false );

			// Inform the active gamemode
			GamemodeSystem.Current?.PostPlayerKilled( this, LastDamage );

			// TODO - Control this in the active gamemode
			if ( newLifeState == LifeState.Respawning )
				AsyncRespawn();
		}
	}

	/// <summary>
	/// Called clientside every time we fire the footstep anim event.
	/// </summary>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( !Game.IsClient )
			return;

		if ( LifeState != LifeState.Alive )
			return;

		if ( TimeSinceFootstep < 0.2f )
			return;

		volume *= GetFootstepVolume();

		TimeSinceFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	protected float GetFootstepVolume()
	{
		return Controller.Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 1f;
	}

	[ConCmd.Server( "kill" )]
	public static void DoSuicide()
	{
		(ConsoleSystem.Caller.Pawn as Player)?.TakeDamage( DamageInfo.Generic( 1000f ) );
	}

	[ConCmd.Server( "sethp" )]
	public static void SetHP( float value )
	{
		(ConsoleSystem.Caller.Pawn as Player).Health = value;
	}
}
