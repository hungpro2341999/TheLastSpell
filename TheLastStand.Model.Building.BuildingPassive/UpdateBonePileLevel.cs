using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class UpdateBonePileLevel : BuildingPassiveEffect
{
	public UpdateBonePileLevelDefinition UpdateBonePileLevelDefinition => base.BuildingPassiveEffectDefinition as UpdateBonePileLevelDefinition;

	public UpdateBonePileLevel(PassivesModule buildingPassivesModule, UpdateBonePileLevelDefinition buildingPassiveDefinition, UpdateBonePileLevelController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
