using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Controller;

public class RarityProbabilitiesTreeController
{
	public static ItemDefinition.E_Rarity GenerateRarity(ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition, int minRarityIndex = -1, int maxRarityIndex = -1)
	{
		return GenerateRarity(ComputeRarityProbabilities(probabilityTreeEntriesDefinition), minRarityIndex, maxRarityIndex);
	}

	public static ItemDefinition.E_Rarity GenerateRarity(Dictionary<int, int> probabilities, int minRarityIndex = -1, int maxRarityIndex = -1)
	{
		int num = ((maxRarityIndex != -1) ? maxRarityIndex : 4);
		int num2 = ((minRarityIndex == -1) ? 1 : minRarityIndex);
		for (int num3 = num; num3 >= num2; num3--)
		{
			if (probabilities.ContainsKey(num3) && RandomManager.GetRandomRange("RarityProbabilitiesTreeController", 0, 100) < probabilities[num3])
			{
				return (ItemDefinition.E_Rarity)num3;
			}
		}
		return (ItemDefinition.E_Rarity)num2;
	}

	public static Dictionary<int, int> ComputeRarityProbabilities(ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(probabilityTreeEntriesDefinition.ProbabilityLevels);
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		if (TPSingleton<GlyphManager>.Instance.RarityProbabilityTreeModifiers.TryGetValue(probabilityTreeEntriesDefinition.Id, out var value))
		{
			dictionary = dictionary.Add(value);
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<ItemRaritiesMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int num = effects.Length - 1; num >= 0; num--)
			{
				ItemRaritiesMetaEffectDefinition itemRaritiesMetaEffectDefinition = effects[num];
				if (itemRaritiesMetaEffectDefinition.RarityTreeId == probabilityTreeEntriesDefinition.Id)
				{
					foreach (KeyValuePair<int, int> item in itemRaritiesMetaEffectDefinition.WeightBonusByRarityLevel)
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
			}
		}
		return dictionary.Add(dictionary2);
	}

	public static int GetMinRarityIndexFromItemDefinition(ItemDefinition itemDefinition)
	{
		int num = -1;
		if (!TPSingleton<GlyphManager>.Exist() || itemDefinition == null)
		{
			return num;
		}
		foreach (string tag in itemDefinition.Tags)
		{
			if (TPSingleton<GlyphManager>.Instance.ItemByTagRarityModifier.TryGetValue(tag, out var value) && num < value.MinRarityIndex)
			{
				num = value.MinRarityIndex;
			}
		}
		return num;
	}
}
