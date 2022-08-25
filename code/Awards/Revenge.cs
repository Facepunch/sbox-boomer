[Library]
public partial class Revenge : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/revenge.png" );
	public override string Name => "#Award.Revenge";
	public override string Description => "#Award.Revenge.Description";
	public override string SoundName => "revenge1";
}
