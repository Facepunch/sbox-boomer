[Library]
public partial class ComboBreaker : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/combobreaker.png" );
	public override string Name => "Combo Breaker";
	public override string Description => "Kill an enemy who has a Killing Spree";
	public override string SoundName => "combobreaker";
}

