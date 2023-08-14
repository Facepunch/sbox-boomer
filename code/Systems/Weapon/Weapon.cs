using Sandbox;

namespace Facepunch.Boomer.WeaponSystem;

[Prefab, Title( "Weapon" ), Icon( "track_changes" )]
public partial class Weapon : AnimatedEntity
{
	public AnimatedEntity EffectEntity => ViewModelEntity.IsValid() ? ViewModelEntity : this;
	public WeaponViewModel ViewModelEntity { get; protected set; }
	public Player Player => Owner as Player;

	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;
	}

	/// <summary>
	/// Can we holster the weapon right now? Reasons to reject this could be that we're reloading the weapon..
	/// </summary>
	/// <returns></returns>
	public bool CanHolster( Player player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets holstered.
	/// </summary>
	public void OnHolster( Player player )
	{
		EnableDrawing = false;

		foreach ( var component in Components.GetAll<WeaponComponent>() )
		{
			component.Enabled = false;
		}
	}

	/// <summary>
	/// Can we deploy this weapon? Reasons to reject this could be that we're performing an action.
	/// </summary>
	/// <returns></returns>
	public bool CanDeploy( Player player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets deployed.
	/// </summary>
	public void OnDeploy( Player player )
	{
		SetParent( player, true );
		Owner = player;

		EnableDrawing = true;

		if ( Game.IsServer )
			CreateViewModel( To.Single( player ) );

		foreach ( var component in Components.GetAll<WeaponComponent>( true ) )
		{
			component.Enabled = true;
		}
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		if ( GetComponent<ViewModelComponent>() is not ViewModelComponent comp ) return;

		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( comp.ViewModelPath );
		ViewModelEntity = vm;
	}

	public override void Simulate( IClient cl )
	{
		SimulateComponents( cl );
	}

	protected override void OnDestroy()
	{
		ViewModelEntity?.Delete();
	}

	public override string ToString()
	{
		return $"Weapon ({Name})";
	}
}
