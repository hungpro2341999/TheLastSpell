using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class BossDeathData
{
	[JsonProperty("map_night")]
	public MapNightData MapNightData;

	[JsonProperty("boss_name")]
	public string BossName;

	public BossDeathData(MapNightData mapNightData, string bossName)
	{
		MapNightData = mapNightData;
		BossName = bossName;
	}
}
