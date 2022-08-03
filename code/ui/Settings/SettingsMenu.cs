
using Boomer.UI;
using Sandbox.UI;

namespace Boomer
{
	[UseTemplate]
	internal class SettingsMenu : Panel
	{

		public Panel Canvas { get; protected set; }

		public SettingsMenu()
		{
			Current = this;

			Rebuild();
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Rebuild();
		}

		private void Rebuild()
		{
			Canvas.DeleteChildren( true );

			Canvas.AddChild( new SettingRow() );
			Canvas.AddChild( new SettingRow() );
			Canvas.AddChild( new SettingRow() );
			Canvas.AddChild( new SettingRow() );
		}

		public void Close() => SetOpen( false );

		private static SettingsMenu Current;
		public static void SetOpen( bool open ) => Current?.SetClass( "open", open );
		public static void ToggleOpen() => Current?.SetClass( "open", !Current.HasClass( "open" ) );

	}
}
