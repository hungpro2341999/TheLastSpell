using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.AnalyticsEventsData;

public class EndNightData
{
	[Serializable]
	public abstract class ASkillUsageData
	{
		[JsonProperty("skill_name")]
		public readonly string SkillName;

		[JsonProperty("skill_count")]
		public readonly int SkillUsageCount;

		[JsonProperty("skill_damage")]
		public readonly int TotalDamage;

		[JsonProperty("kill_count")]
		public readonly int KillCount;

		public ASkillUsageData(string skillName, GameAnalytics.SkillAnalyticsValue value)
		{
			SkillName = skillName;
			SkillUsageCount = value.SkillUsageCount;
			TotalDamage = value.TotalDamage;
			KillCount = value.KillCount;
		}
	}

	[Serializable]
	public class WeaponSkillUsageData : ASkillUsageData
	{
		[JsonProperty("hero_name")]
		public readonly string PlayableUnitId;

		[JsonProperty("weapon_name")]
		public readonly string WeaponName;

		[JsonProperty("weapon_level")]
		public readonly int WeaponLevel;

		public WeaponSkillUsageData(GameAnalytics.WeaponSkillAnalyticsKey key, GameAnalytics.SkillAnalyticsValue value)
			: base(key.SkillName, value)
		{
			PlayableUnitId = key.PlayableUnitId;
			WeaponName = key.WeaponName;
			WeaponLevel = key.WeaponLevel;
		}
	}

	[Serializable]
	public class BuildingSkillUsageData : ASkillUsageData
	{
		[JsonProperty("building_name")]
		public readonly string BuildingId;

		public BuildingSkillUsageData(GameAnalytics.BuildingSkillAnalyticsKey key, GameAnalytics.SkillAnalyticsValue value)
			: base(key.SkillName, value)
		{
			BuildingId = key.BuildingId;
		}
	}

	[Serializable]
	public class SimplePlayableUnitData
	{
		[JsonProperty("hero_name")]
		public readonly string PlayableUnitId;

		[JsonProperty("hero_level")]
		public readonly int PlayableUnitLevel;

		public SimplePlayableUnitData(PlayableUnit playableUnit)
		{
			PlayableUnitId = playableUnit.AnalyticsIdentifier;
			PlayableUnitLevel = (int)playableUnit.Level;
		}
	}

	[JsonProperty("map_night")]
	public readonly MapNightData MapNightData;

	[JsonProperty("gold_win")]
	public readonly int GoldWin;

	[JsonProperty("materials_win")]
	public readonly int MaterialsWin;

	[JsonProperty("night_score")]
	public readonly int NightScore;

	[JsonProperty("use_seer")]
	public readonly bool RepelledFog;

	[JsonProperty("skill")]
	public readonly List<WeaponSkillUsageData> SkillAnalytics = new List<WeaponSkillUsageData>();

	[JsonProperty("building_skill")]
	public readonly List<BuildingSkillUsageData> BuildingSkillAnalytics = new List<BuildingSkillUsageData>();

	[JsonProperty("hero")]
	public readonly List<SimplePlayableUnitData> PlayableUnitAnalytics = new List<SimplePlayableUnitData>();

	[JsonProperty("bonepile_create_count")]
	public readonly int GeneratedBonePileCount;

	public EndNightData(MapNightData mapNightData, int goldWin, int materialsWin, int nightScore, bool repelledFog, GameAnalytics gameAnalytics, List<PlayableUnit> playableUnits, int generatedBonePileCount)
	{
		MapNightData = mapNightData;
		GoldWin = goldWin;
		MaterialsWin = materialsWin;
		NightScore = nightScore;
		RepelledFog = repelledFog;
		foreach (KeyValuePair<GameAnalytics.WeaponSkillAnalyticsKey, GameAnalytics.SkillAnalyticsValue> item in gameAnalytics.SkillDataAnalyticsPerPlayable)
		{
			SkillAnalytics.Add(new WeaponSkillUsageData(item.Key, item.Value));
		}
		foreach (KeyValuePair<GameAnalytics.BuildingSkillAnalyticsKey, GameAnalytics.SkillAnalyticsValue> item2 in gameAnalytics.SkillDataAnalyticsPerBuilding)
		{
			BuildingSkillAnalytics.Add(new BuildingSkillUsageData(item2.Key, item2.Value));
		}
		foreach (PlayableUnit playableUnit in playableUnits)
		{
			if (!playableUnit.IsDead)
			{
				PlayableUnitAnalytics.Add(new SimplePlayableUnitData(playableUnit));
			}
		}
		GeneratedBonePileCount = generatedBonePileCount;
	}
}
