[Library( "dm_GLGrenade" ), HammerEntity]
[Title( "GL Grenade" )]
partial class GLGrenade : BasePhysics
{
	public static readonly Model WorldModel = Model.Load( "models/dm_grenade.vmdl" );

	Particles GrenadeParticles;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		Tags.Add( "trigger" );

		GrenadeParticles = Particles.Create( "particles/grenade.vpcf", this, "trail_particle", true );
		GrenadeParticles.SetPosition( 0, Position );
	}

	public async Task BlowIn( float seconds )
	{
		await Task.DelaySeconds( seconds );

		DeathmatchGame.Explosion( this, Owner, Position, 400, 100, 1.0f );
		Delete();
	}
}
