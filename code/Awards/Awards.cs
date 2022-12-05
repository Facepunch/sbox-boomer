public static partial class Awards
{
	private static Dictionary<string, Award> Lookup { get; set; } = new();

	public static T Add<T>() where T : Award, new()
	{
		var type = typeof( T ).Name;

		if ( !Lookup.ContainsKey( type ) )
        {
			Lookup.Add( type, new T() );
		}

		return (T)Lookup[type];
	}

	public static Award Add( TypeDescription type )
	{
		var name = type.Name;

		if ( !Lookup.ContainsKey( name ) )
		{
			Lookup.Add( name, TypeLibrary.Create<Award>( type.TargetType ) );
		}

		return Lookup[name];
	}

	public static Award Get( string name )
	{
		if ( Lookup.TryGetValue( name, out var award ) )
			return award;

		var type = TypeLibrary.GetDescription( name );

		if ( type != null )
		{
			return Add( type );
		}

		return default;
	}

	public static Award Get<T>() where T : Award, new()
	{
		var type = typeof( T ).Name;
		return Get( type );
	}
}
