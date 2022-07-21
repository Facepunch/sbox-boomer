using Boomer;
using Boomer.Movement;

public partial class BoomerPlayer : Player
{
	TimeSince timeSinceDropped;

	[Net]
	public float Armour { get; set; } = 0;
	[Net]
	public float MaxArmour { get; set; } = 200;
	[Net]
	public float MaxHealth { get; set; } = 200;
	[Net]
	public int SpreeKills { get; set; } = 0;
	[Net]
	public int ConsecutiveKills { get; set; } = 0;

	public bool SupressPickupNotices { get; private set; }
	public int ComboKillCount { get; set; } = 0;
	public TimeSince TimeSinceLastDamage { get; set; }

	public ProjectileSimulator Projectiles { get; private set; }
	public List<Award> EarnedAwards { get; private set; }
	public TimeSince HealthTick { get; set; }
	public TimeSince ArmourTick { get; set; }

	[Net]
	private Material SkinMat { get; set; } = Material.Load( "models/gameplay/citizen/textures/citizen_skin.vmat" );

	[Net]
	private Color PlayerColor { get; set; } = Color.Random;

	public Dictionary<long, int> DominationTracker { get; private set; } = new();

	public BoomerPlayer()
	{
		DominationTracker = new();
		EarnedAwards = new();
		Projectiles = new( this );
		Inventory = new DmInventory( this );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new BoomerController();
		Animator = new StandardPlayerAnimator();
		CameraMode = new BoomerCamera();

		ConsecutiveKills = 0;
		SpreeKills = 0;
		Health = 100;
		Armour = 0;

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		SupressPickupNotices = true;

		var w = StartingWeapons.Instance;

		if ( w.IsValid() )
		{
			w.SetupPlayer( this );
		}
		else
		{
			Inventory.Add( new NailGun() );
			GiveAmmo( AmmoType.Nails, 250 );
		}

		SupressPickupNotices = false;

		SetMaterialOverride( SkinMat, "skin");
		RandomColor();

		base.Respawn();
	}

	public void GiveAward( string type )
	{
		var award = Awards.Get( type );
		if ( award == null ) return;
		ShowAward( To.Single( this ), type );
		EarnedAwards.Add( award );
	}

	public void GiveAward<T>() where T : Award
	{
		GiveAward( typeof( T ).Name );
	}

	[ClientRpc]
	public void ShowAward( string name )
	{
		var award = Awards.Get( name );
		if ( award == null ) return;
		EarnedAwards.Add( award );
		award.Show();
	}

	[ClientRpc]
	public void RandomColor()
	{
		SceneObject.Attributes.Set( "RimColor", PlayerColor );
	}

	[ConCmd.Admin]
	public static void GiveAll()
	{
		var ply = ConsoleSystem.Caller.Pawn as BoomerPlayer;

		ply.GiveAmmo( AmmoType.Rockets, 250 );
		ply.GiveAmmo( AmmoType.Buckshot, 250 );
		ply.GiveAmmo( AmmoType.Nails, 250 );
		ply.GiveAmmo( AmmoType.Rails, 250 );
		ply.GiveAmmo( AmmoType.Grenade, 250 );
		ply.GiveAmmo( AmmoType.Lightning, 250 );

		//ply.Inventory.Add( new Crowbar() );
		ply.Inventory.Add( new RocketLauncher() );
		ply.Inventory.Add( new Shotgun() );
		//ply.Inventory.Add( new NailGun() );
		ply.Inventory.Add( new RailGun() );
		ply.Inventory.Add( new GrenadeLauncher() );
		ply.Inventory.Add( new LightningGun() );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		var coffin = new Coffin();
		coffin.Position = Position + Vector3.Up * 30;
		coffin.Rotation = Rotation;
		coffin.PhysicsBody.Velocity = Velocity + Rotation.Forward * 100;

		coffin.Populate( this );

		Inventory.DeleteContents();

		if ( LastDamage.Flags.HasFlag( DamageFlags.Blast ) )
		{
			using ( Prediction.Off() )
			{
				var particles = Particles.Create( "particles/gib.vpcf" );
				if ( particles != null )
				{
					particles.SetPosition( 0, Position + Vector3.Up * 40 );
				}
			}
		}
		else
		{
			BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );
		}

