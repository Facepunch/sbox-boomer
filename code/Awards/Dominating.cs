[Library]
public partial class Dominating : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/dominating.png" );
	public override string Name => "#Award.Dominating";
	public override string Description => "#Award.Dominating.Description";
	public override string SoundName => "dominating";
}
