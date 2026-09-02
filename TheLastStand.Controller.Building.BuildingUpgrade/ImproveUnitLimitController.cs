using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class ImproveUnitLimitController : BuildingUpgradeEffectController
{
	public ImproveUnitLimitController(ImproveUnitLimitDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveUnitLimit(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.UnitLimitBonus++;
	}
}
