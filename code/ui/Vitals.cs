using Boomer;
using Sandbox.UI;
using Sandbox.UI.Construct;


public partial class HealthHud : Panel
{
	public Image Icon;
	public Label Value;
	public Label Total;
	
	public List<Panel> Segments;

	BoomerPlayer Player => BoomerCamera.Target ?? Local.Pawn as BoomerPlayer;
	
	public static HealthHud Current { get; private set; }
	public HealthHud()
	{
		Current = this;

		Value = Add.Label( "0", "health" );
		
		Icon = Add.Image( "ui/vitals/healthicon.png", "hpicon" );
		
		Total = Add.Label( "/100", "healthsupport" );
		
		var segmentsContainer = Add.Panel( "segments" );
		Segments = new List<Panel>();
		for ( int i = 0; i < 10; i++ )
			Segments.Add( segmentsContainer.Add.Panel() );
	}

	public override void Tick()
	{
		var player = Player;
		if ( !player.IsValid() ) 
			return;

		var col = ColorConvert.HSLToRGB((int)player.Health, 1.0f, 0.5f );

		Value.Text = $"{player.Health.CeilToInt()}";

		SetClass( "low", player.Health < 40.0f );
		SetClass( "overheal", player.Health > 100.0f );
		SetClass( "empty", player.Health <= 0.0f );

		var activeSegments = (int)MathF.Round( (player.Health / 100.0f) * Segments.Count );

		for ( int i = 0; i < Segments.Count; i++ )
		{
			var segment = Segments[i];

			if ( i < activeSegments )
			{
				segment.Style.BackgroundColor = col;
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

public class ArmourHud : Panel
{
	public Image Icon;
	public Label Value;
	public Label Total;

	public List<Panel> ArmourSegments;

	BoomerPlayer Player => BoomerCamera.Target ?? Local.Pawn as BoomerPlayer;

	public static ArmourHud Current { get; private set; }

	public ArmourHud()
	{
		Current = this;
		
		Value = Add.Label( "0", "armour" );
		
		Icon = Add.Image( "ui/vitals/armour.png", "armouricon" );

		Total = Add.Label( "/100", "armoursupport" );

		var segmentsContainer = Add.Panel( "armoursegments" );
		ArmourSegments = new List<Panel>();
		for ( int i = 0; i < 10; i++ )
			ArmourSegments.Add( segmentsContainer.Add.Panel() );

	}

	public override void Tick()
	{
		var player = Player;
		if ( !player.IsValid() )
			return;

		Value.Text = $"{player.Armour.CeilToInt()}";

		var col = ColorConvert.HSLToRGB( (int)player.Armour, 1.0f, 0.5f );

		Value.Text = $"{player.Armour.CeilToInt()}";

		SetClass( "low", player.Armour < 40.0f );
		SetClass( "overarmour", player.Armour > 100.0f );
		SetClass( "empty", player.Armour <= 0.0f );

		var activeSegments = (int)MathF.Round( (player.Armour / 100.0f) * ArmourSegments.Count );

		for ( int i = 0; i < ArmourSegments.Count; i++ )
		{
			var segment = ArmourSegments[i];

			if ( i < activeSegments )
			{
				segment.Style.BackgroundColor = col;
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
