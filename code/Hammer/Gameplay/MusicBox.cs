namespace Boomer;

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

	public Sound PlayingSound { get; protected set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public override void ClientSpawn()
	{
		//	OnStartSound();
	}

	[Event.Tick.Client]
	public void Tick()
	{
		if( DeathmatchGame.CurrentState == DeathmatchGame.GameStates.Live )
		{
			if ( PlayingSound.Index <= 0 )
			{
				OnStartSound();
			}
		}
	}
	
	[ClientRpc]
	protected void OnStartSound()
	{
		PlayingSound = Sound.FromScreen( SoundName ).SetVolume( .2f );
	}
}
