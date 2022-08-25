[Library]
public partial class DoubleKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/doublekill.png" );
	public override string Name => "#Award.DoubleKill";
	public override string Description => "#Award.DoubleKill.Description";
	public override string SoundName => "doublekill";
}
