namespace Facepunch.Boomer.UI;

public class ObjectEditor : Panel
{
	public ObjectEditor()
	{
		Style.FlexDirection = FlexDirection.Column;
	}

	public void SetTarget( object target ) 
	{
		DeleteChildren( true );

		var properties = TypeLibrary.GetPropertyDescriptions( target );
		foreach ( var group in properties.GroupBy( x => x.Group ).Where( x => !string.IsNullOrEmpty( x.Key ) ) )
		{
			AddChild<Label>( "group" ).Text = group.Key;

			foreach( var property in group )
			{
				AddChild( new SettingRow( target, property ) );
			}
		}
	}
}
