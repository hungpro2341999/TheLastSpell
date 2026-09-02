using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization.Meta;

namespace TheLastStand.Manager.Meta;

public class MetaConditionManager : Manager<MetaConditionManager>
{
	public static class Constants
	{
		public const string GlobalContext = "ENVIRONMENT";

		public const string CampaignContext = "CAMPAIGN";

		public const string RunContext = "RUN";

		public const string LocalContext = "LOCAL";

		public const char ContextSeparator = ':';
	}

	private readonly MetaConditionGlobalContext globalContext = new MetaConditionGlobalContext();

	private MetaConditionSpecificContext runContext = new MetaConditionSpecificContext();

	private MetaConditionSpecificContext campaignContext;

	private readonly List<MetaConditionController> metaConditionControllers = new List<MetaConditionController>();

	private bool isDirty;

	private CancellationTokenSource refreshCancellationTokenSource = new CancellationTokenSource();

	public static bool IsRefreshingWithDispatch { get; private set; }

	public MetaConditionSpecificContext RunContext => runContext;

	public MetaConditionSpecificContext CampaignContext => campaignContext;

	public MetaConditionsDatabase ConditionsLibrary { get; } = new MetaConditionsDatabase();

	public bool DisableRefreshes { private get; set; }

	private bool ShouldUpdateConditions
	{
		get
		{
			if (!(ApplicationManager.Application.State.GetName() != "Game"))
			{
				return !TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap;
			}
			return true;
		}
	}

	public static event Action OnConditionsRefreshed;

	public MetaConditionController CreateMetaCondition(string id, MetaUpgradeController upgradeController, MetaConditionDefinition definition, MetaCondition.E_MetaConditionCategory category)
	{
		if (metaConditionControllers.Find((MetaConditionController o) => o.MetaCondition.Id == id) != null)
		{
			throw new Exception("Duplicate meta condition Id " + id + ".");
		}
		MetaConditionController metaConditionController = new MetaConditionController(new MetaCondition(upgradeController)
		{
			MetaConditionDefinition = definition,
			Id = id,
			LocalContext = new MetaConditionSpecificContext(),
			Category = category
		}, runContext, campaignContext, globalContext);
		metaConditionControllers.Add(metaConditionController);
		return metaConditionController;
	}

	public void DeserializeFromAppSave(ISerializedData container = null)
	{
		SerializedMetaConditions serializedMetaConditions = (container as SerializedMetaConditions) ?? new SerializedMetaConditions();
		campaignContext = serializedMetaConditions.CampaignContext?.Context ?? new MetaConditionSpecificContext();
		Log($"Read campaign context from disk, updating every single reference of condition controllers to reference {campaignContext.GetHashCode()}.");
		foreach (MetaConditionController metaConditionController2 in metaConditionControllers)
		{
			metaConditionController2.SetCampaignContext(campaignContext);
		}
		foreach (SerializedMetaCondition serializedCondition in serializedMetaConditions.Conditions)
		{
			MetaUpgradeController controller = TPSingleton<MetaUpgradesManager>.Instance.GetController(serializedCondition.MetaUpgradeId);
			MetaConditionController metaConditionController = metaConditionControllers.Find((MetaConditionController o) => o.MetaCondition.Id == serializedCondition.Id);
			if (controller == null)
			{
				LogWarning("Skipped unknown upgrade for condition " + serializedCondition.Id + " (upgrade: " + serializedCondition.MetaUpgradeId + ") in the game save (?)");
				continue;
			}
			if (metaConditionController == null)
			{
				LogWarning("Skipped unknown condition " + serializedCondition.Id + " (upgrade: " + serializedCondition.MetaUpgradeId + ") in the game save (?)");
				continue;
			}
			MetaCondition condition = new MetaCondition(controller)
			{
				MetaConditionDefinition = metaConditionController.MetaCondition.MetaConditionDefinition
			};
			condition.Deserialize(serializedCondition);
			if (condition.Id == null)
			{
				throw new Exception($"Invalid Id on serialized condition {condition} (non-loaded or non-existing condition), breaking save loading.");
			}
			metaConditionControllers.RemoveAll((MetaConditionController o) => o.MetaCondition.Id == condition.Id);
			MetaConditionController item = new MetaConditionController(condition, runContext, campaignContext, globalContext);
			metaConditionControllers.Add(item);
		}
		LoadContextsDatabaseRelatedData();
	}

