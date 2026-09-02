using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class IncreasePlaysPerTurn : BuildingUpgradeEffect
{
	public IncreasePlaysPerTurnController IncreasePlaysPerTurnController => base.BuildingUpgradeEffectController as IncreasePlaysPerTurnController;

	public IncreasePlaysPerTurnDefinition IncreasePlaysPerTurnDefinition => base.BuildingUpgradeEffectDefinition as IncreasePlaysPerTurnDefinition;

	public IncreasePlaysPerTurn(IncreasePlaysPerTurnDefinition definition, IncreasePlaysPerTurnController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}
