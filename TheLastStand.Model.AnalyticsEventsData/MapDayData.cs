using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class MapDayData : MapData
{
	[JsonProperty("day_number")]
	public int DayNumber;

	public MapDayData(string runId, string mapName, int apocalypseLevel, int dayNumber)
		: base(runId, mapName, apocalypseLevel)
	{
		DayNumber = dayNumber;
	}
}