	public void DeserializeFromGameSave(ISerializedData container = null)
	{
		if (container is SerializedMetaConditionsContext serializedMetaConditionsContext)
		{
			UpdateRunContext(serializedMetaConditionsContext.Context);
		}
		else
		{
			RenewRunContext();
		}
		runContext = (container as SerializedMetaConditionsContext)?.Context ?? new MetaConditionSpecificContext();
		Log($"Read run context from disk, updating every single reference of condition controllers to reference RunContext {runContext.GetHashCode()}");
		foreach (MetaConditionController metaConditionController in metaConditionControllers)
		{
			metaConditionController.SetRunContext(runContext);
		}
		LoadContextsDatabaseRelatedData();
		TPSingleton<MetaConditionManager>.Instance.isDirty = true;
	}

	public void EraseConditionsControllers()
	{
		metaConditionControllers.Clear();
	}

	public List<MetaConditionController> GetControllers(MetaUpgrade upgrade, MetaCondition.E_MetaConditionCategory category)
	{
		return metaConditionControllers.FindAll((MetaConditionController controller) => controller.MetaCondition.Category == category && controller.MetaCondition.MetaUpgradeController.MetaUpgrade == upgrade);
	}

	public static List<List<MetaConditionController>> SplitConditionsByGroupIndex(List<MetaConditionController> conditions)
	{
		List<List<MetaConditionController>> list = new List<List<MetaConditionController>>();
		foreach (var item in conditions.GroupBy((MetaConditionController o) => o.MetaCondition.ConditionsGroupIndex, (int key, IEnumerable<MetaConditionController> values) => new
		{
			Index = key,
			Conditions = values
		}))
		{
			list.Add(item.Conditions.Select((MetaConditionController o) => o).ToList());
		}
		return list;
	}

	public void RefreshProgression()
	{
		if (DisableRefreshes)
		{
			return;
		}
		TPSingleton<MetaConditionManager>.Instance.Log($"Setting run context to {runContext.GetHashCode()} for {metaConditionControllers.Count} conditions and refreshing their progression.", CLogLevel.DETAILED);
		foreach (MetaConditionController metaConditionController in metaConditionControllers)
		{
			metaConditionController.SetRunContext(runContext);
		}
		foreach (MetaConditionController metaConditionController2 in metaConditionControllers)
		{
			metaConditionController2.RefreshProgression(ConditionsLibrary);
		}
		MetaConditionManager.OnConditionsRefreshed?.Invoke();
	}

	public void RefreshUpgradeProgression(MetaUpgrade metaUpgrade)
	{
		if (!DisableRefreshes)
		{
			TPSingleton<MetaConditionManager>.Instance.Log($"Setting run context to {runContext.GetHashCode()} for upgrade {metaUpgrade.MetaUpgradeDefinition.Id} and refreshing its progression.", CLogLevel.DETAILED);
			List<MetaConditionController> controllers = GetControllers(metaUpgrade, MetaCondition.E_MetaConditionCategory.Unlock);
			List<MetaConditionController> controllers2 = GetControllers(metaUpgrade, MetaCondition.E_MetaConditionCategory.Activation);
			controllers.ForEach(delegate(MetaConditionController o)
			{
				o.SetRunContext(runContext);
				o.SetCampaignContext(campaignContext);
			});
			controllers2.ForEach(delegate(MetaConditionController o)
			{
				o.SetRunContext(runContext);
				o.SetCampaignContext(campaignContext);
			});
			controllers.ForEach(delegate(MetaConditionController o)
			{
				o.RefreshProgression(ConditionsLibrary);
			});
			controllers2.ForEach(delegate(MetaConditionController o)
			{
				o.RefreshProgression(ConditionsLibrary);
			});
			MetaConditionManager.OnConditionsRefreshed?.Invoke();
		}
	}

	public void RenewRunContext()
	{
		UpdateRunContext(new MetaConditionSpecificContext());
	}

	public ISerializedData SerializeToAppSave()
	{
		SortConditionsData();
		SerializedMetaConditions serializedMetaConditions = new SerializedMetaConditions();
		serializedMetaConditions.CampaignContext = new SerializedMetaConditionsContext
		{
			Context = campaignContext
		};
		serializedMetaConditions.Conditions = new List<SerializedMetaCondition>();
		serializedMetaConditions.Conditions.AddRange(from o in metaConditionControllers
			where !TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Contains(o.MetaCondition.MetaUpgradeController.MetaUpgrade)
			select o.MetaCondition.Serialize() as SerializedMetaCondition);
		return serializedMetaConditions;
	}

	public ISerializedData SerializeToGameSave()
	{
		return new SerializedMetaConditionsContext
		{
			Context = runContext
		};
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		refreshCancellationTokenSource.Cancel();
	}

