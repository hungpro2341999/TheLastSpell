using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingAction;

public class UpgradeStatBuildingActionEffect : BuildingActionEffect
{
	public UpgradeStatBuildingActionEffectController UpgradeStatBuildingActionController => base.BuildingActionEffectController as UpgradeStatBuildingActionEffectController;

	public UpgradeStatBuildingActionEffectDefinition UpgradeStatBuildingActionDefinition => base.BuildingActionEffectDefinition as UpgradeStatBuildingActionEffectDefinition;

	public UpgradeStatBuildingActionEffect(UpgradeStatBuildingActionEffectDefinition definition, UpgradeStatBuildingActionEffectController controller, ProductionModule productionBuilding)
		: base(definition, controller, productionBuilding)
	{
	}
}
