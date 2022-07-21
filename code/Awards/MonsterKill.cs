[Library]
public partial class MonsterKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/monsterkill.png" );
	public override string Name => "Monster Kill";
	public override string Description => "Kill 5 players within a short time";
	public override string SoundName => "monsterkill";
}