	private IEnumerable<MetaConditionSpecificContext> GetContextsToIncreaseStatsOf()
	{
		return new List<MetaConditionSpecificContext> { campaignContext, runContext }.Concat(from o in metaConditionControllers
			where !TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Contains(o.MetaCondition.MetaUpgradeController.MetaUpgrade)
			select o.MetaCondition.LocalContext);
	}

	private void UpdateRunContext(MetaConditionSpecificContext newRunContext)
	{
		runContext = newRunContext;
		metaConditionControllers.ForEach(delegate(MetaConditionController o)
		{
			o.SetRunContext(runContext);
		});
	}

	private void Update()
	{
		if (isDirty)
		{
			isDirty = false;
			RefreshProgression();
		}
	}

	private void LoadContextsDatabaseRelatedData()
	{
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			for (int num = item.BuildingsBuiltDetailsNew.Count - 1; num >= 0; num--)
			{
				if (!BuildingDatabase.BuildingDefinitions.ContainsKey(item.BuildingsBuiltDetailsNew[num].Id))
				{
					TPSingleton<MetaConditionManager>.Instance.LogError("Trying to load BuildingsBuiltDetailsNew context but can not find " + item.BuildingsBuiltDetailsNew[num].Id + " in BuildingDefinitions.", CLogLevel.MAJOR);
					return;
				}
				item.BuildingsBuiltDetailsNew[num] = new MetaConditionSpecificContext.BuiltBuildingInfos
				{
					Id = item.BuildingsBuiltDetailsNew[num].Id,
					Count = item.BuildingsBuiltDetailsNew[num].Count,
					Category = BuildingDatabase.BuildingDefinitions[item.BuildingsBuiltDetailsNew[num].Id].BlueprintModuleDefinition.Category.ToString()
				};
			}
			for (int num2 = item.ItemsBought.Count - 1; num2 >= 0; num2--)
			{
				if (!ItemDatabase.AllItemsDefinitions.ContainsKey(item.ItemsBought[num2].Id))
				{
					TPSingleton<MetaConditionManager>.Instance.LogError("Trying to load ItemsBought context but can not find " + item.ItemsBought[num2].Id + " in AllItemsDefinitions.", CLogLevel.MAJOR);
					return;
				}
				item.ItemsBought[num2] = new MetaConditionSpecificContext.BoughtItem
				{
					Id = item.ItemsBought[num2].Id,
					Cost = item.ItemsBought[num2].Cost,
					Category = ItemDatabase.AllItemsDefinitions[item.ItemsBought[num2].Id].Category
				};
			}
			for (int num3 = item.ItemsProduced.Count - 1; num3 >= 0; num3--)
			{
				if (!ItemDatabase.AllItemsDefinitions.ContainsKey(item.ItemsProduced[num3].Id))
				{
					TPSingleton<MetaConditionManager>.Instance.LogError("Trying to load ItemsProduced context but can not find " + item.ItemsProduced[num3].Id + " in AllItemsDefinitions.", CLogLevel.MAJOR);
					return;
				}
				item.ItemsProduced[num3] = new MetaConditionSpecificContext.ProducedItem
				{
					Id = item.ItemsProduced[num3].Id,
					SourceBuildingId = item.ItemsProduced[num3].SourceBuildingId,
					Category = ItemDatabase.AllItemsDefinitions[item.ItemsProduced[num3].Id].Category
				};
			}
		}
	}

	private void SortConditionsData()
	{
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			item.ItemsProduced.Sort((MetaConditionSpecificContext.ProducedItem a, MetaConditionSpecificContext.ProducedItem b) => a.SourceBuildingId.CompareTo(b.SourceBuildingId));
			item.BuildingsBuiltDetailsNew.Sort((MetaConditionSpecificContext.BuiltBuildingInfos a, MetaConditionSpecificContext.BuiltBuildingInfos b) => b.Count.CompareTo(a.Count));
			item.StatValuesReached.Sort((MetaConditionSpecificContext.StatValueReached a, MetaConditionSpecificContext.StatValueReached b) => b.Value.CompareTo(a.Value));
			item.EnemiesKills.Sort((MetaConditionSpecificContext.EnemyKills a, MetaConditionSpecificContext.EnemyKills b) => b.Count.CompareTo(a.Count));
			item.DamageInflictedToEnemies.Sort((MetaConditionSpecificContext.DamageInflictedByEnemyType a, MetaConditionSpecificContext.DamageInflictedByEnemyType b) => b.Value.CompareTo(a.Value));
			item.DamageTakenByEnemies.Sort((MetaConditionSpecificContext.DamageTakenByEnemyType a, MetaConditionSpecificContext.DamageTakenByEnemyType b) => b.Value.CompareTo(a.Value));
			item.UsesPerWeapons.Sort((MetaConditionSpecificContext.UsesPerWeapon a, MetaConditionSpecificContext.UsesPerWeapon b) => b.Value.CompareTo(a.Value));
		}
	}

	public void IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory valueCategory, double value)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			MetaConditionSpecificContext.DoubleValue doubleValue = item.DoubleValues.Find(Match);
			item.DoubleValues.RemoveAll(Match);
			item.DoubleValues.Add(new MetaConditionSpecificContext.DoubleValue
			{
				Category = valueCategory,
				Value = doubleValue.Value + value
			});
		}
		isDirty = true;
		bool Match(MetaConditionSpecificContext.DoubleValue o)
		{
			return o.Category == valueCategory;
		}
	}

	public void DecreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory valueCategory, double value)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			MetaConditionSpecificContext.DoubleValue doubleValue = item.DoubleValues.Find(Match);
			item.DoubleValues.RemoveAll(Match);
			item.DoubleValues.Add(new MetaConditionSpecificContext.DoubleValue
			{
				Category = valueCategory,
				Value = doubleValue.Value - value
			});
		}
		isDirty = true;
		bool Match(MetaConditionSpecificContext.DoubleValue o)
		{
			return o.Category == valueCategory;
		}
	}

	public void RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory valueCategory, double value)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			MetaConditionSpecificContext.DoubleValue doubleValue = item.DoubleValues.Find(Match);
			item.DoubleValues.RemoveAll(Match);
			item.DoubleValues.Add(new MetaConditionSpecificContext.DoubleValue
			{
				Category = valueCategory,
				Value = Math.Max(doubleValue.Value, value)
			});
		}
		isDirty = true;
		bool Match(MetaConditionSpecificContext.DoubleValue o)
		{
			return o.Category == valueCategory;
		}
	}

	public void IncreaseBuildingsBuilt(BuildingDefinition buildingDefinition)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			int count = item.BuildingsBuiltDetailsNew.Find((MetaConditionSpecificContext.BuiltBuildingInfos o) => o.Id == buildingDefinition.Id).Count;
			item.BuildingsBuiltDetailsNew.RemoveAll((MetaConditionSpecificContext.BuiltBuildingInfos o) => o.Id == buildingDefinition.Id);
			item.BuildingsBuiltDetailsNew.Add(new MetaConditionSpecificContext.BuiltBuildingInfos
			{
				Category = buildingDefinition.BlueprintModuleDefinition.Category.ToString(),
				Id = buildingDefinition.Id,
				Count = count + 1
			});
		}
		isDirty = true;
	}

	public void IncreaseDamageTakenByEnemyType(string enemyId, float value)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			double value2 = item.DamageTakenByEnemies.Find((MetaConditionSpecificContext.DamageTakenByEnemyType o) => o.EnemyId == enemyId).Value;
			item.DamageTakenByEnemies.RemoveAll((MetaConditionSpecificContext.DamageTakenByEnemyType o) => o.EnemyId == enemyId);
			item.DamageTakenByEnemies.Add(new MetaConditionSpecificContext.DamageTakenByEnemyType
			{
				EnemyId = enemyId,
				Value = value2 + (double)value
			});
		}
		isDirty = true;
	}

	public void IncreaseDamageInflictedToEnemyType(string enemyId, float value)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			double value2 = item.DamageInflictedToEnemies.Find((MetaConditionSpecificContext.DamageInflictedByEnemyType o) => o.EnemyId == enemyId).Value;
			item.DamageInflictedToEnemies.RemoveAll((MetaConditionSpecificContext.DamageInflictedByEnemyType o) => o.EnemyId == enemyId);
			item.DamageInflictedToEnemies.Add(new MetaConditionSpecificContext.DamageInflictedByEnemyType
			{
				EnemyId = enemyId,
				Value = value2 + (double)value
			});
		}
		isDirty = true;
	}

	public void IncreaseEnemiesKilled(EnemyUnit enemyUnit)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		IEnumerable<MetaConditionSpecificContext> contextsToIncreaseStatsOf = GetContextsToIncreaseStatsOf();
		string enemyId = enemyUnit.Id;
		foreach (MetaConditionSpecificContext item in contextsToIncreaseStatsOf)
		{
			MetaConditionSpecificContext.EnemyKills enemyKills = item.EnemiesKills.Find((MetaConditionSpecificContext.EnemyKills o) => o.Id == enemyId);
			item.EnemiesKills.RemoveAll((MetaConditionSpecificContext.EnemyKills o) => o.Id == enemyId);
			item.EnemiesKills.Add(new MetaConditionSpecificContext.EnemyKills
			{
				Id = enemyId,
				Count = enemyKills.Count + 1,
				FromPoisonCount = enemyKills.FromPoisonCount,
				IsolatedCount = (enemyUnit.IsIsolated ? (enemyKills.IsolatedCount + 1) : enemyKills.IsolatedCount)
			});
		}
		isDirty = true;
	}

	public void IncreaseEnemyKillsFromPoison(string enemyId)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			MetaConditionSpecificContext.EnemyKills enemyKills = item.EnemiesKills.Find((MetaConditionSpecificContext.EnemyKills o) => o.Id == enemyId);
			item.EnemiesKills.RemoveAll((MetaConditionSpecificContext.EnemyKills o) => o.Id == enemyId);
			item.EnemiesKills.Add(new MetaConditionSpecificContext.EnemyKills
			{
				Id = enemyId,
				Count = enemyKills.Count,
				FromPoisonCount = enemyKills.FromPoisonCount + 1,
				IsolatedCount = enemyKills.IsolatedCount
			});
		}
		isDirty = true;
	}

	public void IncreaseProducedItems(ItemDefinition item, string sourceBuildingId = "")
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item2 in GetContextsToIncreaseStatsOf())
		{
			item2.ItemsProduced.Add(new MetaConditionSpecificContext.ProducedItem
			{
				Id = item.Id,
				Category = item.Category,
				SourceBuildingId = sourceBuildingId
			});
		}
		isDirty = true;
	}

	public void IncreaseBoughtItems(ItemDefinition item, int cost)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item2 in GetContextsToIncreaseStatsOf())
		{
			item2.ItemsBought.Add(new MetaConditionSpecificContext.BoughtItem
			{
				Id = item.Id,
				Category = item.Category,
				Cost = cost
			});
		}
		isDirty = true;
	}

	public void IncreaseRecruitedHeroes(int heroLevel, int cost)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			item.RecruitedHeroes.Add(new MetaConditionSpecificContext.RecruitedHeroInfo
			{
				CityId = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id,
				Cost = cost,
				HeroLevel = heroLevel
			});
		}
		isDirty = true;
	}

	public void IncreaseRunsCompleted(bool isGameOver)
	{
		IncreaseRunsCompleted(isGameOver, TPSingleton<WorldMapCityManager>.Instance.SelectedCity, ApocalypseManager.CurrentApocalypseLevel);
	}

	public void IncreaseRunsCompleted(bool isGameOver, WorldMapCity city, int apocalypseIndex)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			item.RunsCompleted.Add(new MetaConditionSpecificContext.CompletedRun
			{
				CityId = city.CityDefinition.Id,
				IsVictorious = !isGameOver,
				ApocalypseIndex = apocalypseIndex
			});
		}
		isDirty = true;
	}

	public void IncreaseTrapsUsed(string trapId)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			int count = item.TrapsUsed.Find((MetaConditionSpecificContext.TrapUsedInfo o) => o.TrapId == trapId).Count;
			item.TrapsUsed.RemoveAll((MetaConditionSpecificContext.TrapUsedInfo o) => o.TrapId == trapId);
			item.TrapsUsed.Add(new MetaConditionSpecificContext.TrapUsedInfo
			{
				TrapId = trapId,
				Count = count + 1
			});
		}
		isDirty = true;
	}

	public void IncreaseScavengedBonePile(string bonePileId)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			double count = item.ScavengedBonePiles.Find((MetaConditionSpecificContext.ScavengedBonePilesInfo o) => o.BonePileId == bonePileId).Count;
			item.ScavengedBonePiles.RemoveAll((MetaConditionSpecificContext.ScavengedBonePilesInfo o) => o.BonePileId == bonePileId);
			item.ScavengedBonePiles.Add(new MetaConditionSpecificContext.ScavengedBonePilesInfo
			{
				BonePileId = bonePileId,
				Count = count + 1.0
			});
		}
		isDirty = true;
	}

	public void IncreaseUsesPerWeapon(string weaponId, int actionPointsCost)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			MetaConditionSpecificContext.UsesPerWeapon usesPerWeapon = item.UsesPerWeapons.Find((MetaConditionSpecificContext.UsesPerWeapon o) => o.WeaponId == weaponId);
			item.UsesPerWeapons.RemoveAll((MetaConditionSpecificContext.UsesPerWeapon o) => o.WeaponId == weaponId);
			item.UsesPerWeapons.Add(new MetaConditionSpecificContext.UsesPerWeapon
			{
				WeaponId = weaponId,
				Value = usesPerWeapon.Value + 1.0,
				ActionPointsSpent = usesPerWeapon.ActionPointsSpent + (double)actionPointsCost
			});
		}
		isDirty = true;
	}

	public void RefreshEquippedUsables()
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		IEnumerable<MetaConditionSpecificContext> contextsToIncreaseStatsOf = GetContextsToIncreaseStatsOf();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (!playableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.Usables, out var value))
			{
				continue;
			}
			foreach (EquipmentSlot item in value)
			{
				if (item.Item != null && (item.Item.ItemDefinition.Category & ItemDefinition.E_Category.Usable) != ItemDefinition.E_Category.None)
				{
					int level = item.Item.Level;
					if (dictionary.ContainsKey(level))
					{
						dictionary[level]++;
					}
					else
					{
						dictionary.Add(level, 1);
					}
				}
			}
			foreach (MetaConditionSpecificContext item2 in contextsToIncreaseStatsOf)
			{
				foreach (KeyValuePair<int, int> countByLevel in dictionary)
				{
					if (item2.UsablesEquipped.Find((MetaConditionSpecificContext.UsablesEquippedInfo o) => o.Level == (double)countByLevel.Key).Count < (double)countByLevel.Value)
					{
						item2.UsablesEquipped.RemoveAll((MetaConditionSpecificContext.UsablesEquippedInfo o) => o.Level == (double)countByLevel.Key);
						item2.UsablesEquipped.Add(new MetaConditionSpecificContext.UsablesEquippedInfo
						{
							Level = countByLevel.Key,
							Count = countByLevel.Value
						});
					}
				}
			}
			dictionary.Clear();
		}
	}

	public void RefreshLowestPanicLevelReached(float fearLevel)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		string cityId = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			bool flag = false;
			if (item.LowestPanicLevels.Count((MetaConditionSpecificContext.LowestPanicLevel o) => o.CityId == cityId) == 0)
			{
				flag = true;
			}
			else
			{
				for (int num = 0; num < item.LowestPanicLevels.Count; num++)
				{
					if (item.LowestPanicLevels[num].CityId != cityId)
					{
						continue;
					}
					if (item.LowestPanicLevels[num].PanicLevel > (double)fearLevel)
					{
						item.LowestPanicLevels.RemoveAll((MetaConditionSpecificContext.LowestPanicLevel o) => o.CityId == cityId);
						flag = true;
					}
					break;
				}
			}
			if (flag)
			{
				item.LowestPanicLevels.Add(new MetaConditionSpecificContext.LowestPanicLevel
				{
					CityId = cityId,
					PanicLevel = fearLevel,
					ApocalypseIndex = ApocalypseManager.CurrentApocalypseLevel
				});
			}
		}
		isDirty = true;
	}

	public void RefreshMaxHeroStatReached(UnitStatDefinition.E_Stat stat, float value)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		string stringStat = stat.ToString();
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			if (item.StatValuesReached.Find((MetaConditionSpecificContext.StatValueReached o) => o.Stat == stringStat).Value < (double)value)
			{
				item.StatValuesReached.RemoveAll((MetaConditionSpecificContext.StatValueReached o) => o.Stat == stringStat);
				item.StatValuesReached.Add(new MetaConditionSpecificContext.StatValueReached
				{
					Stat = stringStat,
					Value = value
				});
			}
		}
		isDirty = true;
	}

	public void RefreshNightsReached(int night)
	{
		if (!ShouldUpdateConditions)
		{
			return;
		}
		string cityId = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		int densityIndex = TPSingleton<FogManager>.Instance.Fog.DensityIndex;
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			if (item.NightsReached.Count((MetaConditionSpecificContext.NightReached o) => o.CityId == cityId) > 0 && item.NightsReached.Where((MetaConditionSpecificContext.NightReached o) => o.CityId == cityId).Max((MetaConditionSpecificContext.NightReached o) => o.Night) < (double)night)
			{
				item.LowestPanicLevels.RemoveAll((MetaConditionSpecificContext.LowestPanicLevel o) => o.CityId == cityId);
			}
			Dictionary<BuildingDefinition, int> dictionary = new Dictionary<BuildingDefinition, int>();
			foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
			{
				if (dictionary.ContainsKey(building.BuildingDefinition))
				{
					dictionary[building.BuildingDefinition]++;
				}
				else
				{
					dictionary.Add(building.BuildingDefinition, 1);
				}
			}
			string text = string.Empty;
			foreach (KeyValuePair<BuildingDefinition, int> item2 in dictionary)
			{
				text += $"{item2.Key.Id},{item2.Value}|";
			}
			text = text.Substring(0, text.Length - 1);
			item.NightsReached.Add(new MetaConditionSpecificContext.NightReached
			{
				CityId = cityId,
				Night = night,
				FogDensityIndex = densityIndex,
				BuildingsString = text,
				Buildings = dictionary
			});
			item.NightsReached.Sort((MetaConditionSpecificContext.NightReached a, MetaConditionSpecificContext.NightReached b) => (!(a.CityId != b.CityId)) ? a.Night.CompareTo(b.Night) : a.CityId.CompareTo(b.CityId));
		}
		isDirty = true;
	}

	public void RefreshMaxSingleHitDamageByType(double damage, AttackSkillActionDefinition.E_AttackType damagesType)
	{
		if (!ShouldUpdateConditions || damagesType == AttackSkillActionDefinition.E_AttackType.None)
		{
			return;
		}
		foreach (MetaConditionSpecificContext item in GetContextsToIncreaseStatsOf())
		{
			if (item.HighestAttacksDamagesByType.Find((MetaConditionSpecificContext.HighestAttackDamagesByType o) => o.DamagesType == damagesType.ToString()).Value < damage)
			{
				item.HighestAttacksDamagesByType.RemoveAll((MetaConditionSpecificContext.HighestAttackDamagesByType o) => o.DamagesType == damagesType.ToString());
				item.HighestAttacksDamagesByType.Add(new MetaConditionSpecificContext.HighestAttackDamagesByType
				{
					DamagesType = damagesType.ToString(),
					Value = damage
				});
			}
		}
		isDirty = true;
	}

	public void RefreshMaxPlayableUnitStatReached(PlayableUnit playableUnit)
	{
		foreach (UnitStatDefinition.E_Stat item in (IEnumerable<UnitStatDefinition.E_Stat>)playableUnit.PlayableUnitStatsController.UnitStats.StatsKeys)
		{
			RefreshMaxHeroStatReached(item, playableUnit.UnitStatsController.GetStat(item).FinalClamped);
		}
	}

	public void RefreshMaxPlayableUnitStatReached(List<PlayableUnit> playableUnits)
	{
		if (playableUnits.Count == 1)
		{
			RefreshMaxPlayableUnitStatReached(playableUnits[0]);
			return;
		}
		foreach (UnitStatDefinition.E_Stat stat in (IEnumerable<UnitStatDefinition.E_Stat>)playableUnits[0].PlayableUnitStatsController.UnitStats.StatsKeys)
		{
			RefreshMaxHeroStatReached(stat, playableUnits.Max((PlayableUnit p) => p.UnitStatsController.GetStat(stat).FinalClamped));
		}
	}

	[DevConsoleCommand(Name = "ShowActivationConditionsForUpgrade")]
	public static void DebugCheckActivateConditionsForUpgrade([StringConverter(typeof(MetaUpgrade.StringToMetaUpgradeIdConverter))] string upgradeId)
	{
		TPSingleton<DebugManager>.Instance.LogDevConsoleError("Targeting " + upgradeId + "... (Don't forget to refresh!)");
		MetaUpgradeController controller = TPSingleton<MetaUpgradesManager>.Instance.GetController(upgradeId);
		List<List<MetaConditionController>> list = controller?.MetaUpgrade?.GetConditions(MetaCondition.E_MetaConditionCategory.Activation);
		for (int i = 0; i < list.Count; i++)
		{
			TPSingleton<MetaConditionManager>.Instance.LogError($"Group {i} conditions:");
			DebugPrintConditions(list[i], controller);
		}
	}

	[DevConsoleCommand(Name = "ShowUnlockConditionsForUpgrade")]
	public static void DebugCheckConditionsForUpgrade([StringConverter(typeof(MetaUpgrade.StringToMetaUpgradeIdConverter))] string upgradeId)
	{
		TPSingleton<DebugManager>.Instance.LogDevConsoleError("Targeting " + upgradeId + "... (Don't forget to refresh!)");
		MetaUpgradeController controller = TPSingleton<MetaUpgradesManager>.Instance.GetController(upgradeId);
		List<List<MetaConditionController>> list = controller?.MetaUpgrade?.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock);
		for (int i = 0; i < list.Count; i++)
		{
			TPSingleton<MetaConditionManager>.Instance.LogError($"Group {i} conditions:");
			DebugPrintConditions(list[i], controller);
		}
	}

	public static void DebugClearConditionControllers()
	{
		TPSingleton<MetaConditionManager>.Instance.metaConditionControllers.Clear();
	}

	[DevConsoleCommand(Name = "LocalizeMetaConditions")]
	public static void DebugLocalizeConditions([StringConverter(typeof(MetaUpgrade.StringToMetaUpgradeIdConverter))] string upgradeId)
	{
		List<MetaUpgrade> list = new List<MetaUpgrade>();
		list.AddRange(TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades);
		list.AddRange(TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades);
		list.AddRange(TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades);
		foreach (MetaUpgrade item in list)
		{
			if (item.MetaUpgradeDefinition.Id != upgradeId)
			{
				continue;
			}
			List<MetaConditionController> conditionControllers = new List<MetaConditionController>();
			item.GetConditions(MetaCondition.E_MetaConditionCategory.Activation)?.ForEach(delegate(List<MetaConditionController> o)
			{
				conditionControllers.AddRange(o);
			});
			item.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock)?.ForEach(delegate(List<MetaConditionController> o)
			{
				conditionControllers.AddRange(o);
			});
			{
				foreach (MetaConditionController item2 in conditionControllers)
				{
					TPSingleton<MetaConditionManager>.Instance.LogError(item2.LogLocalizedDescriptionBreakdown());
				}
				break;
			}
		}
	}

	[DevConsoleCommand(Name = "LocalizeAllMetaConditions")]
	public static void DebugLocalizeAllConditions()
	{
		List<MetaUpgrade> list = new List<MetaUpgrade>();
		list.AddRange(TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades);
		list.AddRange(TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades);
		list.AddRange(TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades);
		foreach (MetaUpgrade item in list)
		{
			List<MetaConditionController> conditionControllers = new List<MetaConditionController>();
			item.GetConditions(MetaCondition.E_MetaConditionCategory.Activation)?.ForEach(delegate(List<MetaConditionController> o)
			{
				conditionControllers.AddRange(o);
			});
			item.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock)?.ForEach(delegate(List<MetaConditionController> o)
			{
				conditionControllers.AddRange(o);
			});
			foreach (MetaConditionController item2 in conditionControllers)
			{
				TPSingleton<MetaConditionManager>.Instance.LogError(item2.LogLocalizedDescriptionBreakdown());
			}
		}
	}

	[DevConsoleCommand(Name = "RefreshMetaConditions")]
	public static void DebugRefreshConditions()
	{
		TPSingleton<MetaConditionManager>.Instance.LogError("Refreshing all condition progression...");
		TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		TPSingleton<MetaConditionManager>.Instance.LogError("Success!");
	}

	[DevConsoleCommand(Name = "MetaRenewRunContext")]
	public static void DebugRenewRunContext()
	{
		TPSingleton<MetaConditionManager>.Instance.RenewRunContext();
	}

	private static void DebugPrintConditions(IEnumerable<MetaConditionController> conditionControllers, MetaUpgradeController upgrade)
	{
		if (upgrade == null)
		{
			TPSingleton<MetaConditionManager>.Instance.LogError("No upgrades found!\nExisting upgrades: " + string.Join(", ", TPSingleton<MetaConditionManager>.Instance.metaConditionControllers.Select((MetaConditionController o) => o.MetaCondition.MetaUpgradeController.MetaUpgrade.MetaUpgradeDefinition.Id).Distinct()));
			return;
		}
		if (conditionControllers.Count() <= 0)
		{
			TPSingleton<MetaConditionManager>.Instance.LogError("Note: This upgrade has no conditions.");
		}
		foreach (MetaConditionController conditionController in conditionControllers)
		{
			string message = conditionController.MetaCondition.Id + (conditionController.MetaCondition.MetaConditionDefinition.Hidden ? "(Hidden)" : "") + ":\n" + $"{conditionController.IsComplete()} ({conditionController.GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary).ProgressionValueToString()}/{conditionController.GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary).GoalValueToString()}) ({conditionController.MetaCondition.OccurenceProgression}/{conditionController.MetaCondition.MetaConditionDefinition.Occurences})";
			TPSingleton<MetaConditionManager>.Instance.LogError(message);
		}
		TPSingleton<MetaConditionManager>.Instance.LogError("Status: " + (upgrade.AreUnlockConditionsFulfilled() ? "<color=green>UNLOCKED</color>" : "<color=red>LOCKED</color>"));
	}
}
