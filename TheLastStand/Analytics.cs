using System.Collections.Generic;
using System.Linq;
using NaconAPI;
using Rewired;
using Steamworks;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.AnalyticsEventsData;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand;

public static class Analytics
{
	public static class AnalyticsEventTypes
	{
		public const string ACHIEVEMENT_UNLOCKED = "achievement";

		public const string BOSS_DEATH = "boss_death";

		public const string BOSS_SPAWN = "vulnerability_boss";

		public const string BUILDING_GOLD_PRODUCTION = "gold_building";

		public const string BUILDING_MATERIAL_PRODUCTION = "material_building";

		public const string BUILDING_UPGRADE_BOUGHT = "purchase_building";

		public const string END_RUN = "end_run";

		public const string GAME_START = "game_start";

		public const string HERO_DEATH = "death_hero";

		public const string HERO_STAT_LEVEL_UP = "hero_stat_level_up";

		public const string PERK_UNLOCK = "perk_unlock";

		public const string SALE_ITEM = "sale_item";

		public const string START_RUN = "start_run";

		public const string END_DAY = "end_day";

		public const string END_NIGHT = "end_night";
	}

	public static class Constants
	{
		public static readonly string[] SUPPORTED_BRANCHES_TO_SEND_DATA = new string[1] { "closed_beta" };

		public const string STATUS_ABANDON = "abandon";

		public const string STATUS_FAIL = "fail";

		public const string STATUS_WIN = "win";

		public const string INPUT_CONTROLLER_JOYSTICK = "Joystick";

		public const string INPUT_CONTROLLER_KEYBOARD = "Keyboard";
	}

	private static HashSet<string> storedUnlockedAchievements = new HashSet<string>();

	private static MapDayData currentMapDayData;

	private static MapNightData currentMapNightData;

	public static bool AllowedToSendData
	{
		get
		{
			if (SettingsManager.HasBeenInitialized && TPSingleton<SettingsManager>.Instance.Settings.AllowDataCollection)
			{
				return NaconAPIHandler.IsInitialized;
			}
			return false;
		}
	}

	public static string AnalyticsDebugLog { get; private set; }

	public static MapDayData CurrentMapDayData => currentMapDayData ?? GenerateMapDayData();

	public static MapNightData CurrentMapNightData => currentMapNightData ?? GenerateMapNightData();

	public static MapDayData GenerateMapDayData()
	{
		string id = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		currentMapDayData = new MapDayData(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.RunId, id, ApocalypseManager.CurrentApocalypseLevel, TPSingleton<GameManager>.Instance.Game.DayNumber);
		return currentMapDayData;
	}

	public static MapNightData GenerateMapNightData()
	{
		if (currentMapDayData == null)
		{
			GenerateMapDayData();
		}
		string waveName = SpawnWaveManager.CurrentSpawnWave?.SpawnWaveDefinition.Id ?? SpawnWaveManager.LastPlayedSpawnWaveId;
		int turnNumber = ((TPSingleton<GameManager>.Instance.Game.CurrentNightHour > 0) ? TPSingleton<GameManager>.Instance.Game.CurrentNightHour : TPSingleton<GameManager>.Instance.Game.LastNightLastHour);
		currentMapNightData = new MapNightData(CurrentMapDayData.RunId, CurrentMapDayData.MapName, CurrentMapDayData.ApocalypseLevel, CurrentMapDayData.DayNumber, waveName, turnNumber);
		return currentMapNightData;
	}

	public static void InitializeAfterDLCManager()
	{
		if (!SettingsManager.HasBeenInitialized)
		{
			TPSingleton<SettingsManager>.Instance.OnSettingsDeserializedEvent += OnSettingsManagerInitialized;
		}
		if (!NaconAPIHandler.IsInitialized && !DLCManager.Initialized)
		{
			DLCManager.OnInitialized += OnDLCManagerInitialized;
			return;
		}
		if (!NaconAPIHandler.IsInitialized)
		{
			InitializeNaconAPIHandler();
		}
		if (AllowedToSendData)
		{
			SendGameStartEvent();
		}
	}

