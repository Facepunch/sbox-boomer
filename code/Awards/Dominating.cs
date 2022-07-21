[Library]
public partial class Dominating : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/dominating.png" );
	public override string Name => "Dominating";
	public override string Description => "Kill the same enemy player 3 times";
	public override string SoundName => "dominating";
}
