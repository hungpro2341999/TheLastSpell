using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class AchievementData
{
	[JsonProperty("achievement_name")]
	public string AchievementName;

	public AchievementData(string achievementName)
	{
		AchievementName = achievementName;
	}
}
