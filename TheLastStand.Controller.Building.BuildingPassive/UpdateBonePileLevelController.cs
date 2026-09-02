using System;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class UpdateBonePileLevelController : BuildingPassiveEffectController
{
	public UpdateBonePileLevel UpdateBonePileLevel => base.BuildingPassiveEffect as UpdateBonePileLevel;

	public UpdateBonePileLevelController(PassivesModule buildingPassivesModule, UpdateBonePileLevelDefinition updateBonePileLevelDefinition)
	{
		base.BuildingPassiveEffect = new UpdateBonePileLevel(buildingPassivesModule, updateBonePileLevelDefinition, this);
	}

	public override void Apply()
	{
		int level = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule.Level;
		if (BonePileDatabase.BonePileGeneratorsDefinition.BonePileEvolutionDefinitions.TryGetValue(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.BonePilesEvolutionId, out var value))
		{
			foreach (Tuple<int, int> item in value)
			{
				if (TPSingleton<GameManager>.Instance.Game.DayNumber >= item.Item1)
				{
					level = item.Item2;
					continue;
				}
				break;
			}
		}
		base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.ProductionModule.Level = level;
	}
}
