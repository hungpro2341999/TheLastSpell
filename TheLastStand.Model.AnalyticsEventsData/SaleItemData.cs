using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class SaleItemData
{
	[JsonProperty("map_day")]
	public MapDayData MapDayData;

	[JsonProperty("item_name")]
	public string ItemName;

	[JsonProperty("gold_win")]
	public int GoldWin;

	public SaleItemData(MapDayData mapDayData, string itemName, int goldWin)
	{
		MapDayData = mapDayData;
		ItemName = itemName;
		GoldWin = goldWin;
	}
}
