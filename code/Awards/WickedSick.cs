[Library]
public partial class WickedSick : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/wickedsick.png" );
	public override string Name => "#Award.WickedSick";
	public override string Description => "#Award.WickedSick.Description";
	public override string SoundName => "wickedsick";
}
