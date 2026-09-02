using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.AnalyticsEventsData;

public class EndDayData
{
	[Serializable]
	public class BuildingBuiltData
	{
		[JsonProperty("building_name")]
		public readonly string Name;

		[JsonProperty("building_count")]
		public readonly int Count;

		[JsonProperty("is_less_3_slots_count")]
		public readonly int CloseToMagicCircleCount;

		public BuildingBuiltData(string name, GameAnalytics.BuildingBuiltData data)
		{
			Name = name;
			Count = data.Count;
			CloseToMagicCircleCount = data.CloseToMagicCircleCount;
		}
	}

	[JsonProperty("map_day")]
	public readonly MapDayData MapDayData;

	[JsonProperty("building")]
	public readonly List<BuildingBuiltData> BuildingBuiltAnalytics = new List<BuildingBuiltData>();

	[JsonProperty("hero")]
	public readonly PlayableUnitsData PlayableUnitsData;

	[JsonProperty("bonepile_gold_win")]
	public readonly int BonePileGoldWin;

	[JsonProperty("bonepile_material_win")]
	public readonly int BonePileMaterialWin;

	public EndDayData(MapDayData mapDayData, GameAnalytics gameAnalytics, List<PlayableUnit> playableUnits)
	{
		MapDayData = mapDayData;
		foreach (KeyValuePair<string, GameAnalytics.BuildingBuiltData> buildingBuiltAnalytic in gameAnalytics.BuildingBuiltAnalytics)
		{
			BuildingBuiltAnalytics.Add(new BuildingBuiltData(buildingBuiltAnalytic.Key, buildingBuiltAnalytic.Value));
		}
		PlayableUnitsData = new PlayableUnitsData(playableUnits);
		BonePileGoldWin = gameAnalytics.ScavengedBonePilesGold;
		BonePileMaterialWin = gameAnalytics.ScavengedBonePilesMaterials;
	}
}
