using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class ImproveGuessWhoController : BuildingUpgradeEffectController
{
	public ImproveGuessWho ImproveGuessWho => base.BuildingUpgradeEffect as ImproveGuessWho;

	public ImproveGuessWhoController(ImproveGuessWhoDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveGuessWho(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		TPSingleton<SpawnWaveManager>.Instance.SetDisplayAllEnemyTiers(state: true);
		TPSingleton<SpawnWaveManager>.Instance.SetDisplayQuantities(state: true);
	}
}
