using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class ImproveSellRatioController : BuildingUpgradeEffectController
{
	public ImproveSellRatio ImproveSellRatio => base.BuildingUpgradeEffect as ImproveSellRatio;

	public ImproveSellRatioController(ImproveSellRatioDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveSellRatio(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		TPSingleton<BuildingManager>.Instance.Shop.SellRatioLevel += ImproveSellRatio.ImproveSellRatioDefinition.Value;
	}
}
