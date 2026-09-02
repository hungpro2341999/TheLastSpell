using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class MapData
{
	[JsonProperty("run_id")]
	public string RunId;

	[JsonProperty("map_name")]
	public string MapName;

	[JsonProperty("apocalypse_level")]
	public int ApocalypseLevel;

	public MapData(string runId, string mapName, int apocalypseLevel)
	{
		RunId = runId;
		MapName = mapName;
		ApocalypseLevel = apocalypseLevel;
	}
}
