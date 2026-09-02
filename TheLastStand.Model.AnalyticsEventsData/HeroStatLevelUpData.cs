using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class HeroStatLevelUpData
{
	[JsonProperty("map_day")]
	public MapDayData MapDayData;

	[JsonProperty("hero_id")]
	public string HeroId;

	[JsonProperty("stat_name")]
	public string StatName;

	[JsonProperty("stat_level_up_rarity")]
	public string StatLevelUpRarity;

	[JsonProperty("is_main_stat")]
	public bool IsMainStat;

	public HeroStatLevelUpData(MapDayData mapDayData, string heroId, string statName, string statLevelUpRarity, bool isMainStat)
	{
		MapDayData = mapDayData;
		HeroId = heroId;
		StatName = statName;
		StatLevelUpRarity = statLevelUpRarity;
		IsMainStat = isMainStat;
	}
}