	public static void SendBossDeathEvent(string bossName)
	{
		NaconAPIHandler.Analytics.SendEventAsync("boss_death", new BossDeathData(CurrentMapNightData, bossName));
	}

	public static void SendDeathHeroEvent(string heroId, string skillName, string killerName)
	{
		NaconAPIHandler.Analytics.SendEventAsync("death_hero", new DeathHeroData(CurrentMapNightData, heroId, skillName, killerName));
	}

	public static void SendEndRunEvent(Game.E_GameOverCause gameOverReason)
	{
		string[] activeWeaponFamiliesForAnalytics = TPSingleton<ItemRestrictionManager>.Instance.GetActiveWeaponFamiliesForAnalytics();
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day;
		string status = gameOverReason switch
		{
			Game.E_GameOverCause.Abandon => "abandon", 
			Game.E_GameOverCause.MagicSealsCompleted => "win", 
			_ => "fail", 
		};
		string waveName = string.Empty;
		int turnNumberMax = 0;
		MapData mapData = new MapData(CurrentMapDayData.RunId, CurrentMapDayData.MapName, CurrentMapDayData.ApocalypseLevel);
		int dayNumber;
		if (!flag)
		{
			waveName = SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.Id;
			turnNumberMax = CurrentMapNightData.TurnNumber;
			dayNumber = currentMapNightData.DayNumber;
		}
		else
		{
			dayNumber = currentMapDayData.DayNumber;
		}
		NaconAPIHandler.Analytics.SendEventAsync("end_run", new EndRunData(mapData, waveName, dayNumber, turnNumberMax, activeWeaponFamiliesForAnalytics, status, gameOverReason.ToString()));
	}

	public static void SendEndDayEvent()
	{
		EndDayData eventData = new EndDayData(currentMapDayData, TPSingleton<GameManager>.Instance.GameAnalytics, TPSingleton<PlayableUnitManager>.Instance.PlayableUnits);
		NaconAPIHandler.Analytics.SendEventAsync("end_day", eventData);
	}

	public static void SendEndNightEvent(int generatedBonePileCount)
	{
		int gold = PanicManager.Panic.PanicReward.Gold;
		int materials = PanicManager.Panic.PanicReward.Materials;
		int tonightRank = TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightRank;
		bool hasBeenRepelled = TPSingleton<FogManager>.Instance.Fog.HasBeenRepelled;
		EndNightData eventData = new EndNightData(CurrentMapNightData, gold, materials, tonightRank, hasBeenRepelled, TPSingleton<GameManager>.Instance.GameAnalytics, TPSingleton<PlayableUnitManager>.Instance.PlayableUnits, generatedBonePileCount);
		NaconAPIHandler.Analytics.SendEventAsync("end_night", eventData);
	}

	public static void SendGoldBuildingEvent(int producedGoldAmount)
	{
		NaconAPIHandler.Analytics.SendEventAsync("gold_building", new GoldBuildingData(CurrentMapDayData, producedGoldAmount));
	}

	public static void SendHeroStatLevelUpEvent(string heroId, string statName, string statLevelUpRarity, bool isMainStat)
	{
		NaconAPIHandler.Analytics.SendEventAsync("hero_stat_level_up", new HeroStatLevelUpData(CurrentMapDayData, heroId, statName, statLevelUpRarity, isMainStat));
	}

	public static void SendMaterialBuildingEvent(int producedMaterialAmount)
	{
		NaconAPIHandler.Analytics.SendEventAsync("material_building", new MaterialBuildingData(CurrentMapDayData, producedMaterialAmount));
	}

	public static void SendPerkUnlockEvent(string heroId, string perkName)
	{
		NaconAPIHandler.Analytics.SendEventAsync("perk_unlock", new PerkUnlockData(CurrentMapDayData, heroId, perkName));
	}

	public static void SendPurchaseBuildingEvent(string buildingType, string buildingName, string upgradeName)
	{
		NaconAPIHandler.Analytics.SendEventAsync("purchase_building", new PurchaseBuildingData(CurrentMapDayData, buildingType, buildingName, upgradeName));
	}

	public static void SendSaleItemEvent(string itemName, int earnedGold)
	{
		NaconAPIHandler.Analytics.SendEventAsync("sale_item", new SaleItemData(currentMapDayData, itemName, earnedGold));
	}

