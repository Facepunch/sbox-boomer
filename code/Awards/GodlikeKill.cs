[Library]
public partial class GodlikeKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/godlikekill.png" );
	public override string Name => "Godlike Kill";
	public override string Description => "Kill 7 players within a short time";
	public override string SoundName => "godlike";
}
