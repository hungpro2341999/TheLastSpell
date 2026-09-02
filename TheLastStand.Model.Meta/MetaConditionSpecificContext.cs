using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Fog;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Fog;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Model.Meta;

[Serializable]
public class MetaConditionSpecificContext : MetaConditionContext
{
	public enum E_ValueCategory
	{
		None,
		DebuffsApplied,
		GoldObtained,
		GoldSpentInShop,
		GoldSpent,
		PropagationBounces,
		MaterialsObtained,
		MaterialsSpent,
		MaxHeroLevelReached,
		MaxActionPointsOnHeroSingleTurn,
		MaxTilesCrossedSingleTurn,
		MomentumOneShots,
		PotionsUsed,
		PotionsOnAllies,
		PunchKills,
		SoldItems,
		WorkersSpent,
		ScavengeWorkers,
		MaxScavengeWorkersSingleProd,
		BoomerDamage,
		BestBlow,
		CriticalHits,
		DamageBlocked,
		DamageInflicted,
		DamageMitigatedByResistance,
		DamageTakenOnArmor,
		Dodges,
		HealthLost,
		JumpsOverWallUsed,
		Kills,
		ManaSpent,
		MostUnitsKilledInOneBlow,
		PunchesUsed,
		StunnedEnemies,
		PoisonedEnemies,
		AlterationKills,
		TilesCrossed
	}

	[Serializable]
	public struct DoubleValue
	{
		[XmlAttribute]
		public E_ValueCategory Category;

		[XmlAttribute]
		public double Value;
	}

	[Serializable]
	public struct BuiltBuildingInfos
	{
		[XmlIgnore]
		public string Category;

		[XmlAttribute]
		public string Id;

		[XmlAttribute]
		public int Count;
	}

	[Serializable]
	public struct EnemyKills
	{
		[XmlAttribute]
		public string Id;

		[XmlAttribute]
		public int Count;

		[XmlAttribute]
		public int FromPoisonCount;

		[XmlAttribute]
		public int IsolatedCount;
	}

	[Serializable]
	public struct CompletedRun
	{
		[XmlAttribute]
		public string CityId;

		[XmlAttribute]
		public bool IsVictorious;

		[XmlAttribute]
		public int ApocalypseIndex;
	}

	[Serializable]
	public struct HighestAttackDamagesByType
	{
		[XmlAttribute]
		public string DamagesType;

		[XmlAttribute]
		public double Value;
	}

	[Serializable]
	public struct ProducedItem
	{
		[XmlIgnore]
		public ItemDefinition.E_Category Category;

		[XmlAttribute]
		public string Id;

		[XmlAttribute]
		public string SourceBuildingId;
	}

	[Serializable]
	public struct BoughtItem
	{
		[XmlIgnore]
		public ItemDefinition.E_Category Category;

		[XmlAttribute]
		public string Id;

		[XmlAttribute]
		public int Cost;
	}

	[Serializable]
	public struct StatValueReached
	{
		[XmlAttribute]
		public string Stat;

		[XmlAttribute]
		public double Value;
	}

	[Serializable]
	public struct DamageInflictedByEnemyType
	{
		[XmlAttribute]
		public string EnemyId;

		[XmlAttribute]
		public double Value;
	}

	[Serializable]
	public struct DamageTakenByEnemyType
	{
		[XmlAttribute]
		public string EnemyId;

		[XmlAttribute]
		public double Value;
	}

	[Serializable]
	public struct UsesPerWeapon
	{
		[XmlAttribute]
		public string WeaponId;

		[XmlAttribute]
		public double ActionPointsSpent;

		[XmlAttribute]
		public double Value;
	}

	[Serializable]
	public struct LowestPanicLevel
	{
		[XmlAttribute]
		public string CityId;

		[XmlAttribute]
		public double PanicLevel;

		[XmlAttribute]
		public int ApocalypseIndex;
	}

	[Serializable]
	public struct NightReached
	{
		[XmlAttribute]
		public string CityId;

		[XmlAttribute]
		public double Night;

		[XmlAttribute]
		public double FogDensityIndex;

