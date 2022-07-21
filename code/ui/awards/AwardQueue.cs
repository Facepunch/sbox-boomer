using Sandbox.UI;

public class AwardQueue : Panel
{
	public static AwardQueue Instance { get; private set; }

	public Queue<AwardItem> Queue { get; private set; }

	private Sound CurrentSound { get; set; }
	private Panel Container { get; set; }

	public AwardQueue()
	{
		Container = Add.Panel( "container" );
		Instance = this;
		Queue = new();
	}

	public void AddItem( AwardItem item )
	{
		if ( item.Award.ClearQueue )
		{
			Container.DeleteChildren( true );
			Queue.Clear();
		}

		Queue.Enqueue( item );
	}

	public void Next()
	{
		if ( Queue.Count > 0 )
		{
			var item = Queue.Dequeue();
			item.EndTime = Time.Now + 4f;

			CurrentSound.Stop();
			CurrentSound = Sound.FromScreen( item.Award.SoundName );

			Container.AddChild( item );
		}
	}

	public override void Tick()
	{
		if ( Queue.Count > 0 && Container.ChildrenCount == 0 )
		{
			Next();
		}

		base.Tick();
	}
}
