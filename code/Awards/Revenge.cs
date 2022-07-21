[Library]
public partial class Revenge : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/revenge.png" );
	public override string Name => "Revenge";
	public override string Description => "Kill a player who was dominating you";
	public override string SoundName => "revenge1";
}