		[XmlAttribute]
		public string BuildingsString;

		[XmlIgnore]
		public Dictionary<BuildingDefinition, int> Buildings;

		public void GenerateBuildingFromString()
		{
			Buildings = new Dictionary<BuildingDefinition, int>();
			string[] array = BuildingsString.Split('|');
			for (int num = array.Length - 1; num >= 0; num--)
			{
				string[] array2 = array[num].Split(',');
				Buildings.Add(BuildingDatabase.BuildingDefinitions[array2[0]], int.Parse(array2[1]));
			}
		}
	}

	[Serializable]
	public struct RecruitedHeroInfo
	{
		[XmlAttribute]
		public int HeroLevel;

		[XmlAttribute]
		public int Cost;

		[XmlAttribute]
		public string CityId;
	}

	[Serializable]
	public struct TrapUsedInfo
	{
		[XmlAttribute]
		public string TrapId;

		[XmlAttribute]
		public int Count;
	}

	[Serializable]
	public struct UsablesEquippedInfo
	{
		[XmlAttribute]
		public double Level;

		[XmlAttribute]
		public double Count;
	}

	[Serializable]
	public struct ScavengedBonePilesInfo
	{
		[XmlAttribute]
		public string BonePileId;

		[XmlAttribute]
		public double Count;
	}

	public List<DoubleValue> DoubleValues;

	public List<EnemyKills> EnemiesKills = new List<EnemyKills>();

	public List<BuiltBuildingInfos> BuildingsBuiltDetailsNew = new List<BuiltBuildingInfos>();

	public List<HighestAttackDamagesByType> HighestAttacksDamagesByType = new List<HighestAttackDamagesByType>();

	public List<ProducedItem> ItemsProduced = new List<ProducedItem>();

	public List<BoughtItem> ItemsBought = new List<BoughtItem>();

	public List<RecruitedHeroInfo> RecruitedHeroes = new List<RecruitedHeroInfo>();

	public List<CompletedRun> RunsCompleted = new List<CompletedRun>();

	public List<StatValueReached> StatValuesReached = new List<StatValueReached>();

	public List<LowestPanicLevel> LowestPanicLevels = new List<LowestPanicLevel>();

	public List<NightReached> NightsReached = new List<NightReached>();

	public List<TrapUsedInfo> TrapsUsed = new List<TrapUsedInfo>();

	public List<UsablesEquippedInfo> UsablesEquipped = new List<UsablesEquippedInfo>();

	public List<DamageInflictedByEnemyType> DamageInflictedToEnemies = new List<DamageInflictedByEnemyType>();

	public List<DamageTakenByEnemyType> DamageTakenByEnemies = new List<DamageTakenByEnemyType>();

	public List<UsesPerWeapon> UsesPerWeapons = new List<UsesPerWeapon>();

	public List<ScavengedBonePilesInfo> ScavengedBonePiles = new List<ScavengedBonePilesInfo>();

	[XmlIgnore]
	public List<string> EnemiesKilledList
	{
		get
		{
			List<string> list = new List<string>();
			foreach (EnemyKills enemiesKill in EnemiesKills)
			{
				for (int i = 0; i < enemiesKill.Count; i++)
				{
					list.Add(enemiesKill.Id);
				}
			}
			return list;
		}
	}

	public double EnemiesKilledCount => EnemiesKills.Sum((EnemyKills o) => o.Count);

	public double EnemiesKilledFromPoison => EnemiesKills.Sum((EnemyKills o) => o.FromPoisonCount);

	public double IsolatedEnemiesKilled => EnemiesKills.Sum((EnemyKills o) => o.IsolatedCount);

	public double GoldSpentInRecruitment => RecruitedHeroes.Sum((RecruitedHeroInfo o) => o.Cost);

	public double ItemsProducedCount => ItemsProduced.Count;

	public double ItemsBoughtCount => ItemsBought.Count;

	public double ItemsBoughtOrProducedCount => ItemsProducedCount + ItemsBoughtCount;

	public double LastLoadedVersion => ApplicationManager.LastLoadedVersion;

