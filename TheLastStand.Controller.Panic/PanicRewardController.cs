using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Panic;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Item;
using TheLastStand.Model.Panic;

namespace TheLastStand.Controller.Panic;

public class PanicRewardController
{
	public PanicReward PanicReward { get; }

	public PanicRewardController(TheLastStand.Model.Panic.Panic panic)
	{
		PanicReward = new PanicReward(panic, this);
	}

	public bool CanReroll()
	{
		if (!CanRerollFree())
		{
			return CanRerollWithEssence();
		}
		return true;
	}

	public bool CanRerollFree()
	{
		return PanicReward.RemainingNbRerollReward > 0;
	}

	public bool CanRerollWithEssence()
	{
		if (TPSingleton<SinkManager>.Instance.IsSinkUnlocked)
		{
			return ApplicationManager.Application.DamnedSouls >= PanicReward.DamnedSoulsRerollCost;
		}
		return false;
	}

	public bool IsInFreeRerollMode()
	{
		if (TPSingleton<SinkManager>.Instance.IsSinkUnlocked)
		{
			return CanRerollFree();
		}
		return true;
	}

	public void GetReward()
	{
		int level = PanicReward.Panic.Level;
		PanicReward.Gold = PanicReward.Panic.PanicDefinition.PanicLevelDefinitions[level].PanicRewardDefinition.Gold.EvalToInt(PanicReward.Panic.PanicEvalGoldContext);
		PanicReward.Materials = PanicReward.Panic.PanicDefinition.PanicLevelDefinitions[level].PanicRewardDefinition.Materials.EvalToInt(PanicReward.Panic.PanicEvalMaterialContext);
		if (PanicReward.Panic.PanicDefinition.PanicLevelDefinitions[level].PanicRewardDefinition.ItemsListsPerDay == null)
		{
			return;
		}
		if (PanicReward.Items == null)
		{
			PanicReward.Items = new TheLastStand.Model.Item.Item[TPSingleton<ItemManager>.Instance.NightRewardsCount];
		}
		PanicRewardDefinition panicRewardDefinition = PanicReward.Panic.PanicDefinition.PanicLevelDefinitions[PanicReward.Panic.Level].PanicRewardDefinition;
		PanicRewardDefinition.DayGenerationDatas dayGenerationDatas = null;
		int num = TPSingleton<GameManager>.Instance.Game.DayNumber + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.PanicRewardItemsOffset;
		int num2 = -1;
		foreach (KeyValuePair<int, PanicRewardDefinition.DayGenerationDatas> item2 in panicRewardDefinition.ItemsListsPerDay)
		{
			if (dayGenerationDatas == null || (item2.Key > num2 && item2.Key <= num))
			{
				num2 = item2.Key;
				dayGenerationDatas = item2.Value;
			}
		}
		string itemGenerationModifiersListId = dayGenerationDatas.ItemGenerationModifiersListId;
		if (!ItemDatabase.ItemGenerationModifierListDefinitions.TryGetValue(itemGenerationModifiersListId, out var value))
		{
			TPSingleton<ItemManager>.Instance.LogError("Could not find item modifiers list of ID " + itemGenerationModifiersListId);
		}
		LevelProbabilitiesTreeController levelProbabilitiesTreeController = new LevelProbabilitiesTreeController(dayGenerationDatas.BaseGenerationLevel, value);
		int num3 = 1000;
		int num4 = 0;
		for (int i = 0; i < PanicReward.Items.Length; i++)
		{
			num4 = 0;
			ItemsListDefinition itemsListDefinition = ItemDatabase.ItemsListDefinitions[dayGenerationDatas.ItemsListId];
			ItemDefinition itemDefinition;
			int level2;
			do
			{
				itemDefinition = ItemManager.TakeRandomItemInList(itemsListDefinition);
				level2 = levelProbabilitiesTreeController.GenerateLevel();
				level2 = itemDefinition.GetHigherExistingLevelFromInitValue(level2);
				if (level2 == -1)
				{
					num4++;
				}
			}
			while (level2 == -1 && num4 < num3);
			if (num4 >= num3)
			{
				TPSingleton<PanicManager>.Instance.LogError("The generation of a panic reward took the maximum iterations nb and couldn't find a suitable item !", CLogLevel.DETAILED);
			}
			ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition = ItemDatabase.ItemRaritiesListDefinitions[dayGenerationDatas.ItemRaritiesListId];
			int minRarityIndexFromItemDefinition = RarityProbabilitiesTreeController.GetMinRarityIndexFromItemDefinition(itemDefinition);
			ItemDefinition.E_Rarity rarity = RarityProbabilitiesTreeController.GenerateRarity(probabilityTreeEntriesDefinition, minRarityIndexFromItemDefinition);
			TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(new ItemManager.ItemGenerationInfo
			{
				ItemDefinition = itemDefinition,
				Level = level2,
				Rarity = rarity
			});
			PanicReward.Items[i] = item;
			PanicReward.ItemsCount++;
		}
	}

	public int ReloadBaseNbRerollReward()
	{
		int num = 0;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockRerollRewardMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			UnlockRerollRewardMetaEffectDefinition[] array = effects;
			foreach (UnlockRerollRewardMetaEffectDefinition unlockRerollRewardMetaEffectDefinition in array)
			{
				num += unlockRerollRewardMetaEffectDefinition.RerollReward;
			}
		}
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<AdditionalRerollRewardMetaEffectDefinition>(out var effects2, MetaUpgradesManager.E_MetaState.Activated))
		{
			AdditionalRerollRewardMetaEffectDefinition[] array2 = effects2;
			foreach (AdditionalRerollRewardMetaEffectDefinition additionalRerollRewardMetaEffectDefinition in array2)
			{
				num += additionalRerollRewardMetaEffectDefinition.RerollReward;
			}
		}
		num += TPSingleton<GlyphManager>.Instance.AdditionRewardReroll;
		return PanicReward.BaseNbRerollReward = num;
	}

	public void ReloadRemainingNbRerollReward()
	{
		PanicReward.RemainingNbRerollReward = PanicReward.BaseNbRerollReward;
	}

	public void Reset()
	{
		PanicReward.Gold = 0;
		PanicReward.Materials = 0;
		PanicReward.ItemsCount = 0;
		if (PanicReward.Items != null)
		{
			for (int i = 0; i < TPSingleton<ItemManager>.Instance.NightRewardsCount; i++)
			{
				PanicReward.Items[i] = null;
			}
		}
	}
}
