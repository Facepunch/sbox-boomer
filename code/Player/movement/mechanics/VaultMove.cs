
using Sandbox;

namespace Boomer.Movement
{
	partial class VaultMove : BaseMoveMechanic
	{

		public float MinVaultHeight => 30f;
		public float MaxVaultHeight => 150f;
		public float MinVaultTime => .1f;
		public float MaxVaultTime => .65f;
		public float ClimbVaultMultiplier => 1.5f;

		public override bool TakesOverControl => true;

		private bool vaultingFromGround;
		private float vaultHeight;
		private TimeSince timeSinceVault;
		private Vector3 vaultStart, vaultEnd;
		private WallInfo wall;

		public VaultMove( BoomerController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( !InputActions.Jump.Down() ) return false;

			var wall = GetWallInfo( ctrl.Rotation.Forward );

			if ( wall == null ) return false;
			if ( wall.Value.Height == 0 ) return false;
			if ( wall.Value.Distance > ctrl.BodyGirth * 2 ) return false;
			if ( Vector3.Dot( ctrl.EyeRotation.Forward, wall.Value.Normal ) > -.5f ) return false;

			var posFwd = ctrl.Position - wall.Value.Normal * (ctrl.BodyGirth + wall.Value.Distance);
			var floorTraceStart = posFwd.WithZ( wall.Value.Height );
			var floorTraceEnd = posFwd.WithZ( ctrl.Position.z );

			var floorTrace = ctrl.TraceBBox( floorTraceStart, floorTraceEnd );
			if ( !floorTrace.Hit ) return false;
			if ( floorTrace.StartedSolid ) return false;

			var vaultHeight = floorTrace.EndPosition.z - ctrl.Position.z;
			if ( vaultHeight < MinVaultHeight ) return false;
			if ( vaultHeight > MaxVaultHeight ) return false;

			this.wall = wall.Value;
			this.vaultHeight = vaultHeight;
			vaultingFromGround = ctrl.GroundEntity != null;
			timeSinceVault = 0;
			vaultStart = ctrl.Position;
			vaultEnd = ctrl.Position.WithZ( floorTrace.EndPosition.z + 10 ) + ctrl.Rotation.Forward * ctrl.BodyGirth;
			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );

			return true;
		}

		public override void Simulate()
		{
			base.Simulate();

			var vaultTime = MinVaultTime.LerpTo( MaxVaultTime, vaultHeight / MaxVaultHeight );

			if ( !vaultingFromGround )
			{
				vaultTime *= ClimbVaultMultiplier;
			}

			if ( timeSinceVault <= vaultTime + Time.Delta )
			{
				var a = timeSinceVault / vaultTime;
				ctrl.Position = Vector3.Lerp( vaultStart, vaultEnd, a, false );
				ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
				return;
			}

			IsActive = false;
		}

	}
}
