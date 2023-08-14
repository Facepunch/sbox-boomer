using Boomer;

public static partial class WeaponUtil
{
	[ClientRpc]
	public static void PlayFlybySound( string sound )
	{
		Sound.FromScreen( sound );
	}

	public static float GetDamageFalloff( float distance, float damage, float start, float end )
	{
		if ( end > 0f )
		{
			if ( start > 0f )
			{
				if ( distance < start )
				{
					return damage;
				}

				var falloffRange = end - start;
				var difference = (distance - start);

				return Math.Max( damage - (damage / falloffRange) * difference, 0f );
			}

			return Math.Max( damage - (damage / end) * distance, 0f );
		}

		return damage;
	}

	public static void PlayFlybySounds( Entity attacker, Entity victim, Vector3 start, Vector3 end, float minDistance, float maxDistance, List<string> sounds )
	{
		var sound = Game.Random.FromList( sounds );

		foreach ( var client in Game.Clients )
		{
			var pawn = client.Pawn as BoomerPlayer;

			if ( !pawn.IsValid() || pawn == attacker )
				continue;

			if ( pawn.LifeState != LifeState.Alive )
				continue;

			var sqrDistance = pawn.Position.SqrDistanceToLine( start, end, out var _ );

			if ( sqrDistance >= minDistance * minDistance && sqrDistance <= maxDistance * maxDistance )
			{
				PlayFlybySound( To.Single( client ), sound );
			}
		}
	}

	public static void PlayFlybySounds( Entity attacker, Vector3 start, Vector3 end, float minDistance, float maxDistance, List<string> sounds )
	{
		PlayFlybySounds( attacker, null, start, end, minDistance, maxDistance, sounds );
	}
}
