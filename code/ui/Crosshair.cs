using Boomer;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Crosshair : Panel
{
	public Image Icon;
	BoomerPlayer Player => BoomerCamera.Target ?? Game.LocalPawn as BoomerPlayer;

	public DeathmatchWeapon Weapon;

	private DeathmatchWeapon SelectedWeapon { get; set; }
	public Crosshair()
	{
		StyleSheet.Load( "/ui/Crosshair.scss" );

		Icon = Add.Image( "ui/crosshair/crosshair002.png", "icon" );
	}
	public override void Tick()
	{
		base.Tick();
		
		var player = Player;

		SelectedWeapon = player?.ActiveChild as DeathmatchWeapon;
		if ( SelectedWeapon != null )
		{
		var crosshair = SelectedWeapon.Crosshair;

		Icon.SetTexture( crosshair );
		}
	}
}
