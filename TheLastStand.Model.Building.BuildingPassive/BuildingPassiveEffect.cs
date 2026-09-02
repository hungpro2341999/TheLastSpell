using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public abstract class BuildingPassiveEffect
{
	public PassivesModule BuildingPassivesModule { get; private set; }

	public BuildingPassiveEffectController BuildingPassiveEffectController { get; private set; }

	public BuildingPassiveEffectDefinition BuildingPassiveEffectDefinition { get; private set; }

	public BuildingPassiveEffect(PassivesModule buildingPassivesModule, BuildingPassiveEffectDefinition buildingPassiveDefinition, BuildingPassiveEffectController buildingPassiveController)
	{
		BuildingPassivesModule = buildingPassivesModule;
		BuildingPassiveEffectDefinition = buildingPassiveDefinition;
		BuildingPassiveEffectController = buildingPassiveController;
	}
}
