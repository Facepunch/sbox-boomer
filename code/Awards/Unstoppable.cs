[Library]
public partial class Unstoppable : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/unstoppable.png" );
	public override string Name => "Unstoppable";
	public override string Description => "Kill 20 players without dying";
	public override string SoundName => "unstoppable";
}
