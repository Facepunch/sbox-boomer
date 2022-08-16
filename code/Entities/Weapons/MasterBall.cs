namespace Boomer;

[Library( "boomer_masterball" ), HammerEntity]
[EditorModel( "models/dm_crowbar.vmdl" )]
[Title( "MasterBall" ), Category( "Weapons" )]
partial class MasterBall : DeathmatchWeapon
{
	public static Model WorldModel = Model.Load( "models/gameplay/weapons/masterball/w_masterball.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/masterball/masterball.vmdl";

	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 1.0f;
	public override AmmoType AmmoType => AmmoType.None;
	public override int Bucket => 6;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack();
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		// woosh sound
		// screen shake
		PlaySound( "dm.crowbar_attack" );

		Rand.SetSeed( Time.Tick );

		var forward = Owner.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0.1f;
		forward = forward.Normal;

		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 70, 15 ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 32, 25 )
				.UsingTraceResult( tr )
				.WithAttacker( Owner )
				.WithWeapon( this );

			tr.Entity.TakeDamage( damageInfo );
		}
		ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );
		ViewModelEntity?.SetAnimParameter( "attack", true );
		ViewModelEntity?.SetAnimParameter( "holdtype_attack", false ? 2 : 1 );
		if ( Owner is BoomerPlayer player )
		{
			player.SetAnimParameter( "b_attack", true );
		}
	}
	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		OnCarryDrop( ent );

	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 5 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );

		if ( Owner.IsValid() )
		{
			ViewModelEntity?.SetAnimParameter( "b_grounded", Owner.GroundEntity.IsValid() );
			ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );
		}
	}
}
