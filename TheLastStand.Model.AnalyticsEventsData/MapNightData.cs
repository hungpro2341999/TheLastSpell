using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class MapNightData : MapDayData
{
	[JsonProperty("wave_name")]
	public string WaveName;

	[JsonProperty("turn_number")]
	public int TurnNumber;

	public MapNightData(string runId, string mapName, int apocalypseLevel, int dayNumber, string waveName, int turnNumber)
		: base(runId, mapName, apocalypseLevel, dayNumber)
	{
		WaveName = waveName;
		TurnNumber = turnNumber;
	}
}
