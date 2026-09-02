using System.Collections.Generic;

namespace TheLastStand.Serialization;

public class SerializedGameAnalytics
{
	public struct SerializedPlayableUnitSkillData
	{
		public GameAnalytics.WeaponSkillAnalyticsKey Key;

		public GameAnalytics.SkillAnalyticsValue Value;
	}

	public struct SerializedBuildingSkillData
	{
		public GameAnalytics.BuildingSkillAnalyticsKey Key;

		public GameAnalytics.SkillAnalyticsValue Value;
	}

	public struct SerializedBuildingBuiltData
	{
		public string Key;

		public GameAnalytics.BuildingBuiltData Value;
	}

	public List<SerializedPlayableUnitSkillData> PlayableUnitSkillData = new List<SerializedPlayableUnitSkillData>();

	public List<SerializedBuildingSkillData> BuildingSkillData = new List<SerializedBuildingSkillData>();

	public List<SerializedBuildingBuiltData> BuildingBuiltData = new List<SerializedBuildingBuiltData>();

	public int ScavengedBonePilesGold;

	public int ScavengedBonePilesMaterials;
}
