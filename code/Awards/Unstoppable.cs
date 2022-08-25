[Library]
public partial class Unstoppable : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/unstoppable.png" );
	public override string Name => "#Award.Unstoppable";
	public override string Description => "#Award.Unstoppable.Description";
	public override string SoundName => "unstoppable";
}