	public static void SendStartRunEvent(string runId, string mapName, int apocalypseLevel, string[] usedOmens, string[] activeWeaponsFamilies, string[] excludedWeaponsFamilies)
	{
		MapData mapData = new MapData(runId, mapName, apocalypseLevel);
		int darkMetaUnlockedNb = TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Count((MetaUpgrade metaUpgrade) => metaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop);
		int lightMetaUnlockedNb = TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Count((MetaUpgrade metaUpgrade) => !metaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop);
		NaconAPIHandler.Analytics.SendEventAsync("start_run", new StartRunData(mapData, usedOmens, activeWeaponsFamilies, excludedWeaponsFamilies, darkMetaUnlockedNb, lightMetaUnlockedNb));
	}

	public static void SendVulnerabilityBossEvent(string bossName)
	{
		NaconAPIHandler.Analytics.SendEventAsync("vulnerability_boss", new VulnerabilityBossData(CurrentMapNightData, bossName));
	}

	public static void StoreUnlockedAchievementIfNeeded(string achievementName)
	{
		if (!NaconAPIHandler.IsInitialized || !SettingsManager.HasBeenInitialized)
		{
			storedUnlockedAchievements.Add(achievementName);
		}
	}

	public static void TrySendAchievementUnlockedEvent(string achievementName)
	{
		StoreUnlockedAchievementIfNeeded(achievementName);
		if (AllowedToSendData)
		{
			SendAchievementUnlockedEvent(achievementName);
		}
	}

	private static string GetPlatform()
	{
		return "PC";
	}

	private static string GetPlatformUserId()
	{
		return SteamUser.GetSteamID().ToString();
	}

	private static void InitializeNaconAPIHandler()
	{
		bool flag = false;
		flag = true;
		if (flag && SteamApps.GetCurrentBetaName(out var pchName, 128))
		{
			bool flag2 = false;
			string[] sUPPORTED_BRANCHES_TO_SEND_DATA = Constants.SUPPORTED_BRANCHES_TO_SEND_DATA;
			for (int i = 0; i < sUPPORTED_BRANCHES_TO_SEND_DATA.Length; i++)
			{
				if (sUPPORTED_BRANCHES_TO_SEND_DATA[i] == pchName)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				flag = false;
			}
		}
		AnalyticsDebugLog = "Analytics destination: ";
		if (flag)
		{
			AnalyticsDebugLog += "release";
			NaconAPIHandler.Init("https://nacon-os.com/v2", "g3ivky9iXU", "STEAM", GetPlatformUserId(), "9tw1Nu3f9LCttEYpXsvFnFuMTxT0p74x", "the-last-spell", "ishtar-the-last-spell", GetPlatform(), ApplicationManager.VersionString);
		}
		else
		{
			AnalyticsDebugLog += "dev";
			NaconAPIHandler.Init("https://nacon-os-rec-v2-54f75zaw5q-ew.a.run.app/v2", "g3ivky9iXU", "STEAM", GetPlatformUserId(), "ENV-TOKEN-REC", "the-last-spell-dev", "ishtar-the-last-spell", GetPlatform(), ApplicationManager.VersionString);
		}
	}

	private static void OnDLCManagerInitialized()
	{
		DLCManager.OnInitialized -= OnDLCManagerInitialized;
		if (!NaconAPIHandler.IsInitialized)
		{
			InitializeNaconAPIHandler();
		}
		if (AllowedToSendData)
		{
			SendGameStartEvent();
		}
	}

	private static void OnSettingsManagerInitialized()
	{
		TPSingleton<SettingsManager>.Instance.OnSettingsDeserializedEvent -= OnSettingsManagerInitialized;
		if (NaconAPIHandler.IsInitialized && AllowedToSendData)
		{
			SendGameStartEvent();
		}
	}

	private static void SendAchievementUnlockedEvent(string achievementName)
	{
		NaconAPIHandler.Analytics.SendEventAsync("achievement", new AchievementData(achievementName));
	}

