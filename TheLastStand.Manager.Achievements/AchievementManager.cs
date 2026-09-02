using System.Collections.Generic;
using System.Text;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.TileMap;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Achievements;

namespace TheLastStand.Manager.Achievements;

public sealed class AchievementManager : Manager<AchievementManager>
{
	private AAchievementHandler achievementHandler;

	private AchievementUnlocker crit20EnemiesTurnAchievementUnlocker;

	private AchievementUnlocker stun50EnemiesNightAchievementUnlocker;

	private AchievementUnlocker perfectWinAchievementUnlocker;

	private AchievementUnlocker destroy10CrystalsRunAchievementUnlocker;

	private AchievementUnlocker use50BeersRunAchievementUnlocker;

	private AchievementUnlocker crystalCorrupt5EnemiesNightAchievementUnlocker;

	private AchievementUnlocker drink10CoffeesRunAchievementUnlocker;

	private HashSet<string> remainingBuildingsToBuild;

	private HashSet<string> remainingWeaponsToUnlock;

	private HashSet<string> remainingDLC1WeaponsAndTrinketsToUnlock;

	private HashSet<string> remainingDLC2WeaponsToUnlock;

	private HashSet<Achievement> remainingAchievementsToUnlock;

	private bool isInitialized;

	public static bool IsAnyBoundlessModeActive
	{
		get
		{
			if (!TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
			{
				return TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive;
			}
			return true;
		}
	}

	private void Start()
	{
		InitIfNeeded();
	}

	public void CheckPlatinumAchievement()
	{
		HashSet<Achievement> hashSet = remainingAchievementsToUnlock;
		if (hashSet != null && hashSet.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_ALL_ACHIEVEMENTS);
		}
	}

	public int GetAchievementProgression(Stat stat)
	{
		if (achievementHandler == null)
		{
			return -1;
		}
		return achievementHandler.GetAchievementProgression(stat);
	}

	public void RefreshAchievements()
	{
		achievementHandler?.RefreshAchievements();
		CheckPlatinumAchievement();
	}

