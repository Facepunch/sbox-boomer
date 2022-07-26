using Boomer.Movement;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class DashCount : Panel
{
	//public Label dashnumber;
	public IconPanel dashicon;


	public List<Panel> DashSegments;
	public DashCount()
	{
		dashicon = Add.Icon( "double_arrow", "dashicon" );
		//dashnumber = Add.Label( "0", "dashcount" );

		var segmentsContainer = Add.Panel( "dashsegments" );
		DashSegments = new List<Panel>();
		for ( int i = 0; i < 2; i++ )
			DashSegments.Add( segmentsContainer.Add.Panel() );
	}

	public override void Tick()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return;
		if ( player.Controller is BoomerController controller )
		{
			//dashnumber.Text = $"{controller.DashCount}";


			var activeSegments = (int)MathF.Round( controller.DashCount );

			for ( int i = 0; i < DashSegments.Count; i++ )
			{
				var segment = DashSegments[i];

				if ( i < activeSegments )
				{
					segment.Style.BackgroundColor = Color.Parse( "#FAB002" );
					segment.Style.Opacity = 1.0f;
					segment.Style.Dirty();
					continue;
				}

				segment.Style.BackgroundColor = Color.Black;
				segment.Style.Opacity = 0.35f;
				segment.Style.Dirty();
			}

		}
	}
}
