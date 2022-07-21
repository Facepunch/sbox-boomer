[Library]
public partial class TripleKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/triplekill.png" );
	public override string Name => "Triple Kill";
	public override string Description => "Kill 3 players within a short time";
	public override string SoundName => "triplekill";
}
