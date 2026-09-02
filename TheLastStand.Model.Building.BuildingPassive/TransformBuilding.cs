using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class TransformBuilding : BuildingPassiveEffect
{
	public TransformBuildingDefinition TransformBuildingDefinition => base.BuildingPassiveEffectDefinition as TransformBuildingDefinition;

	public TransformBuilding(PassivesModule buildingPassivesModule, TransformBuildingDefinition buildingPassiveDefinition, TransformBuildingController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
