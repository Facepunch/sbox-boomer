[Library]
public partial class FirstBlood : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/firstblood.png" );
	public override string Name => "#Award.FirstBlood";
	public override string Description => "#Award.FirstBlood.Description";
	public override string SoundName => "first_blood";
}
