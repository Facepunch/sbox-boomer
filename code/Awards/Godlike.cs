[Library]
public partial class Godlike : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/godlike.png" );
	public override string Name => "#Award.Godlike";
	public override string Description => "#Award.Godlike.Description";

	public override string SoundName => "godlike";
}
