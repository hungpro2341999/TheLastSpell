using TPLib;
using TheLastStand.Framework;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using UnityEngine;

namespace TheLastStand.View.Building.BuildingGaugeEffect;

public abstract class BuildingGaugeEffectView
{
	public static class Consts
	{
		public const string BuildingProductionGaugeBigIconRewardPrefix = "View/Sprites/UI/Buildings/ProductionRewards/Big/WorldUI_Gauges_Prod_Reward_";

		public const string BuildingProductionGaugeSmallIconRewardPrefix = "View/Sprites/UI/Buildings/ProductionRewards/Small/WorldUI_Gauges_Prod_Reward_";
	}

	public TheLastStand.Model.Building.BuildingGaugeEffect.BuildingGaugeEffect BuildingGaugeEffect { get; set; }

	public BuildingGaugeEffectView()
	{
	}

	public abstract string GetEffectRewardString();

	public Sprite GetProductionRewardIconSpriteBig()
	{
		string productionRewardIconId = GetProductionRewardIconId();
		string resourcePath = ((!(BuildingGaugeEffect.ProductionBuilding.BuildingParent is MagicCircle)) ? ("View/Sprites/UI/Buildings/ProductionRewards/Big/WorldUI_Gauges_Prod_Reward_" + productionRewardIconId) : ("View/Sprites/UI/Buildings/ProductionRewards/Big/WorldUI_Gauges_Prod_Reward_" + productionRewardIconId + "_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id));
		Sprite sprite = ResourcePooler.LoadOnce<Sprite>(resourcePath);
		if (!(sprite != null))
		{
			return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Buildings/ProductionRewards/Big/WorldUI_Gauges_Prod_Reward_" + productionRewardIconId);
		}
		return sprite;
	}

	public Sprite GetProductionRewardIconSpriteSmall()
	{
		string productionRewardIconId = GetProductionRewardIconId();
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Buildings/ProductionRewards/Small/WorldUI_Gauges_Prod_Reward_" + productionRewardIconId);
	}

	protected abstract string GetProductionRewardIconId();
}
