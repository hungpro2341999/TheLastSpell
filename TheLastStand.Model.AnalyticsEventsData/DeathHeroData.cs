using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class DeathHeroData
{
	[JsonProperty("map_night")]
	public MapNightData MapNightData;

	[JsonProperty("wave_id")]
	public string WaveId;

	[JsonProperty("hero_id")]
	public string HeroId;

	[JsonProperty("skill_name")]
	public string SkillName;

	[JsonProperty("unit_name")]
	public string KillerName;

	public DeathHeroData(MapNightData mapNightData, string heroId, string skillName, string killerName)
	{
		MapNightData = mapNightData;
		HeroId = heroId;
		SkillName = skillName;
		KillerName = killerName;
	}
}
