using Boomer.Movement;
using Boomer.UI;

namespace Boomer;

public partial class BoomerPlayer : Player, IHudMarker
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
	private Material EyeMat { get; set; } = Material.Load( "models/gameplay/citizen/textures/eyes/citizen_eyes_advanced.vmat" );

	[Net]
	public Color PlayerColor { get; set; }

	public Dictionary<long, int> DominationTracker { get; private set; } = new();

	[Net]
	public bool TaggedPlayer { get; set; } = false;
									
	public bool HasTheBall => Children.Any( x => x is MasterBall );

	[Net]
	public bool HasQuadDamage { get; set; } = false;

	public BoomerPlayer()
	{
		DominationTracker = new();
		EarnedAwards = new();
		Projectiles = new( this );
		Inventory = new DmInventory( this );
		PlayerColor = Color.Random;
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
		Armour = 50;

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		SupressPickupNotices = true;

		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.Live )
		{
			if ( DeathmatchGame.Current.NotUsingStartingGuns && !TaggedPlayer )
			{
				var w = StartingWeapons.Instance;
				Inventory.DeleteContents();
				if ( w.IsValid() )
				{
					w.SetupPlayer( this );
				}
			}
			else if ( DeathmatchGame.Current.NotUsingStartingGuns && TaggedPlayer )
			{
				Inventory.Add( new RailGun() );
				GiveAmmo( AmmoType.Rails, 100 );
			}
			else
			{
				var w = StartingWeapons.Instance;
				Inventory.DeleteContents();
				if ( w.IsValid() )
				{
					w.SetupPlayer( this );
				}
			}
		}

		SupressPickupNotices = false;

		var team = Client.GetTeam();
		if ( team != null )
			PlayerColor = team.Color;
			
		SetBodyGroup( "Hands", 1 );
		SetBodyGroup( "Feet", 1 );

		SetMaterialOverride( SkinMat, "skin" );
		SetMaterialOverride( EyeMat, "eyes" );
		RpcSetClothes();
		RpcSetPlayerColor();

		base.Respawn();
	}

	[Net]
	private Material ArmourMat { get; set; } = Material.Load( "models/gameplay/textures/textureatlas_player_outfit.vmat" );

	[ClientRpc]
	public void RpcSetClothes()
	{

		ModelEntity pants = new ModelEntity();
		pants.SetModel( "models/cosmetics/outfit/boomeroutfit_pants.vmdl" );
		pants.SetParent( this, true );
		pants.EnableHideInFirstPerson = true;
		pants.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		pants.EnableShadowInFirstPerson = true;

		ModelEntity shoes = new ModelEntity();
		shoes.SetModel( "models/cosmetics/outfit/boomeroutfit_shoes.vmdl" );
		shoes.SetParent( this, true );
		shoes.EnableHideInFirstPerson = true;
		shoes.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		shoes.EnableShadowInFirstPerson = true;

		ModelEntity helmet = new ModelEntity();
		helmet.SetModel( "models/cosmetics/outfit/boomeroutfit_helmet.vmdl" );
		helmet.SetParent( this, true );
		helmet.EnableHideInFirstPerson = true;
		helmet.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		helmet.EnableShadowInFirstPerson = true;

		ModelEntity gloves = new ModelEntity();
		gloves.SetModel( "models/cosmetics/outfit/boomeroutfit_gloves.vmdl" );
		gloves.SetParent( this, true );
		gloves.EnableHideInFirstPerson = true;
		gloves.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		gloves.EnableShadowInFirstPerson = true;

		ModelEntity chest = new ModelEntity();
		chest.SetModel( "models/cosmetics/outfit/boomeroutfit_chest.vmdl" );
		chest.SetParent( this, true );
		chest.EnableHideInFirstPerson = true;
		chest.SceneObject.Attributes.Set( "RimColor1", PlayerColor );
		chest.EnableShadowInFirstPerson = true;

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
	public void RpcSetPlayerColor()
	{
		SceneObject.Attributes.Set( "RimColor", PlayerColor );
	}

	[ConCmd.Admin]
	public static void GiveAll()
	{
		var ply = ConsoleSystem.Caller.Pawn as BoomerPlayer;

		if ( !ply.Children.OfType<RocketLauncher>().Any() )
			ply.Inventory.Add( new RocketLauncher() );

		if ( !ply.Children.OfType<Shotgun>().Any() )
			ply.Inventory.Add( new Shotgun() );

		if ( !ply.Children.OfType<NailGun>().Any() )
			ply.Inventory.Add( new NailGun() );

		if ( !ply.Children.OfType<RailGun>().Any() )
			ply.Inventory.Add( new RailGun() );

		if ( !ply.Children.OfType<GrenadeLauncher>().Any() )
			ply.Inventory.Add( new GrenadeLauncher() );

		if ( !ply.Children.OfType<LightningGun>().Any() )
			ply.Inventory.Add( new LightningGun() );

		if ( !ply.Children.OfType<MasterBall>().Any() )
			ply.Inventory.Add( new MasterBall() );

		ply.GiveAmmo( AmmoType.Rockets, 250 );
		ply.GiveAmmo( AmmoType.Buckshot, 250 );
		ply.GiveAmmo( AmmoType.Nails, 250 );
		ply.GiveAmmo( AmmoType.Rails, 250 );
		ply.GiveAmmo( AmmoType.Grenade, 250 );
		ply.GiveAmmo( AmmoType.Lightning, 250 );
	}


	protected void OnArmorPickUp( BasePickup pickup, BoomerPlayer player )
	{
		pickup?.Delete();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		var coffin = new Coffin();
		coffin.Position = Position + Vector3.Up * 30;
		coffin.Rotation = Rotation;
		coffin.PhysicsBody.Velocity = Velocity + Rotation.Forward * 100;

		coffin.Populate( this );

		if ( IsServer && !DeathmatchGame.InstaGib || !DeathmatchGame.MasterTrio || !DeathmatchGame.RailTag )
			using ( Prediction.Off() )
			{
				for ( int i = 0; i < 2; i++ )
				{
					var armour = new ArmorPickup()
					{
						Position = Position = Position + Vector3.Up * 30,
						OnPickupAction = OnArmorPickUp
					};
					armour.DeleteAsync( 60f );
					armour.PhysicsBody.Velocity = Owner.EyeRotation.Up * 200.0f + Owner.Velocity + Vector3.Random * 100.0f;
				}
			}

		Inventory.DeleteContents();

		if ( LastDamage.Flags.HasFlag( DamageFlags.Blast ) || DeathmatchGame.InstaGib || DeathmatchGame.RailTag )
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
			BecomeRagdollOnClient( LastDamage.Force, PlayerColor );
		}

		if ( LastDamage.Attacker is BoomerPlayer attacker && attacker != this )
		{
			if ( attacker.TimeSinceLastDamage < 4f )
			{
				attacker.ConsecutiveKills++;
				attacker.CalculateConsecutiveKill();

			}

			attacker.TimeSinceLastDamage = 0f;
			attacker.SpreeKills++;
			attacker.CalculateSpreeKill();

			attacker.PlaySoundFromScreen( To.Single( attacker ), "killsound" );

			attacker.TaggedPlayer = false;

			if ( !LastDamage.Flags.HasFlag( DamageFlags.Blast ) && LastDamage.Hitbox.HasTag( "head" ) && LastDamage.Weapon is RailGun )
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

			if ( DeathmatchGame.RailTag )
			{
				attacker.Inventory.DeleteContents();
			}
			
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
				var trace = Trace.Ray( Position, Position + Vector3.Down * 400f )
					.WorldOnly()
					.Ignore( this )
					.Ignore( ActiveChild )
					.Run();

				if ( !trace.Hit && !trace.StartedSolid )
				{
					attacker.GiveAward<Airshot>();
				}
			}

			if ( DeathmatchGame.RailTag )
			{
				TaggedPlayer = true;
			}
		}

		Controller = null;
		CameraMode = new BoomerRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;

		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			child.EnableDrawing = false;
		}
	}

	public override void BuildInput()
	{
		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd )
		{
			Input.AnalogLook = Angles.Zero;
			return;
		};

		if ( OverrideViewAngles )
		{
			OverrideViewAngles = false;
			ViewAngles = NewViewAngles;
		}

		base.BuildInput();
	}

	TimeSince PerSec = 0;

	public override void Simulate( Client cl )
	{
		Projectiles.Simulate();

		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd )
			return;

		base.Simulate( cl );

		if ( LifeState != LifeState.Alive )
			return;

		//We dont use anything.
		//TickPlayerUse();
		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.Live )
		{
			if ( !TaggedPlayer && DeathmatchGame.RailTag && PerSec >= 1f )
			{
				Client.AddInt( "kills" );
				PerSec = 0f;
			}
			else if ( HasTheBall && DeathmatchGame.MasterBall && PerSec >= 2f )
			{
				Client.AddInt( "kills" );
				PerSec = 0f;
			}
		}

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

		if ( ResetDmgCount >= 2 )
		{
			LastDamageDealt = 0;
			ResetDmgCount = 0;
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

		if ( other is MasterBall mb && mb.PickupCooldown > 0 )
			return;

		base.StartTouch( other );
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		base.PostCameraSetup( ref setup );

		setup.ZNear = 1f;

		if ( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd ) return;
		if ( setup.Viewer == null ) return;

		AddCameraEffects( ref setup );

		BaseCameraModifier.PostCameraSetup( ref setup );
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

		if ( !ClientSettings.Current.WalkBob )
		{
			walkBob = 0;
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

	[ClientRpc]
	public void OnDmgRpc()
	{
		Host.AssertClient();
		_ = ChangedHealthAnim();
	}

	[ClientRpc]
	public void OnArmourDmgRpc()
	{
		Host.AssertClient();
		_ = ChangedArmourAnim();
	}

	[ClientRpc]
	public void OnKilledRpc( BoomerPlayer attacker )
	{
		Host.AssertClient();
		_ = ShowDeathScreen( attacker );
	}

	protected static async Task ShowDeathScreen( BoomerPlayer attacker )
	{
		if ( !attacker.IsValid() )
			return;

		KilledHud.Current.AttackerName.SetText( $"{attacker.Client.Name} Killed You" );
		KilledHud.Current.AttackerAvatar.Style.SetBackgroundImage( $"avatar:{attacker.Client.SteamId}" );
		KilledHud.Current.AttackerHealth.SetText( $"{(int)attacker.Health}" );
		KilledHud.Current.AttackerArmour.SetText( $"{(int)attacker.Armour}" );
		KilledHud.Current.SetClass( "show", true );
		await GameTask.DelaySeconds( 3f );
		KilledHud.Current.SetClass( "show", false );
	}

	protected static async Task ChangedHealthAnim()
	{
		HealthHud.Current.Value.SetClass( "low", true );
		await GameTask.DelaySeconds( 0.25f );
		HealthHud.Current.Value.SetClass( "low", false );
	}

	protected static async Task ChangedArmourAnim()
	{
		ArmourHud.Current.Value.SetClass( "low", true );
		await GameTask.DelaySeconds( 0.25f );
		ArmourHud.Current.Value.SetClass( "low", false );
	}

	DamageInfo LastDamage;

	TimeSince ResetDmgCount;

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState == LifeState.Dead )
			return;

		var attacker = info.Attacker as BoomerPlayer;

		if ( attacker.IsValid() && attacker != this && !DeathmatchGame.FriendlyFire && DeathmatchGame.IsTeamPlayEnabled )
		{
			var myTeam = Client.GetTeam();
			var theirTeam = attacker.Client.GetTeam();

			if ( myTeam.IsFriend( theirTeam ) )
			{
				return;
			}
		}

		LastDamage = info;

		if ( info.Hitbox.HasTag( "head" ) && info.Weapon is RailGun && !DeathmatchGame.Current.InstaKillRail )
		{
			info.Damage = 100.0f;
		}

		if ( attacker != null )
		{
			if ( attacker.HasQuadDamage )
			{
				info.Damage *= 4.0f;
			}
		}

		this.ProceduralHitReaction( info );
		ApplyForce( info.Force );

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;


		if ( IsServer && Armour > 0 )
		{
			var lastArmor = Armour;

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

			if ( Armour < lastArmor )
				OnArmourDmgRpc( To.Single( Client ) );

			if ( attacker.IsValid() )
			{
				if ( attacker != this )
				{
					attacker.TimeSinceLastDamage = 0f;
					attacker.DidArmorDamage( To.Single( attacker ), info.Position, lastArmor - Armour, Armour.LerpInverse( 100, 0 ) );
				}
			}
		}

		if ( Health > 0 && info.Damage > 0 )
		{
			Health -= info.Damage;
			if ( Health <= 0 )
			{
				Health = 0;
				OnKilled();
				OnKilledRpc( To.Single( Client ), attacker );
			}
			OnDmgRpc( To.Single( Client ) );
		}

		if ( attacker.IsValid() )
		{
			if ( attacker != this )
			{
				attacker.TimeSinceLastDamage = 0f;
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ), Armour );
			}

			if ( info.Damage > 0 )
				TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
		}

		if ( LifeState == LifeState.Dead && info.Attacker != null && !DeathmatchGame.RailTag )
		{
			if ( info.Attacker.Client != null && info.Attacker != this )
			{
				info.Attacker.Client.AddInt( "kills" );
			}
		}
	}

	public void TrackDominationKill( BoomerPlayer victim )
	{
		var id = victim.Client.SteamId;

		if ( DominationTracker.TryGetValue( id, out var kills ) )
		{
			DominationTracker[id] = kills + 1;
			return;
		}

		DominationTracker[id] = 1;
	}

	public void ClearDominationKills( BoomerPlayer victim )
	{
		var id = victim.Client.SteamId;
		DominationTracker.Remove( id );
	}

	public int GetDominationKills( BoomerPlayer victim )
	{
		var id = victim.Client.SteamId;

		if ( DominationTracker.TryGetValue( id, out var kills ) )
		{
			return kills;
		}

		return 0;
	}

	[ClientRpc]
	public void KillSound()
	{
		Sound.FromScreen( "killsound" ).SetVolume( 10f );
	}

	public float LastDamageDealt { get; set; }

	private TimeSince TimeSinceArmorDamageEffect;
	[ClientRpc]
	public void DidArmorDamage( Vector3 pos, float amount, float armorinv )
	{
		LastDamageDealt += amount;
		ResetDmgCount = 0;

		if ( TimeSinceArmorDamageEffect > .1f )
		{
			HitIndicator.Current?.OnHit( pos, amount );
			Sound.FromScreen( "hitsound" ).SetPitch( 1 + armorinv / 2 );
			TimeSinceArmorDamageEffect = 0f;
		}

		if ( !ClientSettings.Current.ShowDamageNumbers ) return;
		if ( ClientSettings.Current.BatchDamageNumbers )
		{
			DamageNumbers.Enqueue( pos, amount, true );
			return;
		}

		DamageNumbers.Create( pos, amount, true );
	}

	private TimeSince TimeSinceDamageEffect;
	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv, float armour )
	{
		LastDamageDealt += amount;
		ResetDmgCount = 0;

		if ( TimeSinceDamageEffect > .1f )
		{
			HitIndicator.Current?.OnHit( pos, amount );
			Sound.FromScreen( "hitsound" ).SetPitch( 1 + healthinv / 2 );
			TimeSinceDamageEffect = 0f;
		}

		if ( !ClientSettings.Current.ShowDamageNumbers ) return;
		if ( ClientSettings.Current.BatchDamageNumbers )
		{
			DamageNumbers.Enqueue( pos, amount );
			return;
		}

		DamageNumbers.Create( pos, amount );
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

	private bool OverrideViewAngles = false;
	private Angles NewViewAngles;
	[ClientRpc]
	public void SetViewAngles( Angles angles )
	{
		OverrideViewAngles = true;
		NewViewAngles = angles;
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
		RenderControlHud( screenSize );
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
		if ( TimeSinceLastDamage > 4f && ConsecutiveKills > 0 )
		{
			ConsecutiveKills = 0;
		}
	}

	public virtual void RenderControlHud( in Vector2 screensize )
	{
		var center = screensize * 0.75f;

		RenderCrosshair( center, 0, 0 );
	}

	public void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		//var draw = Render.Draw2D;

		//var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 1.0f, 1.4f ) );
		//draw.BlendMode = BlendMode.Lighten;
		//draw.Color = Color.White.WithAlpha( 0.25f );
		//if(ClientSettings.Current.ShowMovementHint)
		//// outer lines
		//{
		//	var length = 22.0f;
		//	var gap = 48.0f;
		//	var thickness = 2.0f;

		//	if ( Input.Down( InputButton.Forward ) )
		//	{
		//		draw.Line( thickness, center - new Vector2( 0, gap + length ), center - new Vector2( 20, gap ) );
		//		draw.Line( thickness, center - new Vector2( 0, gap + length ), center - new Vector2( -20, gap ) );
		//	}
		//	if ( Input.Down( InputButton.Back ) )
		//	{
		//		draw.Line( thickness, center + new Vector2( 0, gap + length ), center + new Vector2( 20, gap ) );
		//		draw.Line( thickness, center + new Vector2( 0, gap + length ), center + new Vector2( -20, gap ) );
		//	}
		//	if ( Input.Down( InputButton.Left ) )
		//	{
		//		draw.Line( thickness, center - new Vector2( gap + length, 0 ), center - new Vector2( gap, 20 ) );
		//		draw.Line( thickness, center - new Vector2( gap + length, 0 ), center - new Vector2( gap, -20 ) );
		//	}
		//	if ( Input.Down( InputButton.Right ) )
		//	{
		//		draw.Line( thickness, center + new Vector2( gap + length, 0 ), center + new Vector2( gap, 20 ) );
		//		draw.Line( thickness, center + new Vector2( gap + length, 0 ), center + new Vector2( gap, -20 ) );
		//	}
		//}
	}
}
