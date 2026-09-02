using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class IncreaseWorkers : BuildingPassiveEffect
{
	public IncreaseWorkersDefinition IncreaseWorkersDefinition => base.BuildingPassiveEffectDefinition as IncreaseWorkersDefinition;

	public int UpgradedBonusValue { get; set; }

	public IncreaseWorkers(PassivesModule buildingPassivesModule, IncreaseWorkersDefinition buildingPassiveDefinition, IncreaseWorkersController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
