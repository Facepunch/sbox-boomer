using Sandbox;

namespace Facepunch.Boomer.WeaponSystem;

public partial class Weapon
{
	[Net, Prefab, Category( "Basic Information" ), ResourceType( "jpg" )]
	public string CrosshairIcon { get; set; }

	[Net, Prefab, Category( "Basic Information" ), ResourceType( "color" )]
	public Color WeaponColor { get; set; } = Color.White;

	[Net, Prefab, Category( "Basic Information" ), ResourceType( "jpg" )]
	public string Icon { get; set; }

	[Net, Prefab, Category( "Basic Information" ), ResourceType( "jpg" )]
	public string AmmoIcon { get; set; }

	[Net, Prefab, Category( "Animation" )]
	public HoldType HoldType { get; set; } = HoldType.Pistol;

	[Net, Prefab, Category( "Animation" )]
	public Handedness Handedness { get; set; } = Handedness.Both;

	[Net, Prefab, Category( "Animation" )]
	public float HoldTypePose { get; set; } = 0;

	// DELET EVERYTHING
	[Net, Change( nameof( OnWeaponDataChanged ) )] private WeaponData weaponData { get; set; }

	/// <summary>
	/// The weapon data resource. This drives all weapon stats and information.
	/// </summary>
	public WeaponData WeaponData
	{
		get => weaponData;
		set
		{
			weaponData = value;
			// SetupData( value );
		}
	}

	protected void OnWeaponDataChanged( WeaponData _, WeaponData data )
	{
		// SetupData( data );
	}
}
