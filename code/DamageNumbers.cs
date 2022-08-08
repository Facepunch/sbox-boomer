
namespace Boomer;

internal static class DamageNumbers
{

	public static void Create( Vector3 pos, float amount, bool armor = false )
	{
		var path = armor 
			? "particles/gameplay/damagenumber/armour_dmg_number.vpcf" 
			: "particles/gameplay/damagenumber/dmg_number.vpcf";
		var number = amount;
		var particle = Particles.Create( path, pos );

		if ( amount < 10 )
		{
			particle.SetPositionComponent( 21, 0, number % 10 );
		}
		else if ( amount < 100 )
		{
			particle.SetPositionComponent( 21, 1, number % 10 );
			particle.SetPositionComponent( 22, 1, 1 );

			number /= 10;
			particle.SetPositionComponent( 21, 0, number % 10 );
		}
		else
		{
			particle.SetPositionComponent( 21, 2, number % 1 );
			particle.SetPositionComponent( 22, 2, 1 );

			number /= 10;
			particle.SetPositionComponent( 21, 1, number % 10 );
			particle.SetPositionComponent( 22, 1, 1 );

			number /= 10;

			particle.SetPositionComponent( 21, 0, number % 100 );
			particle.SetPositionComponent( 22, 0, 1 );
		}
	}

}
