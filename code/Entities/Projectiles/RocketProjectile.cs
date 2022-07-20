public partial class RocketProjectile : BulletDropProjectile
{
	private Sound RocketTrailSound { get; set; }

	public override void CreateEffects()
	{
		RocketTrailSound = Sound.FromEntity( "rl.trail", this );
		base.CreateEffects();
	}

	protected override void OnDestroy()
	{
		RocketTrailSound.Stop();
		base.OnDestroy();
	}
}