	public double PotionsOnSelf => DoubleValues.Find((DoubleValue o) => o.Category == E_ValueCategory.PotionsUsed).Value - DoubleValues.Find((DoubleValue o) => o.Category == E_ValueCategory.PotionsOnAllies).Value;

	public double RecruitedHeroesCount => RecruitedHeroes.Count;

	public double RunsFinishedCount => RunsCompleted.Count;

	public double RunsCompletedCount => RunsCompleted.FindAll((CompletedRun run) => run.IsVictorious).Count;

	public double RunsLostCount
	{
		get
		{
			CityDefinition value;
			return RunsCompleted.FindAll((CompletedRun run) => !run.IsVictorious && CityDatabase.CityDefinitions.TryGetValue(run.CityId, out value) && !value.IsTutorialMap).Count;
		}
	}

	public double MaxNightReached
	{
		get
		{
			if (NightsReached.Count <= 0)
			{
				return 0.0;
			}
			return NightsReached.Max((NightReached o) => o.Night);
		}
	}

	public double NightsPlayed => NightsReached.Count;

	public double NightsWonWithMaxFog => NightsWonWithMaxFogInCity(string.Empty);

	public double TrapsUsedCount => TrapsUsed.Sum((TrapUsedInfo o) => o.Count);

	public double TutorialDone => ApplicationManager.Application.TutorialDone ? 1 : 0;

	public double ScavengedBonePilesCount => ScavengedBonePiles.Sum((ScavengedBonePilesInfo o) => o.Count);

	public List<string> BuildingsBuilt
	{
		get
		{
			List<string> list = new List<string>();
			foreach (BuiltBuildingInfos item in BuildingsBuiltDetailsNew)
			{
				for (int i = 0; i < item.Count; i++)
				{
					list.Add(item.Id);
				}
			}
			return list;
		}
	}

	public double LowestPanicLevelReached
	{
		get
		{
			double num = PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length;
			foreach (LowestPanicLevel lowestPanicLevel in LowestPanicLevels)
			{
				if (lowestPanicLevel.PanicLevel < num)
				{
					num = lowestPanicLevel.PanicLevel;
				}
			}
			return num;
		}
	}

	public MetaConditionSpecificContext()
	{
		DoubleValues = new List<DoubleValue>();
	}

	public double GetDouble(string valueCategory)
	{
		if (!Enum.TryParse<E_ValueCategory>(valueCategory, out var result))
		{
			TPSingleton<MetaConditionManager>.Instance.LogError("Could not parse string into MetaCondition value category: " + valueCategory + ".");
			return 0.0;
		}
		return GetDouble(result);
	}

	public double GetDouble(E_ValueCategory valueCategory)
	{
		return DoubleValues.Find((DoubleValue o) => o.Category == valueCategory).Value;
	}

	public double BuiltBuildingsOfCategory(string category)
	{
		if (!Enum.TryParse<BuildingDefinition.E_BuildingCategory>(category, out var result))
		{
			TPSingleton<BuildingManager>.Instance.LogError("Could not parse string into building category enum : " + category);
			return 0.0;
		}
		return BuiltBuildingsOfCategory(result);
	}

	public double BuiltBuildingsOfCategory(BuildingDefinition.E_BuildingCategory category)
	{
		int num = 0;
		for (int i = 0; i < BuildingsBuiltDetailsNew.Count; i++)
		{
			if (!Enum.TryParse<BuildingDefinition.E_BuildingCategory>(BuildingsBuiltDetailsNew[i].Category, out var result))
			{
				TPSingleton<BuildingManager>.Instance.LogError("Could not parse string into category enum : " + BuildingsBuiltDetailsNew[i].Category);
			}
			else if (result.HasFlag(category))
			{
				num += BuildingsBuiltDetailsNew[i].Count;
			}
		}
		return num;
	}

	public double BuiltBuildingsOfId(string id)
	{
		return BuildingsBuiltDetailsNew.Sum((BuiltBuildingInfos builtBuilding) => (id == builtBuilding.Id) ? builtBuilding.Count : 0);
	}

