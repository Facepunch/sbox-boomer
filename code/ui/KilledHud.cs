using Boomer;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

public partial class KilledHud : Panel
{
	public static KilledHud Current;
	public Label AttackerName;
	public Label AttackerHealth;
	public Label AttackerArmour;
	public Panel AttackerAvatar;
	
	public Image HealthIcon;
	public Image ArmourIcon;
	public KilledHud()
	{
		Current = this;

		AttackerName = Add.Label( "100", "attackername" );
		AttackerAvatar = Add.Panel( "attackeravatar" );

		AttackerHealth = Add.Label( "100", "attackerhealth" );
		HealthIcon = Add.Image( "ui/vitals/healthicon.png", "attackerhealthicon" );
		ArmourIcon = Add.Image( "ui/vitals/armour.png", "attackerarmouricon" );
		AttackerArmour = Add.Label( "0", "attackerarmour" );

	}

	public override void Tick()
	{
		base.Tick();
	}

	public void AddEntry( BoomerPlayer attacker)
	{
		
	}

}