	private static void SendGameStartEvent()
	{
		if (!NaconAPIHandler.IsInitialized)
		{
			InitializeNaconAPIHandler();
		}
		string inputController = "Keyboard";
		if (TPSingleton<TheLastStand.Manager.InputManager>.Instance != null && TPSingleton<TheLastStand.Manager.InputManager>.Instance.Player != null)
		{
			Player.ControllerHelper controllers = TPSingleton<TheLastStand.Manager.InputManager>.Instance.Player.controllers;
			if (controllers != null && controllers.joystickCount > 0)
			{
				inputController = "Joystick";
			}
		}
		NaconAPIHandler.Analytics.SendEventAsync("game_start", new GameStartData(SystemInfo.graphicsDeviceName, SystemInfo.processorType, inputController, Localizer.language, UnityEngine.Application.systemLanguage.ToString(), TPSingleton<DLCManager>.Instance.OwnedDLCIds.ToArray()));
		if (storedUnlockedAchievements.Count <= 0)
		{
			return;
		}
		foreach (string storedUnlockedAchievement in storedUnlockedAchievements)
		{
			SendAchievementUnlockedEvent(storedUnlockedAchievement);
		}
		storedUnlockedAchievements.Clear();
	}

	private static PlayableUnit Debug_GetRandomPlayableUnit()
	{
		return TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.RandomElement();
	}

	[DevConsoleCommand("AnalyticsSend_AchievementUnlocked")]
	public static void Debug_AnalyticsSend_AchievementUnlocked()
	{
		SendAchievementUnlockedEvent(AchievementContainer.AllAchievements.PickRandom().SteamId);
	}

	[DevConsoleCommand("AnalyticsSend_BossDeath")]
	public static void Debug_AnalyticsSend_BossDeath()
	{
		SendBossDeathEvent(BossUnitDatabase.BossUnitTemplateDefinitions.PickRandom().Value.Id);
	}

	[DevConsoleCommand("AnalyticsSend_VulnerabilityBoss")]
	public static void Debug_AnalyticsSend_VulnerabilityBoss()
	{
		SendVulnerabilityBossEvent(BossUnitDatabase.BossUnitTemplateDefinitions.PickRandom().Value.Id);
	}

	[DevConsoleCommand("AnalyticsSend_GoldBuilding")]
	public static void Debug_AnalyticsSend_GoldBuilding()
	{
		SendGoldBuildingEvent(UnityEngine.Random.Range(1, 1001));
	}

	[DevConsoleCommand("AnalyticsSend_MaterialBuilding")]
	public static void Debug_AnalyticsSend_MaterialBuilding()
	{
		SendMaterialBuildingEvent(UnityEngine.Random.Range(1, 1001));
	}

	[DevConsoleCommand("AnalyticsSend_PurchaseBuilding")]
	public static void Debug_AnalyticsSend_PurchaseBuilding()
	{
		BuildingDefinition value = BuildingDatabase.BuildingDefinitions.PickRandom().Value;
		string buildingType = value.BlueprintModuleDefinition.Category.ToString();
		string id = value.Id;
		string upgradeName = value.UpgradeModuleDefinition?.BuildingUpgradeDefinitions?.PickRandom()?.Id;
		SendPurchaseBuildingEvent(buildingType, id, upgradeName);
	}

	[DevConsoleCommand("AnalyticsSend_EndRun")]
	public static void Debug_AnalyticsSend_EndRun()
	{
		SendEndRunEvent((Game.E_GameOverCause)UnityEngine.Random.Range(1, 5));
	}

	[DevConsoleCommand("AnalyticsSend_GameStart")]
	public static void Debug_AnalyticsSend_GameStart()
	{
		SendGameStartEvent();
	}

	[DevConsoleCommand("AnalyticsSend_DeathHero")]
	public static void Debug_AnalyticsSend_DeathHero()
	{
		string analyticsIdentifier = Debug_GetRandomPlayableUnit().AnalyticsIdentifier;
		string id = SkillDatabase.SkillDefinitions.PickRandom().Value.Id;
		string id2 = EnemyUnitDatabase.EnemyUnitTemplateDefinitions.PickRandom().Value.Id;
		SendDeathHeroEvent(analyticsIdentifier, id, id2);
	}

