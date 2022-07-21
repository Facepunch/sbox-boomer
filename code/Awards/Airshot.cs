[Library]
public partial class Airshot : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/airshot.png" );
	public override string Name => "Airshot";
	public override string Description => "Hit a player in the air with a Rocket Launcher or Rail Gun";
	public override string SoundName => "midair1";
}
