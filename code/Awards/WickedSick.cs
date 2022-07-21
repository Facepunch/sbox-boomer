[Library]
public partial class WickedSick : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/wickedsick.png" );
	public override string Name => "Wicked Sick";
	public override string Description => "Kill 30 players without dying";
	public override string SoundName => "wickedsick";
}
