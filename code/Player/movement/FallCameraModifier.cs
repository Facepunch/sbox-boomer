﻿
using Sandbox;
using System;
using Sandbox.Utility;

namespace Boomer.Movement
{
	class FallCameraModifier : BaseCameraModifier
	{

		private float fallSpeed;
		private float pos = 0;
		private float length;
		private float t;

		private const float effectMaxSpeed = 0f;
		private const float effectStrength = 0f;

		public FallCameraModifier( float fallSpeed, float length = .5f )
		{
			this.length = length;
			this.fallSpeed = fallSpeed * .5f;
		}

		public override bool Update()
		{
			var delta = t.LerpInverse( 0, length, true );
			delta = Easing.EaseOut( delta );
			var invdelta = 1 - delta;

			pos += Time.Delta * invdelta;

			var a = Math.Min( Math.Abs(fallSpeed) / effectMaxSpeed, 1f );
			if ( fallSpeed < 0f ) a *= -1f;

			Camera.Rotation *= Rotation.FromAxis( Vector3.Left, effectStrength * invdelta * pos * a );

			t += Time.Delta;

			return t < length;
		}

	}
}
