using Sandbox.UI;

[UseTemplate]
public class AwardItem : Panel
{
	public float EndTime { get; set; }

	public Label Title { get; private set; }
	public Label Text { get; private set; }
	public Image Icon { get; private set; }
	public Award Award { get; private set; }

	public void Update( string title, string text )
	{
		Title.Text = title;
		Text.Text = text;
	}

	public void SetAward( Award award )
	{
		Award = award;
	}

	public void SetIcon( Texture texture )
	{
		Icon.Texture = texture;
	}

	public override void Tick()
	{
		if ( !IsDeleting && Time.Now >= EndTime )
		{
			Delete();
		}
	}
}