	[DevConsoleCommand("AnalyticsSend_HeroStatLevelUp")]
	public static void Debug_AnalyticsSend_HeroStatLevelUp()
	{
		string analyticsIdentifier = Debug_GetRandomPlayableUnit().AnalyticsIdentifier;
		UnitStatDefinition.E_Stat key = (UnitStatDefinition.E_Stat)UnityEngine.Random.Range(0, 54);
		UnitLevelUp.E_StatLevelUpRarity e_StatLevelUpRarity = (UnitLevelUp.E_StatLevelUpRarity)UnityEngine.Random.Range(0, 3);
		SendHeroStatLevelUpEvent(isMainStat: PlayableUnitDatabase.UnitLevelUpMainStatDefinitions.ContainsKey(key), heroId: analyticsIdentifier, statName: key.ToString(), statLevelUpRarity: e_StatLevelUpRarity.ToString());
	}

	[DevConsoleCommand("AnalyticsSend_PerkUnlock")]
	public static void Debug_AnalyticsSend_PerkUnlock()
	{
		string analyticsIdentifier = Debug_GetRandomPlayableUnit().AnalyticsIdentifier;
		string id = PlayableUnitDatabase.PerkDefinitions.PickRandom().Value.Id;
		SendPerkUnlockEvent(analyticsIdentifier, id);
	}

	[DevConsoleCommand("AnalyticsSend_SaleItem")]
	public static void Debug_AnalyticsSend_SaleItem()
	{
		ItemDefinition value = ItemDatabase.AllItemsDefinitions.PickRandom().Value;
		string id = value.Id;
		Dictionary<int, float> basePriceByLevel = value.BasePriceByLevel;
		int earnedGold = (int)((basePriceByLevel != null && basePriceByLevel.Count > 0) ? value.BasePriceByLevel.PickRandom().Value : 0f);
		SendSaleItemEvent(id, earnedGold);
	}

	[DevConsoleCommand("AnalyticsSend_StartRun")]
	public static void Debug_AnalyticsSend_StartRun()
	{
		string id = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		string runId = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.RunId;
		int currentApocalypseLevel = ApocalypseManager.CurrentApocalypseLevel;
		string[] usedOmens = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Select((GlyphDefinition glyphDefinition) => glyphDefinition.Id).ToArray();
		string[] activeWeaponFamiliesForAnalytics = TPSingleton<ItemRestrictionManager>.Instance.GetActiveWeaponFamiliesForAnalytics();
		string[] excludedWeaponFamiliesForAnalytics = TPSingleton<ItemRestrictionManager>.Instance.GetExcludedWeaponFamiliesForAnalytics();
		SendStartRunEvent(runId, id, currentApocalypseLevel, usedOmens, activeWeaponFamiliesForAnalytics, excludedWeaponFamiliesForAnalytics);
	}

	[DevConsoleCommand("AnalyticsSend_EndDay")]
	public static void Debug_AnalyticsSend_EndDay()
	{
		SendEndDayEvent();
	}

	[DevConsoleCommand("AnalyticsSend_EndNight")]
	public static void Debug_AnalyticsSend_EndNight()
	{
		SendEndNightEvent(UnityEngine.Random.Range(0, 11));
	}

	[DevConsoleCommand("AnalyticsSend_All")]
	public static void Debug_AnalyticsSend_All()
	{
		Debug_AnalyticsSend_AchievementUnlocked();
		Debug_AnalyticsSend_BossDeath();
		Debug_AnalyticsSend_VulnerabilityBoss();
		Debug_AnalyticsSend_GoldBuilding();
		Debug_AnalyticsSend_MaterialBuilding();
		Debug_AnalyticsSend_PurchaseBuilding();
		Debug_AnalyticsSend_EndRun();
		Debug_AnalyticsSend_GameStart();
		Debug_AnalyticsSend_DeathHero();
		Debug_AnalyticsSend_HeroStatLevelUp();
		Debug_AnalyticsSend_PerkUnlock();
		Debug_AnalyticsSend_SaleItem();
		Debug_AnalyticsSend_StartRun();
		Debug_AnalyticsSend_EndDay();
		Debug_AnalyticsSend_EndNight();
	}
}
