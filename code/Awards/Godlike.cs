[Library]
public partial class Godlike : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/godlike.png" );
	public override string Name => "Godlike";
	public override string Description => "Kill 25 players without dying";
}
