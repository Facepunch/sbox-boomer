
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Reflection;

namespace Boomer.UI;

internal class SettingRow : Panel
{

	public Label Label { get; }
	public Panel ValueArea { get; }

	public SettingRow( object target, PropertyInfo property ) : this()
	{
		var di = DisplayInfo.ForProperty( property );

		Label.Text = di.Name;

		if ( property.PropertyType == typeof( bool ) )
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

		if( property.PropertyType == typeof( string ) )
		{
			var value = (string)property.GetValue( target );
			var textentry = ValueArea.Add.TextEntry( value );
			textentry.AddEventListener( "value.changed", () =>
			{
				property.SetValue( target, textentry.Text );
				CreateEvent( "save" );
			} );
		}

		if( property.PropertyType.IsEnum )
		{
			var value = property.GetValue( target ).ToString();
			var dropdown = new DropDown( ValueArea );
			dropdown.SetPropertyObject( "value", property.GetValue( target ) );
			dropdown.AddEventListener( "value.changed", () =>
			{
				Enum.TryParse( property.PropertyType, dropdown.Value, out var newval );
				property.SetValue( target, newval );
				CreateEvent( "save" );
			} );
		}

		if( property.PropertyType == typeof( float ) )
		{
			var value = (float)property.GetValue( target );
			var minmax = property.GetCustomAttribute<MinMaxAttribute>();
			var min = minmax?.MinValue ?? 0f;
			var max = minmax?.MaxValue ?? 1000f;
			var slider = ValueArea.Add.SliderWithEntry( min, max, .1f );
			slider.Bind( "value", target, property.Name );
			slider.AddEventListener( "value.changed", () =>
			{
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
