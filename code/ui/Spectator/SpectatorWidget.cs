using Sandbox.UI;

namespace Boomer.UI;

[UseTemplate]
public partial class SpectatorWidget : Panel
{
	public Label TargetLabel { get; set; }
	public bool OverrideOn = true;

	public override void Tick()
	{
		base.Tick();

		var validTarget = BoomerCamera.IsSpectator;

		SetClass( "open", validTarget && OverrideOn );

		if ( validTarget )
		{
			TargetLabel.Text = $"{BoomerCamera.Target?.Client?.Name ?? "nobody"}";
		}

		if ( Input.Pressed( InputButton.Slot1 ) )
		{
			OverrideOn ^= true;
		}
	}
}
