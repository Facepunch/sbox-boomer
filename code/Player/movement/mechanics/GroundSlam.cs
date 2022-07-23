
using Sandbox;
using System;

namespace Boomer.Movement
{
	class GroundSlam : BaseMoveMechanic
	{

		public override string HudName => "Ground Slam";
		public override string HudDescription => $"Press {InputActions.Duck.GetButtonOrigin()} while in the air";

		public float SlamGravity => 2250f;

		public override bool TakesOverControl => true;
		public override bool AlwaysSimulate => false;

		private TimeUntil FreezeTimer;

		public GroundSlam( BoomerController controller ) : base( controller )
		{
		}

		protected override bool TryActivate()
		{
			if ( ctrl.GroundEntity.IsValid() ) return false;
			if ( !InputActions.Duck.Pressed() ) return false;

			ctrl.Velocity = 0f;
			FreezeTimer = .25f;

			return true;
		}

		public override void Simulate()
		{
			base.Simulate();

			if ( ctrl.Pawn is not BoomerPlayer pl )
			{
				IsActive = false;
				return;
			}

			if ( ctrl.GroundEntity != null )
			{
				GroundEffect();
				IsActive = false;
				return;
			}

			if ( FreezeTimer > 0 )
			{
				return;
			}
			
			var ents = Entity.FindInSphere( ctrl.Position, 30f );

			if ( BasePlayerController.Debug )
			{
				//DebugOverlay.Sphere( ctrl.Position, 30f, Color.Red, 3f );
			}

			foreach( var ent in ents )
			{
				if ( ent == ctrl.Pawn ) continue;
				var dmgtype = ent is BoomerPlayer ? DamageFlags.Sonic : DamageFlags.Generic;
				var dmgAmount = ent is BoomerPlayer ? 2 : 80;

				ent.TakeDamage( new DamageInfo()
				{
					Attacker = ctrl.Pawn,
					Flags = dmgtype,
					Damage = dmgAmount
				} );
			}

			ctrl.Velocity += ctrl.Velocity.WithZ( -SlamGravity ) * Time.Delta;
			ctrl.Move();
		}

		public void Cancel()
		{
			IsActive = false;
		}

		private void GroundEffect()
		{
			ctrl.AddEvent( "sitting" );

			if ( !ctrl.Pawn.IsServer ) return;

			using var _ = Prediction.Off();

			Sound.FromWorld( "player.slam.land", ctrl.Position );

			var effectRadius = 100f;
			var overlaps = Entity.FindInSphere( ctrl.Position, effectRadius );

			foreach( var overlap in overlaps )
			{
				if ( overlap is not ModelEntity ent || !ent.IsValid() )
					continue;

				if ( ent.LifeState != LifeState.Alive )
					continue;

				if ( !ent.PhysicsBody.IsValid() )
					continue;

				if ( ent.IsWorld )
					continue;

				if ( ent is BoomerPlayer )
					continue;

				var targetPos = ent.PhysicsBody.MassCenter;

				var dist = Vector3.DistanceBetween( ctrl.Position, targetPos );
				if ( dist > effectRadius )
					continue;

				var forceMult = ent is PropGib ? 60f : 6f;
				var distanceMul = 1.0f - Math.Clamp( dist / effectRadius, 0.0f, 1.0f );
				var force = (forceMult * distanceMul) * ent.PhysicsBody.Mass;
				var forceDir = (targetPos - ctrl.Position).Normal;

				ent.ApplyAbsoluteImpulse( forceDir * force );
			}
		}

	}
}
