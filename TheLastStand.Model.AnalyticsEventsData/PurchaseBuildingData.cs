using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class PurchaseBuildingData
{
	[JsonProperty("map_day")]
	public MapDayData MapDayData;

	[JsonProperty("building_type")]
	public string BuildingType;

	[JsonProperty("building_name")]
	public string BuildingName;

	[JsonProperty("upgrade_name")]
	public string UpgradeName;

	public PurchaseBuildingData(MapDayData mapDayData, string buildingType, string buildingName, string upgradeName)
	{
		MapDayData = mapDayData;
		BuildingType = buildingType;
		BuildingName = buildingName;
		UpgradeName = upgradeName;
	}
}
