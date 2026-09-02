using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class UpdateShopLevel : BuildingPassiveEffect
{
	public UpdateShopLevelDefinition UpdateShopLevelDefinition => base.BuildingPassiveEffectDefinition as UpdateShopLevelDefinition;

	public UpdateShopLevel(PassivesModule buildingPassivesModule, UpdateShopLevelDefinition buildingPassiveDefinition, UpdateShopLevelController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
