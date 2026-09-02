using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class GainResources : BuildingPassiveEffect
{
	public GainResourcesDefinition GainResourcesDefinition => base.BuildingPassiveEffectDefinition as GainResourcesDefinition;

	public GainResources(PassivesModule buildingPassivesModule, GainResourcesDefinition gainResourcesDefinition, GainResourcesController gainResourcesController)
		: base(buildingPassivesModule, gainResourcesDefinition, gainResourcesController)
	{
	}
}
