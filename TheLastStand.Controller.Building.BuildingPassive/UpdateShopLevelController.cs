using System;
using TPLib;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class UpdateShopLevelController : BuildingPassiveEffectController
{
	public UpdateShopLevel UpdateShopLevel => base.BuildingPassiveEffect as UpdateShopLevel;

	public UpdateShopLevelController(PassivesModule buildingPassivesModule, UpdateShopLevelDefinition updateShopLevelDefinition)
	{
		base.BuildingPassiveEffect = new UpdateShopLevel(buildingPassivesModule, updateShopLevelDefinition, this);
	}

	public override void Apply()
	{
		int level = 1;
		if (BuildingDatabase.ShopDefinition.ShopEvolutionDefinitions.TryGetValue(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.ShopEvolutionId, out var value))
		{
			foreach (Tuple<int, int> item in value.LevelsPerDay)
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
