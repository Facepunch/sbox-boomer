
using Sandbox.UI;

namespace Boomer.UI;

public partial class MasterballHud : Panel
{

	public static MasterballHud Current;

	public void Notify( string message )
	{
		DeleteChildren(); // uncomment this to only have 1 notify on screen always
		AddChild( new NotifyLabel()
		{
			Text = message
		} );
	}

	public MasterballHud()
	{
		Current = this;
	}

	[ConCmd.Client( "bm_hasball", CanBeCalledFromServer = true )]
	public static void NotifyHasBall( int clientid )
	{
		var cl = Client.All.FirstOrDefault( x => x.Id == clientid );
		if ( !cl.IsValid() ) return;
		if ( Current == null ) return;

		Current.Notify( $"{cl.Name} picked up the ball!" );

		//Sound.FromScreen( "ball.pickedup" );
	}

	[ConCmd.Client( "bm_droppedball", CanBeCalledFromServer = true )]
	public static void NotifyDroppedBall( int clientid )
	{
		var cl = Client.All.FirstOrDefault( x => x.Id == clientid );
		if ( !cl.IsValid() ) return;
		if ( Current == null ) return;

		Current.Notify( $"{cl.Name} dropped the ball!" );

		//Sound.FromScreen( "ball.dropped" );
	}

	[ConCmd.Client( "bm_ballreset", CanBeCalledFromServer = true )]
	public static void NotifyBallReset()
	{
		if ( Current == null ) return;

		Current.Notify( $"The ball has been reset" );

		//Sound.FromScreen( "ball.reset" );
	}

	public class NotifyLabel : Label
	{

		private TimeUntil TimeUntilDelete;

		public NotifyLabel()
		{
			TimeUntilDelete = 3f;
		}

		public override void Tick()
		{
			base.Tick();

			if( TimeUntilDelete <= 0 )
			{
				Delete();
			}
		}
	}

}
