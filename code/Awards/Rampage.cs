[Library]
public partial class Rampage : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/rampage.png" );
	public override string Name => "#Award.Rampage";
	public override string Description => "#Award.Rampage.Description";
	public override string SoundName => "rampage";
}
