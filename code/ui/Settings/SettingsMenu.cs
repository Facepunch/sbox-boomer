
using Boomer.UI;
using Sandbox.UI;

namespace Boomer
{
	[UseTemplate]
	internal class SettingsMenu : Panel
	{

		public ObjectEditor Editor { get; protected set; }

		public SettingsMenu()
		{
			Current = this;

			Editor.SetTarget( ClientSettings.Current );
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Editor.SetTarget( ClientSettings.Current );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			Editor.SetTarget( ClientSettings.Current );
		}

		public void SetDefaults()
		{
			ClientSettings.Reset();

			Editor.SetTarget( ClientSettings.Current );
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
				ClientSettings.Current.Save();
			}
		}

	}
}
