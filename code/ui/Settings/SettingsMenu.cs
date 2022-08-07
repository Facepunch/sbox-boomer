
using Boomer.UI;
using Sandbox.UI;

namespace Boomer
{
	[UseTemplate]
	internal class SettingsMenu : Panel
	{

		public ObjectEditor Editor { get; protected set; }

		private ClientSettings Settings;

		public SettingsMenu()
		{
			Current = this;
			Settings = ClientSettings.Current;

			Editor.SetTarget( Settings );
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Editor.SetTarget( Settings );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			Editor.SetTarget( Settings );
		}

		public void Close() => SetOpen( false );

		private static SettingsMenu Current;
		public static void SetOpen( bool open ) => Current?.SetClass( "open", open );
		public static void ToggleOpen() => Current?.SetClass( "open", !Current.HasClass( "open" ) );

		protected override void OnEvent( PanelEvent e )
		{
			base.OnEvent( e );

			if ( e.Name == "save" )
			{
				Settings.Save();
			}
		}

	}
}
