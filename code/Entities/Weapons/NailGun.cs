[Library( "dm_nailgun" ), HammerEntity]
[EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
[Title( "NailGun" ), Category( "Weapons" )]
partial class NailGun : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/nailgun/w_nailgun.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/nailgun/nailgun.vmdl";

	public AnimatedEntity AnimationOwner => Owner as AnimatedEntity;
	public override float PrimaryRate => 10;
	public override int Bucket => 1;
	public override AmmoType AmmoType => AmmoType.Nails;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
	}

	public override void AttackPrimary()
	{
		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}
			return;
		}
		
		AnimationOwner.SetAnimParameter( "b_attack", true );
		
		var tr = Trace.Ray( Owner.EyePosition + new Vector3( 0, 0, -10 ), Owner.EyePosition + new Vector3( 0, 0, -10 ) + Owner.EyeRotation.Forward * 48 )
		.UseHitboxes()
		//.HitLayer( CollisionLayer.Water, !InWater )
		.Ignore( Owner )
		.Ignore( this )
		.Size( 4.0f )
		.Run();

		if ( tr.Hit )
		{
			//
			//Push player back
			//
			float flGroundFactor = .75f;
			float flMul = 100f * 1.8f;
			float forMul = 585f * 1.4f;

			if ( Owner is BoomerPlayer player )
			{
				player.Velocity = player.EyeRotation.Backward * forMul * flGroundFactor;
				player.Velocity = player.Velocity.WithZ( flMul * flGroundFactor );
				player.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;
			}
			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, 50, 1 )
			.UsingTraceResult( tr )
			.WithAttacker( Owner )
			.WithWeapon( this );
			Owner.TakeDamage( damageInfo );
		}

		ShootEffects();
		PlaySound( "gl.shoot" );
		PlaySound( "ng.shoot" );

		// TODO - if zoomed in then instant hit, no travel, 120 damage

		if ( IsServer )
		{
			var bolt = new NailProjectile();
			bolt.Position = Owner.EyePosition + new Vector3(0,0,-10);
			bolt.Rotation = Owner.EyeRotation;
			bolt.Owner = Owner;
			bolt.Velocity = Owner.EyeRotation.Forward * 100;
			bolt.FromWeapon = this;

		}
	}
	public override void BuildInput( InputBuilder owner )
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairLastShoot = 0;
	}

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );
		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.2f + lastAttack.LerpInverse( 1.2f, 0 ) * 0.5f );

		// center
		{
			var shootEase = 1 + Easing.BounceIn( lastAttack.LerpInverse( 0.3f, 0.0f ) );
			draw.Ring( center, 15 * shootEase, 14 * shootEase );
		}
	}
}
