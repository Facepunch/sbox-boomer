
namespace Boomer;

internal class ClientSettings
{

	public WeaponPositionSetting WeaponPosition { get; set; }
	public bool TennisBallMode { get; set; } 
	// todo: TennisBallModeColor color editor
	public bool ShowHitMarkers { get; set; } = true;
	public bool ShowDamageNumbers { get; set; } = true;
	public bool BatchDamageNumbers { get; set; }
	// todo: CrosshairColor color editor
	public bool Speedometer { get; set; } = false;
	public GoreModeSetting GoreMode { get; set; }
	public bool WalkBob { get; set; } = true;
	public bool HearOwnFootsteps { get; set; } = true;
	public bool MuteGrunting { get; set; } = false;

	public void Save() => Cookie.Set( "boomer.clientsettings", this );

	private static ClientSettings current;
	public static ClientSettings Current
	{
		get
		{
			if( current == null ) 
				current = Cookie.Get<ClientSettings>( "boomer.clientsettings", new() );
			return current;
		}
	}

	public static void Reset()
	{
		current = new();
		current.Save();
	}

}

internal enum WeaponPositionSetting
{
	Center,
	Left,
	Right
}

internal enum GoreModeSetting
{
	NoRagdolls,
	NoGore,
	Nothing
}
