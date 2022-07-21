[Library]
public partial class FirstBlood : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/firstblood.png" );
	public override string Name => "First Blood";
	public override string Description => "Get the first kill of the round";
	public override string SoundName => "first_blood";
}
