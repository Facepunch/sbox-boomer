using Sandbox;
using Editor;

namespace Facepunch.Boomer;

[Library( "boomer_damagemodpickup" ), HammerEntity]
[EditorModel( "models/gameplay/healthkit/healthkit.vmdl" )]
[Title( "Damage Modifier Pickup" ), Category( "Pickups" )]
partial class DamageModPickup : BasePickup
{
	public override Model WorldModel => Model.Load( "models/gameplay/healthkit/healthkit.vmdl" );

	[Property]
	public float DamageModifierLifetime { get; set; } = 30f;

	[Property]
	public float IncomingDamageScale { get; set; } = 1f;

	[Property]
	public float OutgoingDamageScale { get; set; } = 4f;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		PhysicsEnabled = true;
		UsePhysicsCollision = true;

		Tags.Add( "trigger" );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( Game.IsServer )
		{
			if ( other is not Player pl ) return;
			if ( pl.Components.Get<DamageModComponent>() != null ) return;

			var component = pl.Components.GetOrCreate<DamageModComponent>();
			component.IncomingScale = IncomingDamageScale;
			component.OutgoingScale = OutgoingDamageScale;
			component.Lifetime = DamageModifierLifetime;

			PickEffect( pl );
			PlayPickupSound();
		}
	}

	[ClientRpc]
	private void PlayPickupSound()
	{
		Sound.FromWorld( "health.pickup", Position );
	}

	private void PickEffect( Player player )
	{
		if ( Game.IsServer || !player.IsLocalPawn )
			return;

		Particles.Create( "particles/gameplay/screeneffects/healthpickup/ss_healthpickup.vpcf", player );
	}
}
