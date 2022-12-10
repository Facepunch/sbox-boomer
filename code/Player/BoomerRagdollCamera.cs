
namespace Sandbox
{
	public class BoomerRagdollCamera : BaseCamera
	{
		Vector3 FocusPoint;

		public override void BuildInput()
		{

		}

		public override void Update()
		{
			var player = Game.LocalPawn as Player;
			if ( !player.IsValid() ) return;

			// lerp the focus point
			FocusPoint = Vector3.Lerp( FocusPoint, GetSpectatePoint(), Time.Delta * 5.0f );

			var tr = Trace.Ray( FocusPoint + Vector3.Up * 12, FocusPoint + GetViewOffset() )
				.WorldOnly()
				.Ignore( player )
				.Radius( 6 )
				.Run();

			Camera.Position = tr.EndPosition;
			Camera.Rotation = player.EyeRotation;
			Camera.FieldOfView = Camera.FieldOfView.LerpTo( 90, Time.Delta * 3.0f );
			Camera.FirstPersonViewer = null;
		}

		public virtual Vector3 GetSpectatePoint()
		{
			if ( Game.LocalPawn is Player player && player.Corpse.IsValid() )
			{
				return player.Corpse.PhysicsGroup.MassCenter;
			}

			 return Game.LocalPawn.Position;
		}

		public virtual Vector3 GetViewOffset()
		{
			var player = Game.LocalPawn as Player;
			if ( player == null ) return Vector3.Zero;

			return player.EyeRotation.Forward * (-130 * 1) + Vector3.Up * (20 * 1);
		}
	}
}
