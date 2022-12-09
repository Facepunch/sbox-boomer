using Sandbox.UI;

namespace Boomer.UI;

public class HudRootPanel : RootPanel
{
	public static HudRootPanel Current;

	public Scoreboard Scoreboard { get; set; }

	public HudRootPanel()
	{
		Current = this;

		StyleSheet.Load( "/resource/styles/hud.scss" );
		SetTemplate( "/resource/templates/hud.html" );

		AddChild<Crosshair>();
		
		AddChild<DamageIndicator>();
		AddChild<HitIndicator>();

		AddChild<InventoryBar>();
		AddChild<PickupFeed>();
		AddChild<KilledHud>();
		AddChild<MovementHint>();

		AddChild<BoomerChatBox>();
		//AddChild<Speedo>();
		AddChild<KillFeed>();
		Scoreboard = AddChild<Scoreboard>();
		AddChild<VoiceList>();
		AddChild<VoiceSpeaker>();
		AddChild<MasterballHud>();
		AddChild<HudMarkers>();
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "game-end", DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd );
		SetClass( "game-warmup", DeathmatchGame.CurrentState == DeathmatchGame.GameStates.Warmup );
	}

	protected override void UpdateScale( Rect screenSize )
	{
		base.UpdateScale( screenSize );
	}
}
