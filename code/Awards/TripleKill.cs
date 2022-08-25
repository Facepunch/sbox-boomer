[Library]
public partial class TripleKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/triplekill.png" );
	public override string Name => "#Award.TripleKill";
	public override string Description => "#Award.TripleKill.Description";
	public override string SoundName => "triplekill";
}
