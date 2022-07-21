public abstract partial class Award
{
	public virtual Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/blue.png" );
	public virtual string Name => "";
	public virtual string Description => "";
	public virtual string SoundName => "award.earned";
	public virtual bool ClearQueue => false;

	public virtual Texture GetShowIcon()
	{
		return Icon;
	}

	public virtual void Show()
	{
		var item = new AwardItem();
		item.Update( Name, Description );
		item.SetIcon( GetShowIcon() );
		item.SetAward( this );
		AwardQueue.Instance.AddItem( item );
	}
}
