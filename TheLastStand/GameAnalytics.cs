using System;
using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.TileMap;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization;

namespace TheLastStand;

public class GameAnalytics
{
	[Serializable]
	public struct WeaponSkillAnalyticsKey
	{
		public string PlayableUnitId;

		public string WeaponName;

		public int WeaponLevel;

		public string SkillName;

		public WeaponSkillAnalyticsKey(PlayableUnit playableUnit, Item itemContainer, Skill skill)
		{
			PlayableUnitId = playableUnit.AnalyticsIdentifier;
			WeaponName = itemContainer.ItemDefinition.Id;
			WeaponLevel = itemContainer.Level;
			SkillName = skill.SkillDefinition.Id;
		}
	}

	[Serializable]
	public struct BuildingSkillAnalyticsKey
	{
		public string BuildingId;

		public string SkillName;

		public BuildingSkillAnalyticsKey(Building building, Skill skill)
		{
			BuildingId = building.BuildingDefinition.Id;
			SkillName = skill.SkillDefinition.Id;
		}
	}

	[Serializable]
	public class SkillAnalyticsValue
	{
		public int SkillUsageCount;

		public int TotalDamage;

		public int KillCount;

		public SkillAnalyticsValue()
		{
		}

		public SkillAnalyticsValue(int totalDamage, int killCount)
		{
			SkillUsageCount = 1;
			TotalDamage = totalDamage;
			KillCount = killCount;
		}

		public void UpdateValues(int totalDamage, int killCount)
		{
			SkillUsageCount++;
			TotalDamage += totalDamage;
			KillCount += killCount;
		}
	}

	[Serializable]
	public class BuildingBuiltData
	{
		public const int BuildingCloseToMagicCircleDistance = 3;

		public int Count;

		public int CloseToMagicCircleCount;

		public void IncreaseCount(bool closeToMagicCircle)
		{
			Count++;
			if (closeToMagicCircle)
			{
				CloseToMagicCircleCount++;
			}
		}
	}

	public readonly Dictionary<WeaponSkillAnalyticsKey, SkillAnalyticsValue> SkillDataAnalyticsPerPlayable = new Dictionary<WeaponSkillAnalyticsKey, SkillAnalyticsValue>();

	public readonly Dictionary<BuildingSkillAnalyticsKey, SkillAnalyticsValue> SkillDataAnalyticsPerBuilding = new Dictionary<BuildingSkillAnalyticsKey, SkillAnalyticsValue>();

	public readonly Dictionary<string, BuildingBuiltData> BuildingBuiltAnalytics = new Dictionary<string, BuildingBuiltData>();

	public int ScavengedBonePilesGold { get; private set; }

	public int ScavengedBonePilesMaterials { get; private set; }

	public GameAnalytics(SerializedGameAnalytics gameAnalyticsData = null)
	{
		if (gameAnalyticsData == null)
		{
			return;
		}
		foreach (SerializedGameAnalytics.SerializedPlayableUnitSkillData playableUnitSkillDatum in gameAnalyticsData.PlayableUnitSkillData)
		{
			SkillDataAnalyticsPerPlayable[playableUnitSkillDatum.Key] = playableUnitSkillDatum.Value;
		}
		foreach (SerializedGameAnalytics.SerializedBuildingSkillData buildingSkillDatum in gameAnalyticsData.BuildingSkillData)
		{
			SkillDataAnalyticsPerBuilding[buildingSkillDatum.Key] = buildingSkillDatum.Value;
		}
		foreach (SerializedGameAnalytics.SerializedBuildingBuiltData buildingBuiltDatum in gameAnalyticsData.BuildingBuiltData)
		{
			BuildingBuiltAnalytics[buildingBuiltDatum.Key] = buildingBuiltDatum.Value;
		}
		ScavengedBonePilesGold = gameAnalyticsData.ScavengedBonePilesGold;
		ScavengedBonePilesMaterials = gameAnalyticsData.ScavengedBonePilesMaterials;
	}

	public void ClearSkillAnalytics()
	{
		SkillDataAnalyticsPerPlayable.Clear();
		SkillDataAnalyticsPerBuilding.Clear();
	}