	public void SetAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		if (ApplicationManager.BuildType == ApplicationManager.E_BuildType.Release)
		{
			achievementHandler?.SetAchievementProgression(stat, value, refreshAchievements);
			if (refreshAchievements)
			{
				CheckPlatinumAchievement();
			}
		}
	}

	public void SetAchievementProgression(Stat stat, int value, bool refreshAchievements, int currentProgression)
	{
		if (ApplicationManager.BuildType == ApplicationManager.E_BuildType.Release && value != -1 && currentProgression < value)
		{
			SetAchievementProgression(stat, value, refreshAchievements);
		}
	}

	public void IncreaseAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		if (ApplicationManager.BuildType == ApplicationManager.E_BuildType.Release)
		{
			achievementHandler?.IncreaseAchievementProgression(stat, value, refreshAchievements);
			if (refreshAchievements)
			{
				CheckPlatinumAchievement();
			}
		}
	}

	public void UnlockAchievement(Achievement achievement, bool refreshIfAchieved = true, bool bypassBuildTypeCheck = false)
	{
		if (ApplicationManager.BuildType == ApplicationManager.E_BuildType.Release || bypassBuildTypeCheck)
		{
			achievementHandler?.UnlockAchievement(achievement, refreshIfAchieved);
			if (remainingAchievementsToUnlock != null && remainingAchievementsToUnlock.Remove(achievement) && refreshIfAchieved)
			{
				CheckPlatinumAchievement();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void InitIfNeeded()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			Init();
		}
	}

	private void Init()
	{
		achievementHandler = new SteamAchievementHandler();
		InitData();
	}

	public void InitData()
	{
		remainingBuildingsToBuild = new HashSet<string>();
		List<string> buildingsBuilt = TPSingleton<MetaConditionManager>.Instance.CampaignContext.BuildingsBuilt;
		foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
		{
			if (buildingDefinition.Value.ConstructionModuleDefinition.IsBuyable && !buildingsBuilt.Contains(buildingDefinition.Key))
			{
				remainingBuildingsToBuild.Add(buildingDefinition.Key);
			}
		}
		remainingWeaponsToUnlock = new HashSet<string>(AchievementContainer.UnlockAllWeapons);
		remainingDLC1WeaponsAndTrinketsToUnlock = new HashSet<string>(AchievementContainer.UnlockAllDLC1WeaponsAndTrinkets);
		remainingDLC2WeaponsToUnlock = new HashSet<string>(AchievementContainer.UnlockAllDLC2Weapons);
		foreach (MetaUpgrade activatedUpgrade in TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades)
		{
			remainingWeaponsToUnlock.Remove(activatedUpgrade.MetaUpgradeDefinition.Id);
			remainingDLC1WeaponsAndTrinketsToUnlock.Remove(activatedUpgrade.MetaUpgradeDefinition.Id);
			remainingDLC2WeaponsToUnlock.Remove(activatedUpgrade.MetaUpgradeDefinition.Id);
		}
		InitDataWithUnlockedAchievements();
		if (crit20EnemiesTurnAchievementUnlocker == null)
		{
			crit20EnemiesTurnAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_CRIT_20_ENEMIES_TURN, 20);
		}
		if (stun50EnemiesNightAchievementUnlocker == null)
		{
			stun50EnemiesNightAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_STUN_50_ENEMIES_NIGHT, 50);
		}
		if (perfectWinAchievementUnlocker == null)
		{
			perfectWinAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_PERFECT_WIN, 14);
		}
		if (destroy10CrystalsRunAchievementUnlocker == null)
		{
			destroy10CrystalsRunAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_DESTROY_10_CRYSTALS_RUN, 10);
		}
		if (use50BeersRunAchievementUnlocker == null)
		{
			use50BeersRunAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_USE_50_BEERS_RUN, 50);
		}
		if (crystalCorrupt5EnemiesNightAchievementUnlocker == null)
		{
			crystalCorrupt5EnemiesNightAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_5_ENEMIES_CORRUPTED_BY_CRYSTALS, 5);
		}
		if (drink10CoffeesRunAchievementUnlocker == null)
		{
			drink10CoffeesRunAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_DRINK_10_COFFEES_RUN, 10);
		}
	}

	public void InitDataWithUnlockedAchievements()
	{
		if (remainingAchievementsToUnlock == null)
		{
			remainingAchievementsToUnlock = new HashSet<Achievement>(AchievementContainer.AllAchievements);
		}
		if (achievementHandler == null)
		{
			return;
		}
		foreach (Achievement allAchievement in AchievementContainer.AllAchievements)
		{
			if (achievementHandler.IsAchievementUnlocked(allAchievement))
			{
				remainingAchievementsToUnlock.Remove(allAchievement);
			}
		}
	}

	public void TriggerApplicationBackwardCompatibility(int saveVersion)
	{
		InitIfNeeded();
		if (saveVersion <= 11)
		{
			SetAchievementProgression(StatContainer.STAT_ENEMIES_KILLED_AMOUNT, (int)TPSingleton<MetaConditionManager>.Instance.CampaignContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.Kills), refreshAchievements: false);
			SetAchievementProgression(StatContainer.STAT_POTION_USES_AMOUNT, (int)TPSingleton<MetaConditionManager>.Instance.CampaignContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.PotionsUsed), refreshAchievements: false);
			SetAchievementProgression(StatContainer.STAT_NUMBER_OF_RUNS, (int)ApplicationManager.Application.RunsCompleted, refreshAchievements: false);
			SetAchievementProgression(StatContainer.STAT_COMPLETED_GLYPHS_AMOUNT, TPSingleton<GlyphManager>.Instance.MaxApoPassedByCityByGlyph.Count, refreshAchievements: false);
			List<string> buildingsBuilt = TPSingleton<MetaConditionManager>.Instance.CampaignContext.BuildingsBuilt;
			if (buildingsBuilt.Contains("Inn"))
			{
				UnlockAchievement(AchievementContainer.ACH_FIRST_INN, refreshIfAchieved: false);
			}
			if (buildingsBuilt.Contains("Seer"))
			{
				UnlockAchievement(AchievementContainer.ACH_FIRST_SEER, refreshIfAchieved: false);
			}
			bool flag = false;
			foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
			{
				if (buildingDefinition.Value.ConstructionModuleDefinition.IsBuyable && !buildingsBuilt.Contains(buildingDefinition.Key))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				UnlockAchievement(AchievementContainer.ACH_BUILT_ALL_BUILDINGS, refreshIfAchieved: false);
			}
			SetAchievementProgression(StatContainer.STAT_SCAVENGED_CORPSES_AND_RUINS_AMOUNT, (int)TPSingleton<MetaConditionManager>.Instance.CampaignContext.ScavengedBonePilesCount);
		}
		if (saveVersion <= 12)
		{
			if (TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Count > 0)
			{
				UnlockAchievement(AchievementContainer.ACH_FIRST_UNLOCK, refreshIfAchieved: false);
			}
			int num = 0;
			foreach (MetaUpgrade activatedUpgrade in TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades)
			{
				if (AchievementContainer.UnlockAllWeapons.Contains(activatedUpgrade.MetaUpgradeDefinition.Id))
				{
					num++;
				}
			}
			if (num == AchievementContainer.UnlockAllWeapons.Count)
			{
				UnlockAchievement(AchievementContainer.ACH_UNLOCK_ALL_WEAPONS, refreshIfAchieved: false);
			}
		}
		RefreshAchievements();
	}

	public void TriggerGameBackwardCompatibility()
	{
		if ((int)TPSingleton<MetaConditionManager>.Instance.RunContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.ManaSpent) >= 500)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_SPEND_500_MANA_RUN, refreshIfAchieved: false);
		}
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana) == 0f)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_OUT_OF_MANA, refreshIfAchieved: false);
			}
		}
		if (TPSingleton<ResourceManager>.Instance.MaxWorkers >= 12)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_HAVE_12_WORKERS, refreshIfAchieved: false);
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count >= 6)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_HAVE_6_HEROES, refreshIfAchieved: false);
		}
		if (TPSingleton<DarkShopManager>.Instance.MetaShopView.GoddessView.CurrentEvolutionIndex > 0)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_SCHADEN_REVEAL, refreshIfAchieved: false);
		}
		if (TPSingleton<LightShopManager>.Instance.MetaShopView.GoddessView.CurrentEvolutionIndex > 0)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FREUDE_REVEAL, refreshIfAchieved: false);
		}
		if (TPSingleton<GameManager>.Instance.Game.DayNumber >= 4 && ApocalypseManager.CurrentApocalypseLevel >= 1 && !IsAnyBoundlessModeActive)
		{
			UnlockAchievement(AchievementContainer.ACH_NIGHT3_APO1);
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			int num = 0;
			foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
			{
				if (building.IsTrap && building.BattleModule.RemainingTrapCharges > 0)
				{
					num++;
				}
			}
			if (num >= 40)
			{
				UnlockAchievement(AchievementContainer.ACH_HAVE_40_TRAPS_NIGHT_BEGINNING);
			}
		}
		foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit2.LifetimeStats.MostUnitsKilledInOneBlow >= 12)
			{
				UnlockAchievement(AchievementContainer.ACH_HIT_12_ENEMIES_IN_AOE, refreshIfAchieved: false);
				break;
			}
		}
		if ((int)TPSingleton<MetaConditionManager>.Instance.RunContext.ItemsProducedCount >= 40)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_PRODUCE_40_BUILDING_ITEMS_RUN, refreshIfAchieved: false);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("Catapult") >= 10.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_10_CATAPULTS_RUN, refreshIfAchieved: false);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("StoneWallReinforced") >= 60.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_60_REINFORCED_STONE_WALLS_RUN, refreshIfAchieved: false);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfCategory(BuildingDefinition.E_BuildingCategory.Turret) >= 20.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_20_BALLISTAS_RUN, refreshIfAchieved: false);
		}
		RefreshAchievements();
	}

	public void HandleBuiltBuilding(string buildingId)
	{
		if (buildingId == "Inn")
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FIRST_INN);
		}
		else if (buildingId == "Seer")
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FIRST_SEER);
		}
		if (remainingBuildingsToBuild.Remove(buildingId) && remainingBuildingsToBuild.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILT_ALL_BUILDINGS);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("Catapult") >= 10.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_10_CATAPULTS_RUN);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfId("StoneWallReinforced") >= 60.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_60_REINFORCED_STONE_WALLS_RUN);
		}
		if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfCategory(BuildingDefinition.E_BuildingCategory.Turret) >= 20.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_20_BALLISTAS_RUN);
		}
		if (!GenericDatabase.IdsListDefinitions.TryGetValue("Seeds", out var value))
		{
			TPSingleton<AchievementManager>.Instance.LogError("Can not find the 'Seeds' id list, abort trying to unlock the " + AchievementContainer.ACH_BUILD_12_SEEDS_RUN.SteamId + " achievement.", CLogLevel.MAJOR);
		}
		else if (TPSingleton<MetaConditionManager>.Instance.RunContext.BuiltBuildingsOfIdsList(value) >= 12.0)
		{
			UnlockAchievement(AchievementContainer.ACH_BUILD_12_SEEDS_RUN);
		}
	}

	public void HandleDestroyedBuilding(TheLastStand.Model.Building.Building building, ISkillCaster attacker)
	{
		if (building.IsCrystal && attacker is PlayableUnit)
		{
			destroy10CrystalsRunAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleGameOver(Game.E_GameOverCause gameOverCause)
	{
		if (gameOverCause != Game.E_GameOverCause.MagicSealsCompleted)
		{
			return;
		}
		Achievement? achievement = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id switch
		{
			"Felderland" => AchievementContainer.ACH_WIN_GILDENBERG, 
			"LakeBurg" => AchievementContainer.ACH_WIN_LAKEBURG, 
			"Glenwald" => AchievementContainer.ACH_WIN_GLENWALD, 
			"Elderlicht" => AchievementContainer.ACH_WIN_ELDERLICHT, 
			"Glintfein" => AchievementContainer.ACH_WIN_GLINTFEIN, 
			"GildenbergRedux" => AchievementContainer.ACH_WIN_RUNENBERG, 
			"GlenwaldRedux" => AchievementContainer.ACH_WIN_AMBERWALD, 
			_ => null, 
		};
		if (achievement.HasValue)
		{
			UnlockAchievement(achievement.Value);
		}
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id == "GlenwaldRedux" && BuildingManager.GetBuildingAmountById("SacredBallista") >= 4)
		{
			UnlockAchievement(AchievementContainer.ACH_WIN_AMBERWALD_ALL_SACRED_BALLISTA);
		}
		if (IsAnyBoundlessModeActive)
		{
			return;
		}
		if (ApocalypseManager.CurrentApocalypseModifiersCount >= 1)
		{
			UnlockAchievement(AchievementContainer.ACH_WIN_APO1);
		}
		if (ApocalypseManager.CurrentApocalypseModifiersCount >= 3)
		{
			Achievement? achievement2 = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id switch
			{
				"Felderland" => AchievementContainer.ACH_WIN_GILDENBERG_APO3, 
				"LakeBurg" => AchievementContainer.ACH_WIN_LAKEBURG_APO3, 
				"Glenwald" => AchievementContainer.ACH_WIN_GLENWALD_APO3, 
				"Elderlicht" => AchievementContainer.ACH_WIN_ELDERLICHT_APO3, 
				"Glintfein" => AchievementContainer.ACH_WIN_GLINTFEIN_APO3, 
				"GildenbergRedux" => AchievementContainer.ACH_WIN_RUNENBERG_APO3, 
				"GlenwaldRedux" => AchievementContainer.ACH_WIN_AMBERWALD_APO3, 
				_ => null, 
			};
			if (achievement2.HasValue)
			{
				UnlockAchievement(achievement2.Value);
			}
		}
		if (ApocalypseManager.CurrentApocalypseModifiersCount >= 12)
		{
			Achievement? achievement3 = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id switch
			{
				"Felderland" => AchievementContainer.ACH_WIN_GILDENBERG_APO12, 
				"LakeBurg" => AchievementContainer.ACH_WIN_LAKEBURG_APO12, 
				"Glenwald" => AchievementContainer.ACH_WIN_GLENWALD_APO12, 
				"Elderlicht" => AchievementContainer.ACH_WIN_ELDERLICHT_APO12, 
				"Glintfein" => AchievementContainer.ACH_WIN_GLINTFEIN_APO12, 
				"GildenbergRedux" => AchievementContainer.ACH_WIN_RUNENBERG_APO12, 
				"GlenwaldRedux" => AchievementContainer.ACH_WIN_AMBERWALD_APO12, 
				_ => null, 
			};
			if (achievement3.HasValue)
			{
				UnlockAchievement(achievement3.Value);
			}
		}
		if (ApocalypseManager.CurrentApocalypseModifiersCount >= 6)
		{
			UnlockAchievement(AchievementContainer.ACH_WIN_APO6);
		}
		if (ApocalypseManager.CurrentApocalypseModifiersCount >= 12)
		{
			UnlockAchievement(AchievementContainer.ACH_WIN_APO12);
		}
		if (ApocalypseManager.CurrentApocalypseModifiersCount >= 16)
		{
			UnlockAchievement(AchievementContainer.ACH_WIN_APO16);
		}
	}

	public void HandleMetaUpgrade(string upgradeId)
	{
		UnlockAchievement(AchievementContainer.ACH_FIRST_UNLOCK);
		if (remainingWeaponsToUnlock != null && remainingWeaponsToUnlock.Remove(upgradeId) && remainingWeaponsToUnlock.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_UNLOCK_ALL_WEAPONS);
		}
		if (remainingDLC1WeaponsAndTrinketsToUnlock != null && remainingDLC1WeaponsAndTrinketsToUnlock.Remove(upgradeId) && remainingDLC1WeaponsAndTrinketsToUnlock.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_UNLOCK_ALL_DLC1_WEAPONS_TRINKETS);
		}
		if (remainingDLC2WeaponsToUnlock != null && remainingDLC2WeaponsToUnlock.Remove(upgradeId) && remainingDLC2WeaponsToUnlock.Count == 0)
		{
			UnlockAchievement(AchievementContainer.ACH_UNLOCK_ALL_DLC2_WEAPONS);
		}
	}

	public void HandleNightEnd()
	{
		stun50EnemiesNightAchievementUnlocker.Reset();
		crystalCorrupt5EnemiesNightAchievementUnlocker.Reset();
		if (TPSingleton<GameManager>.Instance.Game.DayNumber == 3 && ApocalypseManager.CurrentApocalypseLevel >= 1 && !IsAnyBoundlessModeActive)
		{
			UnlockAchievement(AchievementContainer.ACH_NIGHT3_APO1);
		}
		if (PanicManager.Panic.Level <= 1)
		{
			perfectWinAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleNightStart()
	{
		int num = 0;
		foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if (building.IsTrap && building.BattleModule.RemainingTrapCharges > 0)
			{
				num++;
			}
		}
		if (num >= 40)
		{
			UnlockAchievement(AchievementContainer.ACH_HAVE_40_TRAPS_NIGHT_BEGINNING);
		}
		if (TPSingleton<GameManager>.Instance.Game.DayNumber == TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxNightReached && TPSingleton<GameManager>.Instance.Game.DayNumber == TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount)
		{
			UnlockAchievement(AchievementContainer.ACH_REENCOUNTER_BOSS);
		}
	}

	public void HandleOnAttackDataComputed(AttackSkillActionExecutionTileData attackData)
	{
		if (attackData.IsCrit && attackData.Damageable is EnemyUnit)
		{
			crit20EnemiesTurnAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleRunLoad()
	{
		perfectWinAchievementUnlocker.SetValueLimit(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
	}

	public void HandleRunStart()
	{
		use50BeersRunAchievementUnlocker.Reset();
		destroy10CrystalsRunAchievementUnlocker.Reset();
		crystalCorrupt5EnemiesNightAchievementUnlocker.Reset();
		drink10CoffeesRunAchievementUnlocker.Reset();
		perfectWinAchievementUnlocker.Reset();
		perfectWinAchievementUnlocker.SetValueLimit(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
		int runsCompleted = (int)ApplicationManager.Application.RunsCompleted;
		SetAchievementProgression(StatContainer.STAT_NUMBER_OF_RUNS, runsCompleted, refreshAchievements: true, GetAchievementProgression(StatContainer.STAT_NUMBER_OF_RUNS));
	}

	public void HandleSkillEnd(SkillActionExecutionController skillActionExecutionController)
	{
		if (skillActionExecutionController.SkillActionExecution.Caster is PlayableUnit || skillActionExecutionController.SkillActionExecution.Caster is BattleModule)
		{
			int num = 0;
			foreach (TheLastStand.Model.Unit.Unit allAttackedUnit in skillActionExecutionController.SkillActionExecution.AllAttackedUnits)
			{
				if (allAttackedUnit is EnemyUnit)
				{
					num++;
				}
			}
			if (num >= 12)
			{
				UnlockAchievement(AchievementContainer.ACH_HIT_12_ENEMIES_IN_AOE);
			}
		}
		if (skillActionExecutionController.SkillActionExecution.Caster is PlayableUnit playableUnit)
		{
			foreach (TheLastStand.Model.Unit.Unit allAttackedUnit2 in skillActionExecutionController.SkillActionExecution.AllAttackedUnits)
			{
				if (allAttackedUnit2 is EnemyUnit { IsDead: not false } && TileMapController.DistanceBetweenTiles(allAttackedUnit2.OriginTile, playableUnit.OriginTile) >= 14)
				{
					UnlockAchievement(AchievementContainer.ACH_KILL_14_TILES);
					break;
				}
			}
		}
		if (skillActionExecutionController.SkillActionExecution.Skill.Id == "ElvenCoffee")
		{
			drink10CoffeesRunAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleStatusAdded(TheLastStand.Model.Unit.Unit unit, Status status)
	{
		if (unit is EnemyUnit && status is StunStatus)
		{
			stun50EnemiesNightAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandlePotionUsed(TheLastStand.Model.Item.Item item, Tile targetTile)
	{
		if (item.IsBeer)
		{
			use50BeersRunAchievementUnlocker.IncreaseValue();
		}
	}

	public void HandleCrystalCorruptedEnemy()
	{
		crystalCorrupt5EnemiesNightAchievementUnlocker.IncreaseValue();
	}

	public void HandleTurnStart()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			crit20EnemiesTurnAchievementUnlocker.Reset();
		}
	}

	public void Deserialize(SerializedAchievements serializedAchievements)
	{
		if (crit20EnemiesTurnAchievementUnlocker == null)
		{
			crit20EnemiesTurnAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_CRIT_20_ENEMIES_TURN, 20);
		}
		crit20EnemiesTurnAchievementUnlocker.Deserialize(serializedAchievements?.Crit20EnemiesTurnAchievementUnlocker);
		if (stun50EnemiesNightAchievementUnlocker == null)
		{
			stun50EnemiesNightAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_STUN_50_ENEMIES_NIGHT, 50);
		}
		stun50EnemiesNightAchievementUnlocker.Deserialize(serializedAchievements?.Stun50EnemiesNightAchievementUnlocker);
		if (perfectWinAchievementUnlocker == null)
		{
			perfectWinAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_PERFECT_WIN, 14);
		}
		perfectWinAchievementUnlocker.Deserialize(serializedAchievements?.PerfectWinAchievementUnlocker);
		if (destroy10CrystalsRunAchievementUnlocker == null)
		{
			destroy10CrystalsRunAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_DESTROY_10_CRYSTALS_RUN, 10);
		}
		destroy10CrystalsRunAchievementUnlocker.Deserialize(serializedAchievements?.Destroy10CrystalsRunAchievementUnlocker);
		if (use50BeersRunAchievementUnlocker == null)
		{
			use50BeersRunAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_USE_50_BEERS_RUN, 50);
		}
		use50BeersRunAchievementUnlocker.Deserialize(serializedAchievements?.Use50BeersRunAchievementUnlocker);
		if (crystalCorrupt5EnemiesNightAchievementUnlocker == null)
		{
			crystalCorrupt5EnemiesNightAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_5_ENEMIES_CORRUPTED_BY_CRYSTALS, 5);
		}
		crystalCorrupt5EnemiesNightAchievementUnlocker.Deserialize(serializedAchievements?.CrystalCorrupt5EnemiesNightAchievementUnlocker);
		if (drink10CoffeesRunAchievementUnlocker == null)
		{
			drink10CoffeesRunAchievementUnlocker = new AchievementUnlocker(AchievementContainer.ACH_DRINK_10_COFFEES_RUN, 10);
		}
		drink10CoffeesRunAchievementUnlocker.Deserialize(serializedAchievements?.Drink10CoffeesRunAchievementUnlocker);
	}

	public SerializedAchievements Serialize()
	{
		return new SerializedAchievements
		{
			Crit20EnemiesTurnAchievementUnlocker = (crit20EnemiesTurnAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			Stun50EnemiesNightAchievementUnlocker = (stun50EnemiesNightAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			PerfectWinAchievementUnlocker = (perfectWinAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			Destroy10CrystalsRunAchievementUnlocker = (destroy10CrystalsRunAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			Use50BeersRunAchievementUnlocker = (use50BeersRunAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			CrystalCorrupt5EnemiesNightAchievementUnlocker = (crystalCorrupt5EnemiesNightAchievementUnlocker.Serialize() as SerializedAchievementUnlocker),
			Drink10CoffeesRunAchievementUnlocker = (drink10CoffeesRunAchievementUnlocker.Serialize() as SerializedAchievementUnlocker)
		};
	}

	private void OnGoGAchievementHandlerReady()
	{
		InitDataWithUnlockedAchievements();
		RefreshAchievements();
	}

	[DevConsoleCommand("AchievementUnlock")]
	public static void UnlockAchievementDebug([StringConverter(typeof(Achievement.StringToAchievementIdConverter))] string achievementId)
	{
		TPSingleton<AchievementManager>.Instance.achievementHandler?.UnlockAchievement(achievementId);
	}

	[DevConsoleCommand("AchievementUnlockAllButOne")]
	public static void UnlockAllAchievementButOneDebug()
	{
		foreach (Achievement allAchievement in AchievementContainer.AllAchievements)
		{
			if (allAchievement.SteamId != AchievementContainer.ACH_USE_50_SCROLLS.SteamId)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(allAchievement, refreshIfAchieved: true, bypassBuildTypeCheck: true);
			}
		}
	}

	[DevConsoleCommand("AchievementReset")]
	public static void ResetAchievementDebug()
	{
		TPSingleton<AchievementManager>.Instance.achievementHandler?.ClearAchievements();
		TPSingleton<AchievementManager>.Instance.Log("Achievements cleared !", CLogLevel.DETAILED, forcePrintInUnity: true);
	}

	[DevConsoleCommand("AchievementDisplayRemainingAchievementsToUnlock")]
	public static void DisplayRemainingAchievementsToUnlock()
	{
		TPSingleton<DebugManager>.Instance.LogDevConsole($"RemainingAchievementsToUnlock nb: {TPSingleton<AchievementManager>.Instance.remainingAchievementsToUnlock.Count}");
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Achievement item in TPSingleton<AchievementManager>.Instance.remainingAchievementsToUnlock)
		{
			stringBuilder.Append(item.SteamId).AppendLine();
		}
		TPSingleton<DebugManager>.Instance.LogDevConsole(stringBuilder.ToString());
	}

	[DevConsoleCommand("AchievementIsUnlocked")]
	public static void IsAchievementUnlocked([StringConverter(typeof(Achievement.StringToAchievementIdConverter))] string achievementId)
	{
		bool flag = false;
		foreach (Achievement allAchievement in AchievementContainer.AllAchievements)
		{
			if (allAchievement.SteamId == achievementId)
			{
				flag = TPSingleton<AchievementManager>.Instance.achievementHandler.IsAchievementUnlocked(allAchievement);
			}
		}
		TPSingleton<DebugManager>.Instance.LogDevConsole($"IsAchievementUnlocked({achievementId}) : {flag}");
	}
}
