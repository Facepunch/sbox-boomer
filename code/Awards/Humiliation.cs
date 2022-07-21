[Library]
public partial class Humiliation : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/humiliation.png" );
	public override string Name => "Humiliation";
	public override string Description => "Melee an enemy player to death";
	public override string SoundName => "humiliation1";
}
