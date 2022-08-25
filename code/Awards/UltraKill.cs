[Library]
public partial class UltraKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/ultrakill.png" );
	public override string Name => "#Award.UltraKill";
	public override string Description => "#Award.UltraKill.Description";
	public override string SoundName => "ultrakill";
}