		if ( LastDamage.Attacker is BoomerPlayer attacker && attacker != this )
		{
			if ( attacker.TimeSinceLastDamage < 3f )
			{
				attacker.ConsecutiveKills++;
				attacker.CalculateConsecutiveKill();
			}

			attacker.TimeSinceLastDamage = 0f;
			attacker.SpreeKills++;
			attacker.CalculateSpreeKill();

			if ( !LastDamage.Flags.HasFlag( DamageFlags.Blast ) && GetHitboxGroup( LastDamage.HitboxIndex ) == 1 )
			{
				attacker.PlaySoundFromScreen( To.Single( attacker ), "headshot" );
			}

			if ( ConsecutiveKills >= 5 )
			{
				attacker.GiveAward<ComboBreaker>();
			}

			if ( !DeathmatchGame.HasFirstPlayerDied )
			{
				DeathmatchGame.HasFirstPlayerDied = true;
				attacker.GiveAward<FirstBlood>();
			}

			attacker.TrackDominationKill( this );

			if ( attacker.GetDominationKills( this ) == 3 )
			{
				attacker.GiveAward<Dominating>();
			}

			if ( GetDominationKills( attacker ) >= 3 )
			{
				ClearDominationKills( attacker );
				attacker.GiveAward<Revenge>();
			}

			if ( attacker.ActiveChild is DeathmatchWeapon weapon && weapon.GivesAirshotAward )
			{
				var trace = Trace.Ray( attacker.Position, attacker.Position + Vector3.Down * 500f )
					.WorldOnly()
					.Ignore( attacker )
					.Ignore( attacker.ActiveChild )
					.Run();

				if ( !trace.Hit && !trace.StartedSolid )
				{
					attacker.GiveAward<Airshot>();
				}
			}
		}

