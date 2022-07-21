[Library]
public partial class Rampage : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/rampage.png" );
	public override string Name => "Rampage";
	public override string Description => "Kill 10 players without dying";
	public override string SoundName => "rampage";
}
