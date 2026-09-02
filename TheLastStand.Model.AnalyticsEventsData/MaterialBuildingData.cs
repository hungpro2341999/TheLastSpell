using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class MaterialBuildingData
{
	[JsonProperty("map_day")]
	public MapDayData MapDayData;

	[JsonProperty("material_win")]
	public int MaterialWin;

	public MaterialBuildingData(MapDayData mapDayData, int materialWin)
	{
		MapDayData = mapDayData;
		MaterialWin = materialWin;
	}
}
