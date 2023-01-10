using Sandbox;
using Sandbox.Utility;

namespace Facepunch.Boomer.CameraModifiers;

public class Fov : CameraModifier
{
	float Offset = 15f;
	float Time = 1f;

	TimeSince lifeTime = 0;
	float pos = 0;

	public Fov( float offset, float time = 1f )
	{
		Time = time;
		Offset = offset;
	}

	public override bool Update()
	{
		var delta = ((float)lifeTime).LerpInverse( 0, Time, true );
		delta = Easing.EaseOut( delta );
		var invdelta = 1 - delta;

		Camera.FieldOfView += invdelta * Offset;

		return lifeTime < Time;
	}
}
