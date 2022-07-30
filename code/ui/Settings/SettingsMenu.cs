
using Sandbox.UI;

namespace Boomer
{
	[UseTemplate]
	internal class SettingsMenu : Panel
	{

		private static SettingsMenu Current;

		public SettingsMenu()
		{
			Current = this;
		}

		public void Close() => SetOpen( false );
		public static void SetOpen( bool open ) => Current?.SetClass( "open", open );
		public static void ToggleOpen() => Current?.SetClass( "open", !Current.HasClass( "open" ) );

	}
}
