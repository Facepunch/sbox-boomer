[Library]
public partial class BeyondGodlike : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/beyondgodlike.png" );
	public override string Name => "Beyond Godlike";
	public override string Description => "Kill 8 players within a short time";
}
