
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Boomer.UI;

internal class SettingRow : Panel
{

	public Label Label { get;  }
	public Panel ValueArea { get; }

	public SettingRow()
	{
		Label = Add.Label( "Label" );
		Add.Panel().Style.FlexGrow = 1;
		ValueArea = Add.Panel( "value-area" );
		//ValueArea.Add.TextEntry( "Test" );

		var button = ValueArea.Add.Button( string.Empty, "toggle" );
		button.AddEventListener( "onmousedown", () =>
		{
			button.SetClass( "active", !button.HasClass( "active" ) );
		} );
	}

}
