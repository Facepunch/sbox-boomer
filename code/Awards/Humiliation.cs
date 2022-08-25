[Library]
public partial class Humiliation : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/humiliation.png" );
	public override string Name => "#Award.Humiliation";
	public override string Description => "#Award.Humiliation.Description";
	public override string SoundName => "humiliation1";
}
