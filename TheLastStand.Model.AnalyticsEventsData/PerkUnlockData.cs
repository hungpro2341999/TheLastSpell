using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class PerkUnlockData
{
	[JsonProperty("map_day")]
	public MapDayData MapDayData;

	[JsonProperty("hero_id")]
	public string HeroId;

	[JsonProperty("perk_name")]
	public string PerkName;

	public PerkUnlockData(MapDayData mapDayData, string heroId, string perkName)
	{
		MapDayData = mapDayData;
		HeroId = heroId;
		PerkName = perkName;
	}
}
