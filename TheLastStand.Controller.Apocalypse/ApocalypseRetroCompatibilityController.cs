using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Model.WorldMap;

namespace TheLastStand.Controller.Apocalypse;

public static class ApocalypseRetroCompatibilityController
{
	private static bool isInitialized;

	private static List<ApocalypseRetroCompatibilityLevelEquivalence> apocalypseLevelsEquivalences = new List<ApocalypseRetroCompatibilityLevelEquivalence>();

	public static void ApplyRetroCompatibilityToGameSaveApocalypse(int saveVersion, int apocalypseLevel)
	{
		if (saveVersion > 23)
		{
			return;
		}
		ApocalypseRetroCompatibilityLevelEquivalence apocalypseLevelEquivalence = GetApocalypseLevelEquivalence(apocalypseLevel);
		if (apocalypseLevelEquivalence != null)
		{
			if (TPSingleton<ApocalypseManager>.Exist())
			{
				TPSingleton<ApocalypseManager>.Instance.Log($"Trying to apply apocalypse retro compatibility to selected apocalypse modifiers on apocalypse level '{apocalypseLevel}' with save version {saveVersion} !", CLogLevel.MAJOR, forcePrintInUnity: true);
				TPSingleton<ApocalypseManager>.Instance.Log(apocalypseLevelEquivalence.ToString(), CLogLevel.MAJOR, forcePrintInUnity: true);
			}
			ApocalypseManager.SetApocalypse(GetApocalypseModifierStepDefinitionsFromLevelEquivalence(apocalypseLevelEquivalence), computeLevel: true, computeEffects: false);
		}
	}

	public static void ApplyRetroCompatibilityToWorldMapCity(int saveVersion, WorldMapCity worldMapCity)
	{
		if (saveVersion > 13)
		{
			return;
		}
		ApocalypseRetroCompatibilityLevelEquivalence apocalypseLevelEquivalence = GetApocalypseLevelEquivalence(worldMapCity.MaxApocalypsePassed);
		if (apocalypseLevelEquivalence == null)
		{
			return;
		}
		int maxApocalypsePassed = worldMapCity.MaxApocalypsePassed;
		int correspondingLevel = apocalypseLevelEquivalence.CorrespondingLevel;
		if (TPSingleton<ApocalypseManager>.Exist())
		{
			TPSingleton<ApocalypseManager>.Instance.Log($"Trying to apply apocalypse retro compatibility to city '{worldMapCity.CityDefinition.Id}' with save version {saveVersion} ! (from apocalypse level {maxApocalypsePassed} to {correspondingLevel})", CLogLevel.MAJOR, forcePrintInUnity: true);
			TPSingleton<ApocalypseManager>.Instance.Log(apocalypseLevelEquivalence.ToString(), CLogLevel.DETAILED, forcePrintInUnity: true);
		}
		worldMapCity.MaxApocalypsePassed = correspondingLevel;
		foreach (ApocalypseModifierIdAndStepIndex item in apocalypseLevelEquivalence.CorrespondingModifiersStep)
		{
			worldMapCity.WorldMapCityController.TryAddingCompletedApocalypseModifierStep(item.ModifierId, item.StepIndex);
		}
	}

	public static ApocalypseRetroCompatibilityLevelEquivalence GetApocalypseLevelEquivalence(int oldApocalypseLevel)
	{
		InitializeIfNeeded();
		bool flag = false;
		ApocalypseRetroCompatibilityLevelEquivalence apocalypseRetroCompatibilityLevelEquivalence = new ApocalypseRetroCompatibilityLevelEquivalence(oldApocalypseLevel, new List<ApocalypseModifierIdAndStepIndex>());
		for (int i = 0; i < apocalypseLevelsEquivalences.Count; i++)
		{
			ApocalypseRetroCompatibilityLevelEquivalence apocalypseRetroCompatibilityLevelEquivalence2 = apocalypseLevelsEquivalences[i];
			if (apocalypseRetroCompatibilityLevelEquivalence2.OldSystemLevel <= oldApocalypseLevel)
			{
				apocalypseRetroCompatibilityLevelEquivalence.CorrespondingLevel += apocalypseRetroCompatibilityLevelEquivalence2.CorrespondingLevel;
				apocalypseRetroCompatibilityLevelEquivalence.CorrespondingModifiersStep.AddRange(apocalypseRetroCompatibilityLevelEquivalence2.CorrespondingModifiersStep);
				flag = true;
			}
		}
		if (flag)
		{
			return apocalypseRetroCompatibilityLevelEquivalence;
		}
		return null;
	}

