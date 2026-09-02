using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.View.WorldMap;
using UnityEngine;

namespace TheLastStand.Manager;

public sealed class ApocalypseManager : Manager<ApocalypseManager>, ISerializable, IDeserializable
{
	public static class Constants
	{
		public const string Percentage = "Percentage";

		public const int AppSaveVersionBeforeRework = 13;

		public const int GameSaveVersionBeforeRework = 23;
	}

	private class StringToApocalypseModifierConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ApocalypseDatabase.ModifierDefinitions.Keys);
	}

	private class StringToApocalypseModifierStepConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ApocalypseDatabase.ModifierStepDefinitions.Keys);
	}

	private class StringToApocalypseTierConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ApocalypseDatabase.TierDefinitions.Keys);
	}

	public static ApocalypseStateBeforeGameOver ApocalypseStateBeforeGameOver;

	public static Apocalypse CurrentApocalypse { get; private set; }

	public static List<ApocalypseModifierStepDefinition> CurrentApocalypseModifierStepDefinitions => CurrentApocalypse?.ModifierStepDefinitions;

	public static int CurrentApocalypseModifiersCount
	{
		get
		{
			if (CurrentApocalypse?.ModifierStepDefinitions == null)
			{
				return 0;
			}
			return CurrentApocalypse.ModifierStepDefinitions.Count;
		}
	}

	public static int CurrentApocalypseLevel
	{
		get
		{
			if (CurrentApocalypse != null)
			{
				return CurrentApocalypse.CurrentLevel;
			}
			return 0;
		}
	}

	public static bool IsApocalypseUnlocked => TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0;

	public static Dictionary<string, bool> ModifiersUnlockSeen { get; private set; }

	public static Dictionary<string, bool> TiersUnlockedState { get; private set; }

	public int MaxApocalypseIndexAvailable { get; set; }

	public uint DamnedSoulsPercentageModifier => GetDamnedSoulsPercentageModifier();

	public static event Action OnStatModifierFromTurnCheck;

	public static event Action OnDeactivateModifiersWithTurnConditions;

	public static void CheckEnemyUnitTurnEnded()
	{
		ApocalypseManager.OnStatModifierFromTurnCheck();
	}

	public static void CheckEffectsActivationOnTurnCondition()
	{
		CurrentApocalypse?.ApocalypseController.CheckEffectsActivationOnTurnCondition();
	}

	public static void DeactivateModifiersWithTurnConditions()
	{
		ApocalypseManager.OnDeactivateModifiersWithTurnConditions();
	}

	public static uint GetDamnedSoulsPercentageModifier(int apocalypseLevel = -1)
	{
		if (apocalypseLevel == -1)
		{
			apocalypseLevel = CurrentApocalypseLevel;
		}
		return (uint)apocalypseLevel * ApocalypseDatabase.ConfigurationDefinition.DamnedSoulsPercentagePerLevel;
	}

	public static int GetHighestApocalypseLevelReached()
	{
		int num = 0;
		if (!IsApocalypseUnlocked)
		{
			return num;
		}
		foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
		{
			if (city.MaxApocalypsePassed > num)
			{
				num = city.MaxApocalypsePassed;
			}
		}
		return num;
	}

	public static int GetMaxApocalypseLevel(bool onlyGetReachableLevelWithCurrentUnlocks = false)
	{
		int num = 0;
		foreach (ApocalypseModifierDefinition value in ApocalypseDatabase.ModifierDefinitions.Values)
		{
			if (onlyGetReachableLevelWithCurrentUnlocks)
			{
				if (IsModifierUnlocked(value))
				{
					num += value.StepDefinitions[^1].ApocalypseLevel;
				}
			}
			else
			{
				num += value.StepDefinitions[^1].ApocalypseLevel;
			}
		}
		return num;
	}

	public static bool IsModifierUnlocked(ApocalypseModifierDefinition apocalypseModifierDefinition)
	{
		if (TiersUnlockedState.ContainsKey(apocalypseModifierDefinition.TierId))
		{
			if (!TiersUnlockedState[apocalypseModifierDefinition.TierId])
			{
				return HasAnyModifierStepBeenCompleted(apocalypseModifierDefinition);
			}
			return true;
		}
		return false;
	}

	public static bool IsThisStatIncreasedByPercentage(UnitStatDefinition.E_Stat stat)
	{
		if (ApocalypseDatabase.ConfigurationDefinition.StatWithModifierTypes.TryGetValue(stat, out var value))
		{
			return value == "Percentage";
		}
		TPSingleton<ApocalypseManager>.Instance.LogError($"This stat hasn't an Apocalypse type modifier : {stat}", CLogLevel.MAJOR);
		return false;
	}

	public static void LogCurrentApocalypse()
	{
		if (CurrentApocalypse != null)
		{
			TPSingleton<ApocalypseManager>.Instance.Log(CurrentApocalypse.ToString(), CLogLevel.MAJOR);
		}
	}

	public static void RefreshTiersUnlockedState(bool canRefreshUnlockedTiersState = false)
	{
		if (!IsApocalypseUnlocked)
		{
			return;
		}
		int highestApocalypseLevelReached = GetHighestApocalypseLevelReached();
		foreach (string item in new List<string>(TiersUnlockedState.Keys))
		{
			if (ApocalypseDatabase.TierDefinitions.TryGetValue(item, out var value) && (canRefreshUnlockedTiersState || !TiersUnlockedState[item]))
			{
				TiersUnlockedState[item] = value.ApocalypseLevelCompletedToUnlock == -1 || highestApocalypseLevelReached >= value.ApocalypseLevelCompletedToUnlock;
			}
		}
	}

	public static void SetApocalypse(List<ApocalypseModifierStepDefinition> modifierStepDefinitions, bool computeLevel = true, bool computeEffects = true)
	{
		if (CurrentApocalypse == null)
		{
			CurrentApocalypse = new ApocalypseController(modifierStepDefinitions).Apocalypse;
		}
		else
		{
			CurrentApocalypse.ApocalypseController.SetSelectedModifierSteps(modifierStepDefinitions, computeLevel, computeEffects);
		}
	}

	public static void SetModifierUnlockSeen(string modifierId, bool isSeen)
	{
		if (ModifiersUnlockSeen.ContainsKey(modifierId))
		{
			ModifiersUnlockSeen[modifierId] = isSeen;
		}
	}

	public static void StoreApocalypseStateBeforeGameOver()
	{
		ApocalypseStateBeforeGameOver.WasApocalypseUnlocked = IsApocalypseUnlocked;
		ApocalypseStateBeforeGameOver.HighestLevelReached = GetHighestApocalypseLevelReached();
	}

	public static void TryIncreaseMaxApocalypseIndexAvailable()
	{
		if (ApocalypseDatabase.ConfigurationDefinition.ApocalypseUnlockConditions.All((ApocalypseUnlockCondition condition) => condition.IsValid) && TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable == 0)
		{
			TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable = 1;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid && CurrentApocalypse == null)
		{
			SetApocalypse(null);
		}
	}

	private static bool HasAnyModifierStepBeenCompleted(ApocalypseModifierDefinition apocalypseModifierDefinition)
	{
		if (!TPSingleton<WorldMapCityManager>.Exist() || TPSingleton<WorldMapCityManager>.Instance.Cities == null)
		{
			return false;
		}
		foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
		{
			if (city.CompletedApocalypseModifiersStepIndex.ContainsKey(apocalypseModifierDefinition.Id) && city.CompletedApocalypseModifiersStepIndex[apocalypseModifierDefinition.Id] >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private static void InitApocalypseTiers()
	{
		TiersUnlockedState.Clear();
		foreach (ApocalypseTierDefinition value in ApocalypseDatabase.TierDefinitions.Values)
		{
			if (!TiersUnlockedState.ContainsKey(value.Id))
			{
				TiersUnlockedState.Add(value.Id, value: false);
			}
		}
	}

	private static void InitApocalypseModifiersUnlockSeen()
	{
		ModifiersUnlockSeen.Clear();
		foreach (ApocalypseModifierDefinition value in ApocalypseDatabase.ModifierDefinitions.Values)
		{
			if (!ModifiersUnlockSeen.ContainsKey(value.Id))
			{
				ModifiersUnlockSeen.Add(value.Id, value: false);
			}
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedApocalypse serializedApocalypse && CurrentApocalypse != null)
		{
			if (saveVersion <= 23)
			{
				ApocalypseRetroCompatibilityController.ApplyRetroCompatibilityToGameSaveApocalypse(saveVersion, serializedApocalypse.ApocalypseIndex);
			}
			CurrentApocalypse.ApocalypseController.ComputeAllData(computeLevel: false);
			LogCurrentApocalypse();
		}
	}

	public void GlobalDeserialize(ISerializedData container = null)
	{
		InitApocalypseTiers();
		InitApocalypseModifiersUnlockSeen();
		SetApocalypse(null);
		if (container != null)
		{
			int maxApocalypseLevel = GetMaxApocalypseLevel();
			if (!(container is SerializedGlobalApocalypse serializedGlobalApocalypse))
			{
				return;
			}
			MaxApocalypseIndexAvailable = Mathf.Clamp(serializedGlobalApocalypse.MaxAvailableApocalypseIndex, 0, maxApocalypseLevel);
			foreach (string item2 in serializedGlobalApocalypse.ApocalypseModifiersUnlockSeen)
			{
				if (ModifiersUnlockSeen.ContainsKey(item2))
				{
					ModifiersUnlockSeen[item2] = true;
				}
			}
			List<ApocalypseModifierStepDefinition> list = new List<ApocalypseModifierStepDefinition>();
			if (serializedGlobalApocalypse.SelectedModifierSteps != null)
			{
				foreach (SerializedApocalypseModifierStep selectedModifierStep in serializedGlobalApocalypse.SelectedModifierSteps)
				{
					if (!ApocalypseDatabase.ModifierDefinitions.TryGetValue(selectedModifierStep.ModifierId, out var value))
					{
						LogError("Couldn't find apocalypse modifier with id '" + selectedModifierStep.ModifierId + "' when loading GameSave Apocalypse configuration !", CLogLevel.MAJOR);
						continue;
					}
					if (selectedModifierStep.StepIndex == -1 || selectedModifierStep.StepIndex >= value.StepDefinitions.Count)
					{
						LogError($"Couldn't find apocalypse step modifier with stepIndex '{selectedModifierStep.StepIndex}' when loading GameSave Apocalypse configuration ! Trying to find another existing step !", CLogLevel.MAJOR);
						selectedModifierStep.StepIndex = value.StepDefinitions.Count - 1;
					}
					if (selectedModifierStep.StepIndex < value.StepDefinitions.Count)
					{
						ApocalypseModifierStepDefinition item = value.StepDefinitions[selectedModifierStep.StepIndex];
						list.Add(item);
					}
				}
			}
			SetApocalypse(list, computeLevel: true, computeEffects: false);
		}
		else
		{
			MaxApocalypseIndexAvailable = 0;
		}
	}

	public ISerializedData GlobalSerialize()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, bool> item in ModifiersUnlockSeen)
		{
			if (item.Value)
			{
				list.Add(item.Key);
			}
		}
		List<SerializedApocalypseModifierStep> list2 = new List<SerializedApocalypseModifierStep>();
		foreach (ApocalypseModifierStepDefinition currentApocalypseModifierStepDefinition in CurrentApocalypseModifierStepDefinitions)
		{
			(string, int) modifierIdAndStepIndex = currentApocalypseModifierStepDefinition.GetModifierIdAndStepIndex();
			SerializedApocalypseModifierStep serializedApocalypseModifierStep = new SerializedApocalypseModifierStep();
			(serializedApocalypseModifierStep.ModifierId, serializedApocalypseModifierStep.StepIndex) = modifierIdAndStepIndex;
			list2.Add(serializedApocalypseModifierStep);
		}
		return new SerializedGlobalApocalypse
		{
			MaxAvailableApocalypseIndex = MaxApocalypseIndexAvailable,
			ApocalypseModifiersUnlockSeen = list,
			SelectedModifierSteps = list2
		};
	}

	public ISerializedData Serialize()
	{
		return new SerializedApocalypse
		{
			ApocalypseIndex = CurrentApocalypseLevel
		};
	}

	[DevConsoleCommand("ApocalypseAddModifier")]
	public static void Debug_ApocalypseAddModifier([StringConverter(typeof(StringToApocalypseModifierConverter))] string modifierId, int stepIndex = 0)
	{
		if (!ApocalypseDatabase.ModifierDefinitions.TryGetValue(modifierId, out var value))
		{
			TPSingleton<ApocalypseManager>.Instance.LogError("Apocalypse modifier with id '" + modifierId + "' was not found !", TPSingleton<ApocalypseManager>.Instance);
			return;
		}
		int num = value.StepDefinitions.Count - 1;
		if (stepIndex < 0 || stepIndex > num)
		{
			TPSingleton<ApocalypseManager>.Instance.LogError($"StepIndex {stepIndex} is invalid for modifier {modifierId}, please select a value between 0 and {num}", TPSingleton<ApocalypseManager>.Instance);
			return;
		}
		foreach (ApocalypseModifierStepDefinition stepDefinition in value.StepDefinitions)
		{
			CurrentApocalypse.ApocalypseController.RemoveSelectedModifierStep(stepDefinition, computeLevel: false);
		}
		CurrentApocalypse.ApocalypseController.AddSelectedModifierStep(value.StepDefinitions[stepIndex], computeLevel: true, computeEffects: true);
	}

	[DevConsoleCommand("ApocalypseAddStepModifier")]
	public static void Debug_ApocalypseAddStepModifier([StringConverter(typeof(StringToApocalypseModifierStepConverter))] string modifierStepId)
	{
		if (!ApocalypseDatabase.ModifierStepDefinitions.TryGetValue(modifierStepId, out var value))
		{
			TPSingleton<ApocalypseManager>.Instance.LogError("Apocalypse modifier step with id '" + modifierStepId + "' was not found !", TPSingleton<ApocalypseManager>.Instance);
		}
		else
		{
			if (!ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.TryGetValue(value, out var value2))
			{
				return;
			}
			if (value2 != null)
			{
				foreach (ApocalypseModifierStepDefinition stepDefinition in value2.StepDefinitions)
				{
					CurrentApocalypse.ApocalypseController.RemoveSelectedModifierStep(stepDefinition, computeLevel: false);
				}
			}
			CurrentApocalypse.ApocalypseController.AddSelectedModifierStep(value, computeLevel: true, computeEffects: true);
		}
	}

	[DevConsoleCommand("ApocalypseClearModifiers")]
	public static void Debug_ApocalypseClearModifiers()
	{
		CurrentApocalypse.ApocalypseController.ClearAllSelectedModifierSteps(computeLevel: true, computeEffects: true);
	}

	[DevConsoleCommand("ApocalypseDisplayPickedModifiers")]
	public static void Debug_ApocalypseDisplayPickedModifiers()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (CurrentApocalypse.ModifierStepDefinitions.Count > 0)
		{
			stringBuilder.Append("== Steps modifiers: ").AppendLine();
			int count = CurrentApocalypse.ModifierStepDefinitions.Count;
			int num = 0;
			foreach (ApocalypseModifierStepDefinition modifierStepDefinition in CurrentApocalypse.ModifierStepDefinitions)
			{
				stringBuilder.Append(modifierStepDefinition.Id ?? "");
				num++;
				if (num < count)
				{
					stringBuilder.AppendLine();
				}
			}
		}
		else
		{
			stringBuilder.Append("There are <b>no apocalypse modifiers</b> selected !");
		}
		TPSingleton<DebugManager>.Instance.LogDevConsole(stringBuilder.ToString());
	}

	[DevConsoleCommand("ApocalypseGetCurrentLevel")]
	public static void Debug_ApocalypseGetCurrentLevel()
	{
		TPSingleton<DebugManager>.Instance.LogDevConsole($"Apocalypse level: {CurrentApocalypse.CurrentLevel}");
	}

	[DevConsoleCommand("ApocalypseRemoveModifier")]
	public static void Debug_ApocalypseRemoveModifier([StringConverter(typeof(StringToApocalypseModifierConverter))] string modifierId)
	{
		if (!ApocalypseDatabase.ModifierDefinitions.TryGetValue(modifierId, out var value))
		{
			TPSingleton<ApocalypseManager>.Instance.LogError("Apocalypse modifier with id '" + modifierId + "' was not found !", TPSingleton<ApocalypseManager>.Instance);
			return;
		}
		foreach (ApocalypseModifierStepDefinition stepDefinition in value.StepDefinitions)
		{
			CurrentApocalypse.ApocalypseController.RemoveSelectedModifierStep(stepDefinition, computeLevel: true, computeEffects: true);
		}
	}

	[DevConsoleCommand("ApocalypseTierChangeUnlockedState")]
	public static void Debug_ApocalypseTierChangeUnlockedState([StringConverter(typeof(StringToApocalypseTierConverter))] string tierId, bool isUnlocked = true)
	{
		if (TiersUnlockedState.ContainsKey(tierId))
		{
			TiersUnlockedState[tierId] = isUnlocked;
		}
	}

	[DevConsoleCommand("ApocalypseTierUnlockEverything")]
	public static void Debug_ApocalypseTierUnlockEverything()
	{
		foreach (string item in new List<string>(TiersUnlockedState.Keys))
		{
			TiersUnlockedState[item] = true;
		}
	}

	[DevConsoleCommand("ApocalypseTiersDisplay")]
	public static void Debug_ApocalypseTiersDisplay()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string key in TiersUnlockedState.Keys)
		{
			stringBuilder.Append($"tier: {key}, unlocked: {TiersUnlockedState[key]}").AppendLine();
		}
		TPSingleton<DebugManager>.Instance.LogDevConsole(stringBuilder.ToString());
	}

	[DevConsoleCommand("SetMaxApocalypseAvailableTo")]
	public static void SetMaxApocalypseAvailableTo(int apocalypseId)
	{
		TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable = apocalypseId;
		TPSingleton<GameConfigurationsView>.Instance.Refresh();
	}

	[DevConsoleCommand("SetMaxApocalypsePassedTo")]
	public static void SetMaxApocalypsePassedTo(string cityId, int apocalypseId)
	{
		if (apocalypseId > TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable)
		{
			TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable = apocalypseId;
		}
		WorldMapCity worldMapCity = TPSingleton<WorldMapCityManager>.Instance.Cities.FirstOrDefault((WorldMapCity x) => x.CityDefinition.Id == cityId);
		if (worldMapCity != null)
		{
			worldMapCity.MaxApocalypsePassed = apocalypseId;
			RefreshTiersUnlockedState();
		}
		TPSingleton<GameConfigurationsView>.Instance.Refresh();
	}

	static ApocalypseManager()
	{
		ApocalypseManager.OnStatModifierFromTurnCheck = delegate
		{
		};
		ApocalypseManager.OnDeactivateModifiersWithTurnConditions = delegate
		{
		};
		ModifiersUnlockSeen = new Dictionary<string, bool>();
		TiersUnlockedState = new Dictionary<string, bool>();
	}
}
