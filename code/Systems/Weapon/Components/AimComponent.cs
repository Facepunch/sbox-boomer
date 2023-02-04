using Sandbox;

namespace Facepunch.Boomer.WeaponSystem;

[Prefab]
public partial class Aim : WeaponComponent, ISingletonComponent
{
	[Prefab] public float AimTime { get; set; } = 0f;

	protected override bool CanStart( Player player )
	{
		if ( !Input.Down( InputButton.SecondaryAttack ) ) return false;

		return true;
	}

	protected override void OnStart( Player player )
	{
		base.OnStart( player );

		Weapon.Tags.Set( "aiming", true );
	}

	protected override void OnStop( Player player )
	{
		base.OnStop( player );

		Weapon.Tags.Set( "aiming", false );
	}

	public override void BuildInput()
	{
		if ( IsActive )
		{
			Input.AnalogLook *= 0.5f;
		}
	}
}
