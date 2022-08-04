
using Sandbox.UI;

namespace Boomer.UI;

internal class ObjectEditor : Panel
{

	public ObjectEditor()
	{
		Style.FlexDirection = FlexDirection.Column;
	}

	public void SetTarget( object target ) 
	{
		DeleteChildren( true );

		var properties = target.GetType().GetProperties();
		foreach ( var property in properties )
		{
			AddChild( new SettingRow( target, property ) );
		}
	}

}
