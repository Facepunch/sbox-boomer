[Library]
public partial class ComboBreaker : Award
{
	public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/combobreaker.png" );
	public override string Name => "#Award.ComboBreaker";
	public override string Description => "#Award.ComboBreaker.Description";
	public override string SoundName => "combobreaker";
}

