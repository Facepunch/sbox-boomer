[Library]
public partial class KillingSpree : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/killingspree.png" );
	public override string Name => "Killing Spree";
	public override string Description => "Kill 5 players without dying";
	public override string SoundName => "killingspree";
}
