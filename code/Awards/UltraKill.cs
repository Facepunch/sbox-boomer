[Library]
public partial class UltraKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/ultrakill.png" );
	public override string Name => "Ultra Kill";
	public override string Description => "Kill 6 players within a short time";
	public override string SoundName => "ultrakill";
}
