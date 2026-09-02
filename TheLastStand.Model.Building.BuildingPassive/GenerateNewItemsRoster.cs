using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class GenerateNewItemsRoster : BuildingPassiveEffect
{
	public GenerateNewItemsRosterDefinition GenerateNewItemsRosterDefinition => base.BuildingPassiveEffectDefinition as GenerateNewItemsRosterDefinition;

	public GenerateNewItemsRoster(PassivesModule buildingPassivesModule, GenerateNewItemsRosterDefinition buildingPassiveDefinition, GenerateNewItemsRosterController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}
