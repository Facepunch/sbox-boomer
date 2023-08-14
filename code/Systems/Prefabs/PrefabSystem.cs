using Facepunch.Boomer.WeaponSystem;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Boomer;

public partial class PrefabSystem
{
	public static IEnumerable<Prefab> GetPrefabsOfType<T>() where T : Entity
	{
		return ResourceLibrary.GetAll<Prefab>()
			.Where( x => TypeLibrary.GetType( x.Root.Class ).TargetType == typeof( T ) );
	}
}
