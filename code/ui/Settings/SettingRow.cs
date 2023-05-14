namespace Facepunch.Boomer.UI;

internal class SettingRow : Panel
{
	public Label Label { get; }
	public Panel ValueArea { get; }

	public SettingRow()
	{
		Label = Add.Label( null, "title" );
		Add.Panel().Style.FlexGrow = 1;
		ValueArea = Add.Panel( "value-area" );
	}

	public SettingRow( object target, PropertyDescription property ) : this()
	{
		Label.Text = property.GetDisplayInfo().Name;

		var typeDesc = TypeLibrary.GetType( property.PropertyType );
		var currentValue = property.GetValue( target );

		if ( property.IsStatic )
		{
			Delete();
			return;
		}

		if ( property.PropertyType == typeof( bool ) )
		{
			var button = ValueArea.Add.Button( string.Empty, "toggle" );
			button.SetClass( "active", (bool)currentValue );
			button.AddEventListener( "onmousedown", () =>
			{
				button.SetClass( "active", !button.HasClass( "active" ) );
				property.SetValue( target, button.HasClass( "active" ) );
				CreateEvent( "save" );
			} );
		}

		if ( property.PropertyType == typeof( string ) )
		{
			var textentry = ValueArea.Add.TextEntry( (string)currentValue );
			textentry.AddEventListener( "value.changed", () =>
			{
				property.SetValue( target, textentry.Value );
				CreateEvent( "save" );
			} );
		}

		if ( property.PropertyType.IsEnum )
		{
			var dropdown = new DropDown( ValueArea );
			dropdown.SetPropertyObject( "value", currentValue );
			dropdown.AddEventListener( "value.changed", () =>
			{
				Enum.TryParse( property.PropertyType, $"{dropdown.Value}", out var newval );
				property.SetValue( target, newval );
				CreateEvent( "save" );
			} );
		}

		if ( property.PropertyType == typeof( float ) )
		{
			var minmax = property.GetCustomAttribute<MinMaxAttribute>();
			var min = minmax?.MinValue ?? 0f;
			var max = minmax?.MaxValue ?? 1000f;
			var step = property.GetCustomAttribute<SliderStepAttribute>()?.Step ?? .1f;
			var slider = ValueArea.Add.SliderWithEntry( min, max, step );
			slider.Bind( "value", target, property.Name );
			slider.AddEventListener( "value.changed", () =>
			{
				CreateEvent( "save" );
			} );
		}
	}
}
