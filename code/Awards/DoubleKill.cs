[Library]
public partial class DoubleKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/doublekill.png" );
	public override string Name => "Double Kill";
	public override string Description => "Kill 2 players within a short time";
	public override string SoundName => "doublekill";
}
