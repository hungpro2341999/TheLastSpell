using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class GoldBuildingData
{
	[JsonProperty("map_day")]
	public MapDayData MapDayData;

	[JsonProperty("gold_win")]
	public int GoldWin;

	public GoldBuildingData(MapDayData mapDayData, int goldWin)
	{
		MapDayData = mapDayData;
		GoldWin = goldWin;
	}
}
