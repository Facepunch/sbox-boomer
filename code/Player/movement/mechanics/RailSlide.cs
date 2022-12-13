
using System.Linq;
using Sandbox;

namespace Boomer.Movement;
internal class RailSlide : BaseMoveMechanic
{

	public override bool TakesOverControl => true;

	private GenericPathEntity Path;
	private int Node;
	private float Alpha;
	private TimeSince TimeSinceJump;
	private bool IsRailSliding;
	private bool DoOnce;
	private Sound railSlideSound;
	private bool Reverse;

	public RailSlide( BoomerController controller ) : base( controller )
	{
	}

	protected override bool TryActivate()
	{
		if ( TimeSinceJump < .3f ) return false;

		foreach ( var path in Entity.All.OfType<RailPathEntity>() )
		{
			if ( path.PathNodes.Count < 2 ) continue;

			var pa = path.NearestPoint( ctrl.Position + ctrl.Velocity * Time.Delta, false, out int na, out float ta );
			var pb = path.NearestPoint( ctrl.Position + ctrl.Velocity * Time.Delta, true, out int nb, out float tb );

			var dista = pa.Distance( ctrl.Position );
			var distb = pb.Distance( ctrl.Position );

			if( dista < 30 && ( na == 0 || dista < distb ) )
			{
				Path = path;
				Node = na;
				Alpha = ta;
				Reverse = false;
				return true;
			}

			if( distb < 30 && ( nb == path.PathNodes.Count - 1 || distb < dista ) )
			{
				Path = path;
				Node = nb;
				Alpha = tb;
				Reverse = true;
				return true;
			}
		}
		
		return false;
	}

	public override void PostSimulate()
	{
		base.PostSimulate();

		ctrl.GroundEntity = Path;
	}

	public override void Simulate()
	{
		base.Simulate();

		Alpha += Time.Delta;

		if ( Alpha >= 1f )
		{
			Alpha = 0;

			bool reachedEnd;

			if ( Reverse )
			{
				Node--;
				reachedEnd = Node <= 0;
			}
			else
			{
				Node++;
				reachedEnd = Node >= Path.PathNodes.Count - 1;
			}

			if ( reachedEnd )
			{
				IsActive = false;
				Path = null;
				SlideStopped();
				DoOnce = false;

				TimeSinceJump = 0;
				IsActive = false;

				// todo: add velocity up from rail normal,
				// and fix getting grounded immediately so we don't have to set position
				ctrl.Velocity = ctrl.Velocity.WithZ( 420f );
				ctrl.Position = ctrl.Position.WithZ( ctrl.Position.z + 10 );
				ctrl.GetMechanic<DoubleJump>().DoubleJumpsRemaining = 1;

				return;
			}
		}

		var currentNodeIdx = Node;
		var nextNodeIdx = Reverse ? ( Node - 1 ) : ( Node + 1 );

		var node = Path.PathNodes[currentNodeIdx];
		var nextNode = Path.PathNodes[nextNodeIdx];
		var currentPosition = ctrl.Position;
		var nextPosition = Path.GetPointBetweenNodes( node, nextNode, Alpha , Reverse );

		ctrl.Velocity = (nextPosition - currentPosition).Normal * 300f;
		ctrl.Position = nextPosition;

		var rot = Rotation.LookAt( ctrl.Velocity.Normal ).Angles();
		rot.roll = 0;

		ctrl.Rotation = Rotation.From( rot );
		ctrl.GroundEntity = Path;
		ctrl.SetTag( "skidding" );
	//	Particles.Create( "particles/gameplay/player/sliding/railsliding.vpcf", ctrl.Pawn );

		SlideEffect();
		IsRailSliding = true;

		if ( Input.Pressed( InputButton.Jump ) )
		{
			TimeSinceJump = 0;
			IsActive = false;

			// todo: add velocity up from rail normal,
			// and fix getting grounded immediately so we don't have to set position
			ctrl.Velocity = ctrl.Velocity.WithZ( 420f );
			ctrl.Position = ctrl.Position.WithZ( ctrl.Position.z + 10 );
			DoOnce = false;
			SlideStopped();
			ctrl.GetMechanic<DoubleJump>().DoubleJumpsRemaining = 1;
		}
	}

	private void SlideEffect()
	{

		if ( !Game.IsServer ) return;

		if ( IsRailSliding && !DoOnce )
		{
			DoOnce = true;

			using var _ = Prediction.Off();
			ctrl.Pawn.PlaySound( "rail.slide.start" );
			railSlideSound.Stop();
			railSlideSound = ctrl.Pawn.PlaySound( "rail.slide.loop" );
		//	Sound.FromEntity( "rail.slide.loop", ctrl.Pawn );


		}
	}
	private void SlideStopped()
	{
		railSlideSound.Stop();
		ctrl.Pawn.PlaySound( "rail.slide.end" );
	}
}
