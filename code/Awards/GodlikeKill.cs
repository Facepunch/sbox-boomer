[Library]
public partial class GodlikeKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/godlikekill.png" );
	public override string Name => "#Award.GodlikeKill";
	public override string Description => "#Award.Godlike.Description";
	public override string SoundName => "godlike";
}
