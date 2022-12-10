
using Sandbox;

namespace Boomer.Movement
{
	class MoveDebug : BaseMoveMechanic
	{

		public override bool AlwaysSimulate => true;

		public MoveDebug( BoomerController ctrl )
			: base( ctrl )
		{

		}

		public override void PostSimulate()
		{
			if ( BasePlayerController.Debug )
			{
				//var boxColor = Game.IsServer ? Color.Red : Color.Green;
				//DebugOverlay.Box( ctrl.Position + ctrl.TraceOffset, ctrl.Mins, ctrl.Maxs, boxColor );
				//DebugOverlay.Box( ctrl.Position, ctrl.Mins, ctrl.Maxs, boxColor );

				var lineOffset = Game.IsServer ? 10 : 0;
				var printTime = Game.IsServer ? 0f : 0.04f;
				// todo: print this shit so it doesn't flicker
				DebugOverlay.ScreenText( $"        Position: {ctrl.Position}", lineOffset + 0, printTime );
				DebugOverlay.ScreenText( $"        Velocity: {ctrl.Velocity}", lineOffset + 1, printTime );
				DebugOverlay.ScreenText( $"           Speed: {ctrl.Velocity.Length}", lineOffset + 2, printTime );
				DebugOverlay.ScreenText( $"    BaseVelocity: {ctrl.BaseVelocity}", lineOffset + 3, printTime );
				DebugOverlay.ScreenText( $"    GroundEntity: {ctrl.GroundEntity} [{ctrl.GroundEntity?.Velocity}]", lineOffset + 4, printTime );
				DebugOverlay.ScreenText( $"    WishVelocity: {ctrl.WishVelocity}", lineOffset + 5, printTime );
				DebugOverlay.ScreenText( $"		WallRunning: {ctrl.GetMechanic<WallRun>().IsActive}", 17, printTime );
				DebugOverlay.Sphere( ctrl.Position, 10f, Color.Red );
			}
		}
	}
}
