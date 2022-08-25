[Library]
public partial class Airshot : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/airshot.png" );
	public override string Name => "#Award.Airshot";
	public override string Description => "#Award.Airshot.Description";
	public override string SoundName => "midair1";
}
