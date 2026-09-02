using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Database;
using TheLastStand.Database.Fog;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.Apocalypse.ApocalypseEffects;
using UnityEngine;

namespace TheLastStand.Model.Apocalypse;

public class Apocalypse
{
	public Dictionary<string, Dictionary<UnitStatDefinition.E_Stat, float>> EnemiesStatsBaseValueModifiers = new Dictionary<string, Dictionary<UnitStatDefinition.E_Stat, float>>();

	public Dictionary<int, List<ModifyEnemiesInjuryStageApocalypseEffectDefinition.ApocalypseInjuryStatModifier>> EnemiesInjuryStageInjuryStatModifiers = new Dictionary<int, List<ModifyEnemiesInjuryStageApocalypseEffectDefinition.ApocalypseInjuryStatModifier>>();

	public Dictionary<int, List<BuildingDefinition.E_BuildingCategory>> ForbiddenBuildingCategoriesAroundMagicCircleByRange = new Dictionary<int, List<BuildingDefinition.E_BuildingCategory>>();

	public Dictionary<int, string> InnSlotsOverridenRecruitmentLevelId = new Dictionary<int, string>();

	public Dictionary<UnitStatDefinition.E_Stat, int> ActiveStatModifierFromTurn { get; } = new Dictionary<UnitStatDefinition.E_Stat, int>();

	public List<AddEnemiesStatModifierFromTurnApocalypseEffect> AddEnemiesStatModifierFromTurnEffects { get; } = new List<AddEnemiesStatModifierFromTurnApocalypseEffect>();

	public HashSet<string> AffixesFlags { get; } = new HashSet<string>();

	public List<ApocalypseEffectDefinition> AllEffects { get; } = new List<ApocalypseEffectDefinition>();

	public ApocalypseController ApocalypseController { get; }

	public List<ApocalypseModifierStepDefinition> ModifierStepDefinitions { get; } = new List<ApocalypseModifierStepDefinition>();

	public int BuildingsMaxDeadZoneRange { get; set; }

	public Dictionary<string, int> BuildingsModifiedDeadZoneRange { get; } = new Dictionary<string, int>();

	public HashSet<string> BuildingsNotDemolishableAnymore { get; } = new HashSet<string>();

	public int CurrentLevel { get; set; }

	public float FogSpawnersApocalypseMultiplier { get; set; }

	public int DailyFogUpdateFrequencyModifier
	{
		get
		{
			int num = 0;
			for (int i = 0; i < AllEffects.Count; i++)
			{
				if (AllEffects[i] is IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition increaseDailyFogUpdateFrequencyApocalypseEffectDefinition)
				{
					num += increaseDailyFogUpdateFrequencyApocalypseEffectDefinition.Value;
				}
			}
			return num;
		}
	}

	public int EnemiesProgressionOffset { get; set; }

	public Dictionary<string, float> EnemiesSpawnWaveWeightMultiplier { get; } = new Dictionary<string, float>();

	public int ExtraPercentageOfEnemies
	{
		get
		{
			int num = 0;
			for (int i = 0; i < AllEffects.Count; i++)
			{
				if (AllEffects[i] is IncreaseEnemiesNumberApocalypseEffectDefinition increaseEnemiesNumberApocalypseEffectDefinition)
				{
					num += increaseEnemiesNumberApocalypseEffectDefinition.Value;
				}
			}
			return num;
		}
	}

	public bool GenerateMalusAffixes => AllEffects.Count((ApocalypseEffectDefinition o) => o is GenerateMalusAffixesApocalypseEffectDefinition) > 0;

	public float MagicCircleStartingHealthMultiplier { get; set; }

	public float PanicGainMultiplier { get; set; }

	public bool PlayableUnitBlockLineOfSight { get; set; }

	public int RemoveStartingPlayableUnitAmount { get; set; }

	public int StartingFogDensityModifier
	{
		get
		{
			int num = 0;
			for (int i = 0; i < AllEffects.Count; i++)
			{
				if (AllEffects[i] is IncreaseStartingFogDensityApocalypseEffectDefinition increaseStartingFogDensityApocalypseEffectDefinition)
				{
					num += increaseStartingFogDensityApocalypseEffectDefinition.Value;
				}
			}
			return num;
		}
	}

	public HashSet<string> SkillProgressionFlags { get; } = new HashSet<string>();

	public Apocalypse(ApocalypseController apocalypseController, List<ApocalypseModifierStepDefinition> modifierStepDefinitions = null)
	{
		ApocalypseController = apocalypseController;
		InitStepDefinitions(modifierStepDefinitions);
	}

	public int ExtraPercentageForCosts(ResourceManager.E_PriceModifierType type, ResourceManager.E_ResourceType resourceCostType)
	{
		int num = 0;
		for (int i = 0; i < AllEffects.Count; i++)
		{
			if (AllEffects[i] is IncreasePricesApocalypseEffectDefinition increasePricesApocalypseEffectDefinition && increasePricesApocalypseEffectDefinition.Type.HasFlag(type) && increasePricesApocalypseEffectDefinition.ResourceCostType.HasFlag(resourceCostType))
			{
				num += increasePricesApocalypseEffectDefinition.Value;
			}
		}
		return num;
	}

	public int GetModifiedLightFogSpawnersCount(int count)
	{
		if (!FogDatabase.LightFogDefinition.LightFogSpawnersMultipliers.TryGetValue(TPSingleton<FogManager>.Instance.Fog.DensityName, out var value))
		{
			TPSingleton<ApocalypseManager>.Instance.LogError("LightFogSpawners multiplier isn't defined for density : \"" + TPSingleton<FogManager>.Instance.Fog.DensityName + "\".", CLogLevel.NORMAL, forcePrintInUnity: true, printStackTrace: false);
			value = 1f;
		}
		return Mathf.RoundToInt((float)count * FogSpawnersApocalypseMultiplier * value);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"Apocalypse level: {CurrentLevel}");
		if (ModifierStepDefinitions.Count > 0)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder.AppendLine().Append("Apocalypse modifiers: ").AppendLine();
			foreach (ApocalypseModifierStepDefinition modifierStepDefinition in ModifierStepDefinitions)
			{
				if (ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.TryGetValue(modifierStepDefinition, out var value))
				{
					stringBuilder2.AppendLine().Append($"• {value.Id}: step {value.GetModifierStepIndexFromDefinition(modifierStepDefinition)} ({modifierStepDefinition.Id})");
				}
			}
			stringBuilder.Append(stringBuilder2);
		}
		return stringBuilder.ToString();
	}

	private void InitStepDefinitions(List<ApocalypseModifierStepDefinition> modifierStepDefinitions)
	{
		if (modifierStepDefinitions == null)
		{
			return;
		}
		foreach (ApocalypseModifierStepDefinition modifierStepDefinition in modifierStepDefinitions)
		{
			ModifierStepDefinitions.Add(modifierStepDefinition);
		}
	}
}
