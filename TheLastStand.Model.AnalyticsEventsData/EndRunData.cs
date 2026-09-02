using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class EndRunData
{
	[JsonProperty("map")]
	public MapData MapData;

	[JsonProperty("wave_name")]
	public string WaveName;

	[JsonProperty("day_number_max")]
	public int DayNumberMax;

	[JsonProperty("turn_number_max")]
	public int TurnNumberMax;

	[JsonProperty("weapon_pool")]
	public string[] ActiveWeaponsFamilies;

	[JsonProperty("status")]
	public string Status;

	[JsonProperty("reason")]
	public string GameOverReason;

	public EndRunData(MapData mapData, string waveName, int dayNumberMax, int turnNumberMax, string[] activeWeaponsFamilies, string status, string gameOverReason)
	{
		MapData = mapData;
		WaveName = waveName;
		DayNumberMax = dayNumberMax;
		TurnNumberMax = turnNumberMax;
		ActiveWeaponsFamilies = activeWeaponsFamilies;
		Status = status;
		GameOverReason = gameOverReason;
	}
}
