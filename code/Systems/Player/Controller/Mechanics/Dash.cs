using Sandbox;

namespace Facepunch.Boomer.Mechanics;

public partial class DashMechanic : PlayerControllerMechanic
{
	[Net] public int DashCount { get; set; }

	public int MaxDashes => 2;
	public float DashRechargeCooldown => 2;

	protected override bool ShouldStart()
	{
		if ( DashCount < 1 ) return false;
		if ( !Input.Pressed( InputButton.Run ) ) return false;

		return true;
	}

	protected float GetPower()
	{
		return Controller.GroundEntity.IsValid() ? 1500 : 1250;
	}

	protected override void OnStart()
	{
		// Give a speed boost
		var wish = Controller.GetWishInput();

		// If you're not moving, default to forward
		if ( wish.Length.AlmostEqual( 0 ) )
			wish = Controller.Player.Rotation.Forward;

		Controller.Velocity = wish * GetPower();
		Controller.Velocity += Vector3.Up * 200f;

		DashCount--;
		DashEffect();
	}

	[Event.Tick.Server]
	public void TickServer()
	{
		if ( TimeSinceStop > DashRechargeCooldown && DashCount != MaxDashes )
		{
			DashCount = 2;

			if ( Controller.Player.IsLocalPawn )
				Sound.FromScreen( "dashrecharge" ).SetVolume( 1f );
		}
	}

	private void DashEffect()
	{
		if ( Game.IsServer || !Controller.Player.IsLocalPawn ) return;

		Particles.Create( "particles/gameplay/screeneffects/dash/ss_dash.vpcf", Controller.Player );
		Sound.FromWorld( "jump.double", Controller.Position );
	}
}