	public void OnSkillExecutionEnd(SkillActionExecutionController skillActionExecutionController)
	{
		ISkillCaster caster = skillActionExecutionController.SkillActionExecution.Caster;
		Skill skill = skillActionExecutionController.SkillActionExecution.Skill;
		List<List<SkillActionResultDatas>> allResultData = skillActionExecutionController.SkillActionExecution.AllResultData;
		if (!(caster is PlayableUnit playableUnit))
		{
			if (!(caster is BattleModule battleModule))
			{
				return;
			}
			BuildingSkillAnalyticsKey key = new BuildingSkillAnalyticsKey(battleModule.BuildingParent, skill);
			int killCount = allResultData.Sum((List<SkillActionResultDatas> x) => x.Sum((SkillActionResultDatas y) => y.AffectedUnits.Count((Unit u) => u?.IsDead ?? true)));
			int totalDamage = (int)allResultData.Sum((List<SkillActionResultDatas> x) => x.Sum((SkillActionResultDatas y) => y.TotalDamagesToUnits));
			if (!SkillDataAnalyticsPerBuilding.TryGetValue(key, out var value))
			{
				SkillDataAnalyticsPerBuilding[key] = new SkillAnalyticsValue(totalDamage, killCount);
			}
			else
			{
				value.UpdateValues(totalDamage, killCount);
			}
		}
		else
		{
			if (!(skill.SkillContainer is Item itemContainer))
			{
				return;
			}
			WeaponSkillAnalyticsKey key2 = new WeaponSkillAnalyticsKey(playableUnit, itemContainer, skill);
			int killCount2 = allResultData.Sum((List<SkillActionResultDatas> x) => x.Sum((SkillActionResultDatas y) => y.AffectedUnits.Count((Unit u) => u?.IsDead ?? true)));
			int totalDamage2 = (int)allResultData.Sum((List<SkillActionResultDatas> x) => x.Sum((SkillActionResultDatas y) => y.TotalDamagesToUnits));
			if (!SkillDataAnalyticsPerPlayable.TryGetValue(key2, out var value2))
			{
				SkillDataAnalyticsPerPlayable[key2] = new SkillAnalyticsValue(totalDamage2, killCount2);
			}
			else
			{
				value2.UpdateValues(totalDamage2, killCount2);
			}
		}
	}

	public void ClearBuildingBuiltAnalytics()
	{
		BuildingBuiltAnalytics.Clear();
	}

	public void OnBuildingBuilt(Building building)
	{
		string id = building.BuildingDefinition.Id;
		bool closeToMagicCircle = building.OccupiedTiles.Any((Tile buildingTile) => BuildingManager.MagicCircle.OccupiedTiles.Any((Tile tile) => TileMapController.DistanceBetweenTiles(tile, buildingTile) < 3));
		if (!BuildingBuiltAnalytics.ContainsKey(id))
		{
			BuildingBuiltAnalytics[id] = new BuildingBuiltData();
		}
		BuildingBuiltAnalytics[id].IncreaseCount(closeToMagicCircle);
	}

	public void OnBonePileScavenged(int goldGain, int materialGain)
	{
		ScavengedBonePilesGold += goldGain;
		ScavengedBonePilesMaterials += materialGain;
	}

	public SerializedGameAnalytics Serialize()
	{
		SerializedGameAnalytics serializedGameAnalytics = new SerializedGameAnalytics();
		foreach (KeyValuePair<WeaponSkillAnalyticsKey, SkillAnalyticsValue> item in SkillDataAnalyticsPerPlayable)
		{
			serializedGameAnalytics.PlayableUnitSkillData.Add(new SerializedGameAnalytics.SerializedPlayableUnitSkillData
			{
				Key = item.Key,
				Value = item.Value
			});
		}
		foreach (KeyValuePair<BuildingSkillAnalyticsKey, SkillAnalyticsValue> item2 in SkillDataAnalyticsPerBuilding)
		{
			serializedGameAnalytics.BuildingSkillData.Add(new SerializedGameAnalytics.SerializedBuildingSkillData
			{
				Key = item2.Key,
				Value = item2.Value
			});
		}
		foreach (KeyValuePair<string, BuildingBuiltData> buildingBuiltAnalytic in BuildingBuiltAnalytics)
		{
			serializedGameAnalytics.BuildingBuiltData.Add(new SerializedGameAnalytics.SerializedBuildingBuiltData
			{
				Key = buildingBuiltAnalytic.Key,
				Value = buildingBuiltAnalytic.Value
			});
		}
		serializedGameAnalytics.ScavengedBonePilesGold = ScavengedBonePilesGold;
		serializedGameAnalytics.ScavengedBonePilesGold = ScavengedBonePilesMaterials;
		return serializedGameAnalytics;
	}
}
