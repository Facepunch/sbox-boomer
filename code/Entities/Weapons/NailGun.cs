namespace Boomer;

[Library( "boomer_nailgun" ), HammerEntity]
[EditorModel( "models/gameplay/weapons/nailgun/w_nailgun.vmdl" )]
[Title( "NailGun" ), Category( "Weapons" )]
partial class NailGun : BulletDropWeapon<BulletDropProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/nailgun/w_nailgun.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/nailgun/nailgun.vmdl";
	public override string ProjectileModel => "models/gameplay/projectiles/nails/nails.vmdl";
	public override string TrailEffect => "particles/gameplay/weapons/nailgun/nail_trail.vpcf";
	public override string Crosshair => "ui/crosshair/crosshair007.png";
	public AnimatedEntity AnimationOwner => Owner as AnimatedEntity;
	public override float ProjectileLifeTime => 10f;
	public override float Gravity => 0f;
	public override float Speed => 3000f;
	public override float PrimaryRate => 12;
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

		var tr = Trace.Ray( Owner.EyePosition + new Vector3( 0, 0, -10 ), Owner.EyePosition + new Vector3( 0, 0, -10 ) + Owner.EyeRotation.Forward * 48 )
			.UseHitboxes()
			.WithAnyTags( "weapon", "player", "solid" )
			.Ignore( Owner )
			.Ignore( this )
			.Size( 12.0f )
			.Run();

		if ( tr.Hit )
		{
			var flGroundFactor = 0.75f;
			var flMul = 400f * 1.8f;
			var forMul = 110f * 1.4f;

			if ( Owner is BoomerPlayer player )
			{
				player.Velocity += player.EyeRotation.Backward * forMul * flGroundFactor;
			//	This just turns the nail climbing into rocket jumping
			//	player.Velocity = player.Velocity.WithZ( flMul * flGroundFactor );
			//	player.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;
			}

			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, 50, 1 )
				.UsingTraceResult( tr )
				.WithAttacker( Owner )
				.WithWeapon( this );

			Owner.TakeDamage( damageInfo );
		}

		AnimationOwner.SetAnimParameter( "b_attack", true );

		base.AttackPrimary();

		ShootEffects();
		PlaySound( "gl.shoot" );
		PlaySound( "ng.shoot" );
	}

	public override void BuildInput()
	{
		if ( Zoomed )
		{
			Input.AnalogLook *= 0.2f;
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
		//var draw = Render.Draw2D;

		//var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );
		//draw.BlendMode = BlendMode.Lighten;
		//draw.Color = color.WithAlpha( 0.4f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );

		//// outer lines
		//{
		//	var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.2f, 0.1f ) );
		//	var length = 10.0f + shootEase * 2.0f;
		//	var gap = 8.0f + shootEase * 10.0f;
		//	var thickness = 2.0f;

		//	draw.Line( thickness, center - new Vector2( 0, gap + length ), center - new Vector2( 0, gap ) );
		//	draw.Line( thickness, center + new Vector2( 0, gap + length ), center + new Vector2( 0, gap ) );

		//	draw.Line( thickness, center - new Vector2( gap + length, 0 ), center - new Vector2( gap, 0 ) );
		//	draw.Line( thickness, center + new Vector2( gap + length, 0 ), center + new Vector2( gap, 0 ) );

		//}
	}

	protected override void OnProjectileHit( BulletDropProjectile projectile, TraceResult trace )
	{
		if ( IsServer )
		{
			if ( trace.Entity.IsValid() )
			{
				DealDamage( trace.Entity, projectile.Position, 1f );
			}
		}
		else
		{
			var nail = new ModelEntity( projectile.ModelName )
			{
				EnableAllCollisions = false,
				Transform = projectile.Transform
			};

			nail.Position += nail.Rotation.Forward * 5f;
			nail.DeleteAsync( 10f );

			if ( trace.Entity.IsValid() )
			{
				nail.SetParent( trace.Entity, trace.Bone );
			}

			trace.Surface.DoBulletImpact( trace );
		}
	}
}
