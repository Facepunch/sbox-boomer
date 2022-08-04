
using Sandbox.UI;
using System.Reflection;

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

		var properties = target.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance );
		foreach ( var property in properties )
		{
			AddChild( new SettingRow( target, property ) );
		}
	}

}
