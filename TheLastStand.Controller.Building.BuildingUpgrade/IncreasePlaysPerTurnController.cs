using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class IncreasePlaysPerTurnController : BuildingUpgradeEffectController
{
	public IncreasePlaysPerTurn IncreasePlaysPerTurn => base.BuildingUpgradeEffect as IncreasePlaysPerTurn;

	public IncreasePlaysPerTurnController(IncreasePlaysPerTurnDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new IncreasePlaysPerTurn(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		TheLastStand.Model.Building.Building building = base.BuildingUpgradeEffect.BuildingUpgrade.Building;
		if (building.BattleModule != null)
		{
			building.BattleModule.NumberOfGoalsToCompute += IncreasePlaysPerTurn.IncreasePlaysPerTurnDefinition.Value;
		}
	}
}