	public double BuiltBuildingsOfIdsList(IdsListDefinition list)
	{
		return BuildingsBuiltDetailsNew.Sum((BuiltBuildingInfos builtBuilding) => list.Ids.Contains(builtBuilding.Id) ? builtBuilding.Count : 0);
	}

	public double DamageByEnemyType(string enemyId)
	{
		return (DamageTakenByEnemies.FindAll((DamageTakenByEnemyType o) => o.EnemyId == enemyId)?.OrderBy((DamageTakenByEnemyType o) => o.Value)?.FirstOrDefault().Value).GetValueOrDefault();
	}

	public double DamageInflictedToEnemy(string enemyId)
	{
		return (DamageInflictedToEnemies.FindAll((DamageInflictedByEnemyType o) => o.EnemyId == enemyId)?.OrderBy((DamageInflictedByEnemyType o) => o.Value)?.FirstOrDefault().Value).GetValueOrDefault();
	}

	public double EnemiesKilledFromPoisonOfId(string enemyId)
	{
		return EnemiesKills.Find((EnemyKills o) => o.Id == enemyId).FromPoisonCount;
	}

	public double IsolatedEnemiesKilledOfId(string enemyId)
	{
		return EnemiesKills.Find((EnemyKills o) => o.Id == enemyId).IsolatedCount;
	}

	public double HighestDamagesDealtOfType(string damagesType)
	{
		return HighestAttacksDamagesByType.Find((HighestAttackDamagesByType damages) => damages.DamagesType == damagesType).Value;
	}

	public double ItemsProducedFromBuilding(string buildingId)
	{
		return ItemsProduced.FindAll((ProducedItem item) => item.SourceBuildingId == buildingId).Count;
	}

	public double ItemsBoughtOfCategory(string category)
	{
		if (!Enum.TryParse<ItemDefinition.E_Category>(category, out var categoryParsed))
		{
			TPSingleton<ItemManager>.Instance.LogError("Could not parse string into item category enum : " + category);
			return 0.0;
		}
		return ItemsBought.FindAll((BoughtItem item) => (item.Category & categoryParsed) != 0).Count;
	}

	public double ItemsBoughtOrProducedOfCategory(string category)
	{
		if (!Enum.TryParse<ItemDefinition.E_Category>(category, out var categoryParsed))
		{
			TPSingleton<ItemManager>.Instance.LogError("Could not parse string into item category enum : " + category);
			return 0.0;
		}
		return ItemsBought.Count((BoughtItem item) => (item.Category & categoryParsed) != 0) + ItemsProduced.Count((ProducedItem item) => (item.Category & categoryParsed) != 0);
	}

	public double MaxHeroStatValueReachedForStat(string stat)
	{
		return (StatValuesReached.FindAll((StatValueReached statValue) => statValue.Stat == stat)?.OrderBy((StatValueReached o) => o.Value)?.FirstOrDefault().Value).GetValueOrDefault();
	}

	public double RecruitedHeroesWithMinimumLevel(string level)
	{
		return RecruitedHeroes.FindAll((RecruitedHeroInfo hero) => (double)hero.HeroLevel >= Convert.ToDouble(level)).Count;
	}

	public double RecruitedHeroesInCity(string cityId)
	{
		return RecruitedHeroes.FindAll((RecruitedHeroInfo hero) => hero.CityId == cityId).Count;
	}

	public double RecruitedHeroesWithMinimumLevelInCity(string level, string cityId)
	{
		return RecruitedHeroes.FindAll((RecruitedHeroInfo hero) => (double)hero.HeroLevel >= Convert.ToDouble(level) && hero.CityId == cityId).Count;
	}

	public double RunsFinishedInCity(string cityId)
	{
		return RunsCompleted.FindAll((CompletedRun run) => run.CityId == cityId).Count;
	}

	public double RunsCompletedInCity(string cityId)
	{
		return RunsCompleted.FindAll((CompletedRun run) => run.IsVictorious && run.CityId == cityId).Count;
	}

	public double RunsLostInCity(string cityId)
	{
		return RunsCompleted.FindAll((CompletedRun run) => !run.IsVictorious && run.CityId == cityId).Count;
	}