		Controller = null;
		CameraMode = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;

		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			child.EnableDrawing = false;
		}
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd )
		{
			input.ViewAngles = input.OriginalViewAngles;
			return;
		};

		base.BuildInput( input );
	}

	public override void Simulate( Client cl )
	{
		Projectiles.Simulate();

		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd )
			return;

		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		TickPlayerUse();

		if ( Health > 100 )
		{
			if ( HealthTick > 1 )
			{
				Health.Clamp( 100, 200 );
				Health--;
				HealthTick = 0;
			}
		}
		if ( Armour > 100 )
		{
			if ( ArmourTick > 1 )
			{
				Armour.Clamp( 100, 200 );
				Armour--;
				ArmourTick = 0;
			}
		}

		SimulateActiveChild( cl, ActiveChild );

		if ( ActiveChild is DeathmatchWeapon weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f )
		{
			SwitchToBestWeapon();
		}
	}

	public void SwitchToBestWeapon()
	{
		var best = Children.Select( x => x as DeathmatchWeapon )
			.Where( x => x.IsValid() && x.IsUsable() )
			.OrderByDescending( x => x.BucketWeight )
			.FirstOrDefault();

		if ( best == null ) return;

		ActiveChild = best;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 )
			return;

		if ( other is PickupTrigger pickup )
		{
			StartTouch( pickup.Parent );
			return;
		}

		if ( other is DeathmatchWeapon wpn )
		{
			if ( Children.Any( x => x is DeathmatchWeapon w && w.ClassName == wpn.ClassName ) )
				return;
		}

		base.StartTouch( other );
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		base.PostCameraSetup( ref setup );

		setup.ZNear = 1f;

		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd ) return;
		if ( setup.Viewer == null ) return;

		AddCameraEffects( ref setup );
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		if ( Controller is BoomerController ctrl && (ctrl.IsSliding || ctrl.IsDashing) )
		{
			walkBob = 0f;
		}

		setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

		// Camera lean
		lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.01f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.3f;
		setup.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 4.0f );

		setup.FieldOfView += fov;

	}

	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState == LifeState.Dead )
			return;

		LastDamage = info;

		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 2.0f;
		}

		this.ProceduralHitReaction( info );
		ApplyForce( info.Force );

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;

		if ( IsServer && Armour > 0 )
		{
			Armour -= info.Damage;

			if ( Armour < 0 )
			{
				info.Damage = Armour * -1;
				Armour = 0;
			}
			else
			{
				info.Damage = 0;
			}
		}

		if ( Health > 0 && info.Damage > 0 )
		{
			Health -= info.Damage;
			if ( Health <= 0 )
			{
				Health = 0;
				OnKilled();
			}
		}

		if ( info.Attacker is BoomerPlayer attacker )
		{
			if ( attacker != this )
			{
				attacker.TimeSinceDamage = 0f;
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ) );
			}

			TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
		}

		if ( LifeState == LifeState.Dead && info.Attacker != null )
		{
			if ( info.Attacker.Client != null && info.Attacker != this )
			{
				info.Attacker.Client.AddInt( "kills" );
			}
		}
	}

	public void TrackDominationKill( BoomerPlayer victim )
	{
		var id = victim.Client.PlayerId;

		if ( DominationTracker.TryGetValue( id, out var kills ) )
		{
			DominationTracker[id] = kills + 1;
			return;
		}

		DominationTracker[id] = 1;
	}

	public void ClearDominationKills( BoomerPlayer victim )
	{
		var id = victim.Client.PlayerId;
		DominationTracker.Remove( id );
	}

	public int GetDominationKills( BoomerPlayer victim )
	{
		var id = victim.Client.PlayerId;

		if ( DominationTracker.TryGetValue( id, out var kills ) )
		{
			return kills;
		}

		return 0;
	}

	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv )
	{
		Sound.FromScreen( "dm.ui_attacker" )
			.SetPitch( 1 + healthinv * 1 );

		HitIndicator.Current?.OnHit( pos, amount );
	}

	public TimeSince TimeSinceDamage = 1.0f;

	[ClientRpc]
	public void TookDamage( Vector3 pos )
	{
		TimeSinceDamage = 0;
		DamageIndicator.Current?.OnHit( pos );
	}

	[ClientRpc]
	public void PlaySoundFromScreen( string sound )
	{
		Sound.FromScreen( sound );
	}

	[ConCmd.Client]
	public static void InflictDamage()
	{
		if ( Local.Pawn is BoomerPlayer ply )
		{
			ply.TookDamage( ply.Position + ply.EyeRotation.Forward * 100.0f );
		}
	}

	TimeSince timeSinceLastFootstep = 0;

	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive ) return;
		if ( !IsServer ) return;
		if ( timeSinceLastFootstep < 0.2f ) return;
		if ( Controller is BoomerController ctrl && (ctrl.IsSliding || ctrl.IsDashing) ) return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 10 );
	}

	[ConCmd.Admin]
	public static void MapVote()
	{
		_ = new MapVoteEntity();
	}

	public void RenderHud( Vector2 screenSize )
	{
		if ( LifeState != LifeState.Alive ) return;

		if ( ActiveChild is DeathmatchWeapon weapon )
		{
			weapon.RenderHud( screenSize );
		}
	}
	public void ApplyForce( Vector3 force )
	{
		if ( Controller is BoomerController controller )
		{
			controller.Impulse += force;
		}
	}

	protected void CalculateConsecutiveKill()
	{
		if ( ConsecutiveKills == 2 )
			GiveAward<DoubleKill>();
		else if ( ConsecutiveKills == 3 )
			GiveAward<TripleKill>();
		else if ( ConsecutiveKills == 4 )
			GiveAward<MegaKill>();
		else if ( ConsecutiveKills == 5 )
			GiveAward<MonsterKill>();
		else if ( ConsecutiveKills == 6 )
			GiveAward<UltraKill>();
		else if ( ConsecutiveKills == 7 )
			GiveAward<GodlikeKill>();
		else if ( ConsecutiveKills == 8 )
			GiveAward<BeyondGodlike>();
	}

	protected void CalculateSpreeKill()
	{
		if ( SpreeKills == 5 )
			GiveAward<KillingSpree>();
		else if ( SpreeKills == 10 )
			GiveAward<Rampage>();
		else if ( SpreeKills == 20 )
			GiveAward<Unstoppable>();
		else if ( SpreeKills == 25 )
			GiveAward<Godlike>();
		else if ( SpreeKills == 30 )
			GiveAward<WickedSick>();
	}

	[Event.Tick.Server]
	protected virtual void ServerTick()
	{
		if ( TimeSinceLastDamage > 3f && ConsecutiveKills > 0 )
		{
			ConsecutiveKills = 0;
		}
	}
}
