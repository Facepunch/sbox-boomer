
namespace Boomer;

internal class ClientSettings
{

	public WeaponPositions WeaponPosition { get; set; }
	public bool SomeToggle { get; set; }
	public float SomeFloat { get; set; }
	public string SomeString { get; set; }

	public void Save() => Cookie.Set( "boomer.clientsettings", this );
	public static ClientSettings Current => Cookie.Get<ClientSettings>( "boomer.clientsettings", new() );

}

internal enum WeaponPositions
{
	Left,
	Center,
	Right
}