	public double RunsFinishedWithAtLeastApocalypseIndex(string apocalypseIndex)
	{
		return RunsCompleted.FindAll((CompletedRun run) => Convert.ToDouble(apocalypseIndex) <= (double)run.ApocalypseIndex).Count;
	}

	public double RunsCompletedWithAtLeastApocalypseIndex(string apocalypseIndex)
	{
		return RunsCompleted.FindAll((CompletedRun run) => Convert.ToDouble(apocalypseIndex) <= (double)run.ApocalypseIndex && run.IsVictorious).Count;
	}

	public double RunsFinishedInCityWithAtLeastApocalypseIndex(string cityId, string apocalypseIndex)
	{
		return RunsCompleted.FindAll((CompletedRun run) => Convert.ToDouble(apocalypseIndex) <= (double)run.ApocalypseIndex && run.CityId == cityId).Count;
	}

	public double RunsCompletedInCityWithAtLeastApocalypseIndex(string cityId, string apocalypseIndex)
	{
		return RunsCompleted.FindAll((CompletedRun run) => Convert.ToDouble(apocalypseIndex) <= (double)run.ApocalypseIndex && run.IsVictorious && run.CityId == cityId).Count;
	}

	public double ScavengedBonePilesCountOfId(string bonePileId)
	{
		return ScavengedBonePiles.Find((ScavengedBonePilesInfo o) => o.BonePileId == bonePileId).Count;
	}

	public double LowestPanicLevelReachedInCity(string cityId)
	{
		for (int num = LowestPanicLevels.Count - 1; num >= 0; num--)
		{
			if (LowestPanicLevels[num].CityId == cityId)
			{
				return LowestPanicLevels[num].PanicLevel;
			}
		}
		return PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length;
	}

	public double MaxNightReachedInCity(string cityId)
	{
		double num = 0.0;
		foreach (NightReached item in NightsReached)
		{
			if (item.CityId == cityId)
			{
				num = Math.Max(num, item.Night);
			}
		}
		return num;
	}

	public double NightsPlayedInCity(string cityId)
	{
		return NightsReached.Count((NightReached o) => o.CityId == cityId);
	}

	public double NightsPlayedWithBuildingCount(string buildingId, string count)
	{
		double num = 0.0;
		foreach (NightReached item in NightsReached)
		{
			if (item.Buildings == null)
			{
				item.GenerateBuildingFromString();
			}
			if (item.Buildings.TryGetValue(BuildingDatabase.BuildingDefinitions[buildingId], out var value) && (double)value >= Convert.ToDouble(count))
			{
				num += 1.0;
			}
		}
		return num;
	}

	public double NightsWonWithMaxFogInCity(string cityId)
	{
		double num = 0.0;
		foreach (NightReached item in NightsReached)
		{
			if ((string.IsNullOrEmpty(cityId) || !(item.CityId != cityId)) && CityDatabase.CityDefinitions.TryGetValue(item.CityId, out var value))
			{
				FogDefinition fogDefinition = FogDatabase.FogsDefinitions[value.FogDefinitionId];
				if (item.FogDensityIndex >= (double)(fogDefinition.FogDensities.Count - 1))
				{
					num += 1.0;
				}
			}
		}
		return num;
	}

	public double NightsWonWithMaxFogExcludingCity(string cityId)
	{
		double num = 0.0;
		foreach (NightReached item in NightsReached)
		{
			if ((string.IsNullOrEmpty(cityId) || !(item.CityId == cityId)) && CityDatabase.CityDefinitions.TryGetValue(item.CityId, out var value))
			{
				FogDefinition fogDefinition = FogDatabase.FogsDefinitions[value.FogDefinitionId];
				if (item.FogDensityIndex >= (double)(fogDefinition.FogDensities.Count - 1))
				{
					num += 1.0;
				}
			}
		}
		return num;
	}

