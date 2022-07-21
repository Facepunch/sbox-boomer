[Library]
public partial class MegaKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/megakill.png" );
	public override string Name => "Mega Kill";
	public override string Description => "Kill 4 players within a short time";
	public override string SoundName => "megakill";
}
