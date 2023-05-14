using System.ComponentModel.DataAnnotations;

namespace Facepunch.Boomer;

public partial class ClientSettings
{
	[Display( Name = "#GameSettings.TennisBallMode", Description = "Sets all players to the same colour" )]
	public bool TennisBallMode { get; set; }
	// todo: TennisBallModeColor color editor

	[Display( Name = "#GameSettings.ShowMovementHint", Description = "#GameSettings.ShowMovementHint.Description" )]
	public bool ShowMovementHint { get; set; } = false;

	[Display( Name = "#GameSettings.ShowHitMarkers", Description = "#GameSettings.ShowHitMarkers.Description" )]
	public bool ShowHitMarkers { get; set; } = true;

	[Display( Name = "#GameSettings.ShowDamageNumbers", Description = "#GameSettings.ShowDamageNumbers.Description" )]
	public bool ShowDamageNumbers { get; set; } = true;

	[Display( Name = "#GameSettings.BatchDamageNumbers", Description = "#GameSettings.BatchDamageNumbers.Description" )]
	public bool BatchDamageNumbers { get; set; }

	// todo: CrosshairColor color editor
	[Display( Name = "#GameSettings.Speedometer", Description = "#GameSettings.Speedometer.Description" )]
	public bool Speedometer { get; set; } = false;

	[Display( Name = "#GameSettings.GoreMode", Description = "#GameSettings.GoreMode.Description" )]
	public GoreModeSetting GoreMode { get; set; }

	[Display( Name = "#GameSettings.WalkBob", Description = "#GameSettings.WalkBob.Description" )]
	public bool WalkBob { get; set; } = true;

	[Display( Name = "#GameSettings.HearOwnFootsteps", Description = "#GameSettings.HearOwnFootsteps.Description" )]
	public bool HearOwnFootsteps { get; set; } = true;

	[Display( Name = "#GameSettings.MuteGrunting", Description = "#GameSettings.MuteGrunting.Description" )]
	public bool MuteGrunting { get; set; } = false;

	[Display( Name = "#GameSettings.AnnouncerVolume", Description = "#GameSettings.AnnouncerVolume.Description" )]
	[MinMax( 0f, 1f ), SliderStep( .1f )]
	public float AnnouncerVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.MusicVolume", Description = "#GameSettings.MusicVolume.Description" )]
	[MinMax( 0f, 1f ), SliderStep( .1f )]
	public float MusicVolume { get; set; } = .1f;

	public void Save() => Cookie.Set( "boomer.clientsettings", this );

	private static ClientSettings current;
	public static ClientSettings Current
	{
		get
		{
			if ( current == null )
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

public enum GoreModeSetting
{
	NoRagdolls,
	NoGore,
	Nothing
}
