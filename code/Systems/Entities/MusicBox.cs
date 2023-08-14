using Editor;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Boomer;

[Library( "boomer_musicbox", Description = "Music Box" )]
[EditorSprite( "editor/ent_logic.vmat" )]
[Display( Name = "Music Box", GroupName = "Shooter", Description = "Coin Pickup." ), Category( "Gameplay" ), Icon( "currency_bitcoin" )]
[HammerEntity]
partial class MusicBox : Entity
{
	/// <summary>
	/// Name of the sound to play.
	/// </summary>
	[Property( "soundName" ), FGDType( "sound" )]
	[Net] public string SoundName { get; set; }

	public Sound? PlayingSound { get; protected set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[GameEvent.Tick.Client]
	public void Tick()
	{
		if ( !PlayingSound.HasValue )
		{
			OnStartSound();
		}
	}

	[ClientRpc]
	protected void OnStartSound()
	{
		PlayingSound = Sound.FromScreen( SoundName ).SetVolume( ClientSettings.Current.MusicVolume );
	}
}