	public double WeaponUses(string weaponId)
	{
		return (UsesPerWeapons.FindAll((UsesPerWeapon o) => o.WeaponId == weaponId)?.OrderBy((UsesPerWeapon o) => o.Value)?.FirstOrDefault().Value).GetValueOrDefault();
	}

	public double WeaponUsesPerTag(string tag)
	{
		if (!ItemDatabase.ItemsByTag.TryGetValue(tag, out var taggedItems))
		{
			return 0.0;
		}
		return UsesPerWeapons.Where((UsesPerWeapon o) => taggedItems.Contains(o.WeaponId)).Sum((UsesPerWeapon o) => o.Value);
	}

	public double ActionPointsSpentPerTag(string tag)
	{
		if (!ItemDatabase.ItemsByTag.TryGetValue(tag, out var taggedItems))
		{
			return 0.0;
		}
		return UsesPerWeapons.Where((UsesPerWeapon o) => taggedItems.Contains(o.WeaponId)).Sum((UsesPerWeapon o) => o.ActionPointsSpent);
	}

	public double TrapsUsedOfIdCount(string trapId)
	{
		return (TrapsUsed.FindAll((TrapUsedInfo o) => o.TrapId == trapId)?.OrderBy((TrapUsedInfo o) => o.Count)?.FirstOrDefault().Count).GetValueOrDefault();
	}

	public double UsablesEquippedOfLevel(string level)
	{
		double levelDouble = Convert.ToDouble(level);
		return UsablesEquipped.Find((UsablesEquippedInfo o) => o.Level == levelDouble).Count;
	}

	public double UsablesEquippedOfMinimumLevel(string level)
	{
		double levelDouble = Convert.ToDouble(level);
		return UsablesEquipped.Where((UsablesEquippedInfo o) => o.Level >= levelDouble).Sum((UsablesEquippedInfo o) => o.Count);
	}

	public bool ShouldSerializeDoubleValues()
	{
		return DoubleValues.Count > 0;
	}

	public bool ShouldSerializeEnemiesKills()
	{
		if (EnemiesKills != null)
		{
			return EnemiesKills.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeItemsBought()
	{
		if (ItemsBought != null)
		{
			return ItemsBought.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeLowestPanicLevelReached()
	{
		return false;
	}

	public bool ShouldSerializeLowestPanicLevels()
	{
		if (LowestPanicLevels != null)
		{
			return LowestPanicLevels.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeNightsReached()
	{
		if (NightsReached != null)
		{
			return NightsReached.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeDamageInflictedToEnemies()
	{
		if (DamageInflictedToEnemies != null)
		{
			return DamageInflictedToEnemies.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeDamageTakenByEnemies()
	{
		if (DamageTakenByEnemies != null)
		{
			return DamageTakenByEnemies.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeUsesPerWeapons()
	{
		if (UsesPerWeapons != null)
		{
			return UsesPerWeapons.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeItemsProduced()
	{
		if (ItemsProduced != null)
		{
			return ItemsProduced.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeHighestAttacksDamagesByType()
	{
		if (HighestAttacksDamagesByType != null)
		{
			return HighestAttacksDamagesByType.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeRunsCompleted()
	{
		if (RunsCompleted != null)
		{
			return RunsCompleted.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeStatValuesReached()
	{
		if (StatValuesReached != null)
		{
			return StatValuesReached.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeBuildingsBuiltDetailsNew()
	{
		if (BuildingsBuiltDetailsNew != null)
		{
			return BuildingsBuiltDetailsNew.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeTrapsUsed()
	{
		if (TrapsUsed != null)
		{
			return TrapsUsed.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeScavengedBonePiles()
	{
		if (ScavengedBonePiles != null)
		{
			return ScavengedBonePiles.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeUsablesEquipped()
	{
		if (UsablesEquipped != null)
		{
			return UsablesEquipped.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeRecruitedHeroes()
	{
		if (RecruitedHeroes != null)
		{
			return RecruitedHeroes.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeBuildingsEnemiesKills()
	{
		if (EnemiesKills != null)
		{
			return EnemiesKills.Count > 0;
		}
		return false;
	}

	public bool ShouldSerializeBuildingsBuilt()
	{
		return false;
	}
}
