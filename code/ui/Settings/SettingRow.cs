
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Reflection;

namespace Boomer.UI;

internal class SettingRow : Panel
{

	public Label Label { get; }
	public Panel ValueArea { get; }

	private object Target;
	private PropertyInfo Property;

	public SettingRow( object target, PropertyInfo property ) : this()
	{
		Target = target;
		Property = property;

		Label.Text = property.Name;

		if( property.PropertyType == typeof( bool ) )
		{
			var button = ValueArea.Add.Button( string.Empty, "toggle" );
			button.SetClass( "active", (bool)property.GetValue( target ) );
			button.AddEventListener( "onmousedown", () =>
			{
				button.SetClass( "active", !button.HasClass( "active" ) );
				property.SetValue( target, button.HasClass( "active" ) );
				CreateEvent( "save" );
			} );
		}
	}

	public SettingRow()
	{
		Label = Add.Label( "Label" );
		Add.Panel().Style.FlexGrow = 1;
		ValueArea = Add.Panel( "value-area" );
	}

}
