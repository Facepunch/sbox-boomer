[Library]
public partial class KillingSpree : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/killingspree.png" );
	public override string Name => "#Award.KillingSpree";
	public override string Description => "#Award.KillingSpree.Description";
	public override string SoundName => "killingspree";
}
