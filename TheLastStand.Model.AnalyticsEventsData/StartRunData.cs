using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class StartRunData
{
	[JsonProperty("map")]
	public MapData MapData;

	[JsonProperty("omen_use_name")]
	public string[] UsedOmens;

	[JsonProperty("weapon_pool")]
	public string[] ActiveWeaponsFamilies;

	[JsonProperty("weapon_excluded")]
	public string[] ExcludedWeaponsFamilies;

	[JsonProperty("dark_meta_unlocked_number")]
	public int DarkMetaUnlockedNb;

	[JsonProperty("light_meta_unlocked_number")]
	public int LightMetaUnlockedNb;

	public StartRunData(MapData mapData, string[] usedOmens, string[] activeWeaponsFamilies, string[] excludedWeaponsFamilies, int darkMetaUnlockedNb, int lightMetaUnlockedNb)
	{
		MapData = mapData;
		UsedOmens = usedOmens;
		ActiveWeaponsFamilies = activeWeaponsFamilies;
		ExcludedWeaponsFamilies = excludedWeaponsFamilies;
		DarkMetaUnlockedNb = darkMetaUnlockedNb;
		LightMetaUnlockedNb = lightMetaUnlockedNb;
	}
}