	public static List<ApocalypseModifierStepDefinition> GetApocalypseModifierStepDefinitionsFromLevelEquivalence(ApocalypseRetroCompatibilityLevelEquivalence levelEquivalence)
	{
		List<ApocalypseModifierStepDefinition> list = new List<ApocalypseModifierStepDefinition>();
		if (levelEquivalence == null)
		{
			return list;
		}
		foreach (ApocalypseModifierIdAndStepIndex item in levelEquivalence.CorrespondingModifiersStep)
		{
			if (ApocalypseDatabase.ModifierDefinitions.TryGetValue(item.ModifierId, out var value))
			{
				if (item.StepIndex < value.StepDefinitions.Count)
				{
					list.Add(value.StepDefinitions[item.StepIndex]);
				}
				else
				{
					list.Add(value.StepDefinitions[^1]);
				}
			}
		}
		return list;
	}

	private static void InitializeIfNeeded()
	{
		if (!isInitialized)
		{
			apocalypseLevelsEquivalences.Clear();
			ApocalypseRetroCompatibilityLevelEquivalence item = new ApocalypseRetroCompatibilityLevelEquivalence(1, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("EnemyHealthModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			item = new ApocalypseRetroCompatibilityLevelEquivalence(2, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("WaveSizeModifier", 1)));
			apocalypseLevelsEquivalences.Add(item);
			item = new ApocalypseRetroCompatibilityLevelEquivalence(3, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("FogSpawnerModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			item = new ApocalypseRetroCompatibilityLevelEquivalence(4, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("ProductionCostModifier", 1), new ApocalypseModifierIdAndStepIndex("DefenseCostModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			item = new ApocalypseRetroCompatibilityLevelEquivalence(5, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("FasterEnemiesModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			item = new ApocalypseRetroCompatibilityLevelEquivalence(6, GetModifiersAtStep(new ApocalypseModifierIdAndStepIndex("ItemsNegativeAffixesModifier", 0)));
			apocalypseLevelsEquivalences.Add(item);
			isInitialized = true;
		}
	}

	private static List<ApocalypseModifierIdAndStepIndex> GetModifiersAtStep(params ApocalypseModifierIdAndStepIndex[] modifiersIdsAndStepIndex)
	{
		List<ApocalypseModifierIdAndStepIndex> list = new List<ApocalypseModifierIdAndStepIndex>();
		if (modifiersIdsAndStepIndex == null || modifiersIdsAndStepIndex.Length == 0)
		{
			return list;
		}
		foreach (ApocalypseModifierIdAndStepIndex apocalypseModifierIdAndStepIndex in modifiersIdsAndStepIndex)
		{
			if (!ApocalypseDatabase.ModifierDefinitions.TryGetValue(apocalypseModifierIdAndStepIndex.ModifierId, out var value))
			{
				TPSingleton<ApocalypseManager>.Instance.LogError("Apocalypse rework retro compatibility issue, couldn't find modifier '" + apocalypseModifierIdAndStepIndex.ModifierId + "' !", CLogLevel.MAJOR);
			}
			else if (apocalypseModifierIdAndStepIndex.StepIndex >= value.StepDefinitions.Count)
			{
				TPSingleton<ApocalypseManager>.Instance.LogError($"Apocalypse rework retro compatibility issue, couldn't find modifier '{apocalypseModifierIdAndStepIndex.ModifierId}' with step '{apocalypseModifierIdAndStepIndex.StepIndex}', retrieving last available index !", CLogLevel.MAJOR);
				list.Add(new ApocalypseModifierIdAndStepIndex(apocalypseModifierIdAndStepIndex.ModifierId, value.StepDefinitions.Count - 1));
			}
			else
			{
				list.Add(apocalypseModifierIdAndStepIndex);
			}
		}
		return list;
	}
}
