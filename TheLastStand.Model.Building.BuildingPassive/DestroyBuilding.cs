using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class DestroyBuilding : BuildingPassiveEffect
{
	public DestroyBuildingDefinition TransformBuildingDefinition => base.BuildingPassiveEffectDefinition as DestroyBuildingDefinition;

	public DestroyBuilding(PassivesModule buildingPassivesModule, DestroyBuildingDefinition buildingPassiveDefinition, DestroyBuildingController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
