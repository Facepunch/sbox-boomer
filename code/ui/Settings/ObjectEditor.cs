﻿
using Sandbox.UI;
using System.Reflection;

namespace Boomer.UI;

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
		foreach ( var property in properties )
		{
			AddChild( new SettingRow( target, property ) );
		}
	}

}
