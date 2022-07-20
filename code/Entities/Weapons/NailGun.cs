[Library( "dm_nailgun" ), HammerEntity]
[EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
[Title( "NailGun" ), Category( "Weapons" )]
partial class NailGun : BulletDropWeapon<BulletDropProjectile>
{
	public static readonly Model WorldModel = Model.Load( "models/gameplay/weapons/nailgun/w_nailgun.vmdl" );
	public override string ViewModelPath => "models/gameplay/weapons/nailgun/nailgun.vmdl";
	public override string ProjectileModel => "models/editor/arrow.vmdl";
	public AnimatedEntity AnimationOwner => Owner as AnimatedEntity;
	public override float ProjectileLifeTime => 10f;
	public override float Gravity => 0f;
	public override float Speed => 2000f;
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

		base.AttackPrimary();

		ShootEffects();
		PlaySound( "gl.shoot" );
		PlaySound( "ng.shoot" );
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

	protected override void OnProjectileHit( BulletDropProjectile projectile, TraceResult trace )
	{
		if ( IsServer )
		{
			DealDamage( trace.Entity, projectile.Position, 1f );
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

			trace.Surface.DoBulletImpact( trace );
		}
	}
}
