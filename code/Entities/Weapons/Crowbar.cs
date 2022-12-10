namespace Boomer;

[Library( "boomer_crowbar" ), HammerEntity]
[EditorModel( "models/dm_crowbar.vmdl" )]
[Title(  "Crowbar" ), Category( "Weapons" )]
partial class Crowbar : DeathmatchWeapon
{
	public static Model WorldModel = Model.Load( "models/dm_crowbar.vmdl" );
	public override string ViewModelPath => "models/v_dm_crowbar.vmdl";

	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 1.0f;
	public override AmmoType AmmoType => AmmoType.None;
	public override int Bucket => 5;

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

		Game.SetRandomSeed( Time.Tick );

		var forward = Player.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0.1f;
		forward = forward.Normal;

		foreach ( var tr in TraceBullet( Player.EyePosition, Player.EyePosition + forward * 70, 15 ) )
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

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
	}
}
