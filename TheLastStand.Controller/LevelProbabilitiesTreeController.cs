using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Controller;

public sealed class LevelProbabilitiesTreeController
{
	private int highestModifierLevel;

	private int lowestModifierLevel;

	private Dictionary<int, int> modifiersByLevel;

	private ProbabilityTreeEntriesDefinition probabilityTreeDefinition;

	public ILevelOwner LevelOwner { get; private set; }

	public int Level { get; private set; } = -1;

	public LevelProbabilitiesTreeController(int level, Dictionary<int, int> modifiersByLevel)
	{
		Level = level;
		this.modifiersByLevel = ((modifiersByLevel != null && modifiersByLevel.Count > 0) ? new Dictionary<int, int>(modifiersByLevel) : null);
		BoundariesModifierLevel();
	}

	public LevelProbabilitiesTreeController(int level, ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition)
	{
		Level = level;
		modifiersByLevel = ((probabilityTreeEntriesDefinition.ProbabilityLevels != null && probabilityTreeEntriesDefinition.ProbabilityLevels.Count > 0) ? new Dictionary<int, int>(probabilityTreeEntriesDefinition.ProbabilityLevels) : null);
		probabilityTreeDefinition = probabilityTreeEntriesDefinition;
		BoundariesModifierLevel();
	}

	public LevelProbabilitiesTreeController(ILevelOwner levelOwner, ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition)
	{
		LevelOwner = levelOwner;
		modifiersByLevel = ((probabilityTreeEntriesDefinition.ProbabilityLevels != null && probabilityTreeEntriesDefinition.ProbabilityLevels.Count > 0) ? new Dictionary<int, int>(probabilityTreeEntriesDefinition.ProbabilityLevels) : null);
		probabilityTreeDefinition = probabilityTreeEntriesDefinition;
		BoundariesModifierLevel();
	}

	public LevelProbabilitiesTreeController(ILevelOwner levelOwner, Dictionary<int, int> modifiersByLevel)
	{
		LevelOwner = levelOwner;
		this.modifiersByLevel = ((modifiersByLevel != null && modifiersByLevel.Count > 0) ? new Dictionary<int, int>(modifiersByLevel) : null);
		BoundariesModifierLevel();
	}

	public int GenerateLevel()
	{
		if (modifiersByLevel == null)
		{
			return LevelOwner?.Level ?? Level;
		}
		Dictionary<int, int> dictionary = ComputeProbablyTree();
		if (dictionary != null)
		{
			foreach (int key in dictionary.Keys)
			{
				if (key > highestModifierLevel)
				{
					highestModifierLevel = key;
				}
				else if (key < lowestModifierLevel)
				{
					lowestModifierLevel = key;
				}
			}
		}
		for (int num = highestModifierLevel; num >= lowestModifierLevel; num--)
		{
			if (RandomManager.GetRandomRange(this, 0, 100) < dictionary[num])
			{
				int num2 = num + ((LevelOwner != null) ? LevelOwner.Level : Level);
				TPSingleton<RandomManager>.Instance.Log($"[{((LevelOwner != null) ? LevelOwner.Name : string.Empty)}] Generated level {num2}.", CLogLevel.DETAILED);
				return Mathf.Max(0, num2);
			}
		}
		TPSingleton<RandomManager>.Instance.Log("[" + ((LevelOwner != null) ? LevelOwner.Name : string.Empty) + "] Generated level 0.", CLogLevel.DETAILED);
		return 0;
	}

	public void Log()
	{
		string text = $"<b>#--- PROBAS TREE (Level {LevelOwner.Level}) ---#</b>\n";
		foreach (KeyValuePair<int, int> item in modifiersByLevel)
		{
			text += $"{item.Key} / {item.Value}\n";
		}
		TPSingleton<RandomManager>.Instance.Log("#Probabilities Tree.#" + text);
	}

	private void BoundariesModifierLevel()
	{
		if (modifiersByLevel == null)
		{
			return;
		}
		foreach (int key in modifiersByLevel.Keys)
		{
			if (key > highestModifierLevel)
			{
				highestModifierLevel = key;
			}
			else if (key < lowestModifierLevel)
			{
				lowestModifierLevel = key;
			}
		}
	}

	private Dictionary<int, int> ComputeProbablyTree()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(probabilityTreeDefinition.ProbabilityLevels);
		Dictionary<int, int> dictionary2 = TPSingleton<GlyphManager>.Instance.StartingGearLevelModifiers.GetValueOrDefault(probabilityTreeDefinition.Id) ?? new Dictionary<int, int>();
		if (TPSingleton<GlyphManager>.Instance.LevelProbabilityTreeModifiers.TryGetValue(probabilityTreeDefinition.Id, out var value))
		{
			dictionary = dictionary.Add(value);
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<ItemLevelProbabilityMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated) && effects.TryFind((ItemLevelProbabilityMetaEffectDefinition x) => x.LevelTreeId == probabilityTreeDefinition.Id, out var value2))
		{
			foreach (KeyValuePair<int, int> item in value2.WeightBonusByLevelProbability)
			{
				if (dictionary2.ContainsKey(item.Key))
				{
					dictionary2[item.Key] += item.Value;
				}
				else
				{
					dictionary2.Add(item.Key, item.Value);
				}
			}
		}
		return dictionary.Add(dictionary2);
	}
}
