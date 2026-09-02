using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class ImproveSellRatio : BuildingUpgradeEffect
{
	public ImproveSellRatioController ImproveSellRatioController => base.BuildingUpgradeEffectController as ImproveSellRatioController;

	public ImproveSellRatioDefinition ImproveSellRatioDefinition => base.BuildingUpgradeEffectDefinition as ImproveSellRatioDefinition;

	public ImproveSellRatio(ImproveSellRatioDefinition definition, ImproveSellRatioController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}
