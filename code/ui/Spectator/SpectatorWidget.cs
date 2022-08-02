using Sandbox.UI;

namespace Boomer.UI;

[UseTemplate]
public partial class SpectatorWidget : Panel
{
	public Label TargetLabel { get; set; }

	public override void Tick()
	{
		base.Tick();

		var validTarget = BoomerCamera.IsSpectator;

		SetClass( "open", validTarget );

		if ( validTarget )
		{
			TargetLabel.Text = $"{BoomerCamera.Target?.Client?.Name ?? "nobody"}";
		}
	}
}
