[Library]
public partial class MegaKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/megakill.png" );
	public override string Name => "#Award.MegaKill";
	public override string Description => "#Award.MegaKill.Description";
	public override string SoundName => "megakill";
}
