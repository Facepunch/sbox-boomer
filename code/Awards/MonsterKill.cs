[Library]
public partial class MonsterKill : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/monsterkill.png" );
	public override string Name => "#Award.MonsterKill";
	public override string Description => "#Award.MonsterKill.Description";
	public override string SoundName => "monsterkill";
}
