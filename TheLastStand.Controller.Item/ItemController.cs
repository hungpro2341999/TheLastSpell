using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Serialization.Item;

namespace TheLastStand.Controller.Item;

public class ItemController
{
	public TheLastStand.Model.Item.Item Item { get; }

	public ItemController(SerializedItem container, ItemSlot itemSlot)
	{
		Item = new TheLastStand.Model.Item.Item(container, this, itemSlot);
	}

	public ItemController(ItemDefinition itemDefinition, int level, ItemDefinition.E_Rarity rarity)
	{
		Item = new TheLastStand.Model.Item.Item(itemDefinition, this)
		{
			Level = level,
			Rarity = rarity,
			Resistance = ((itemDefinition.Resistance.x == itemDefinition.Resistance.y) ? itemDefinition.Resistance.x : RandomManager.GetRandomRange(this, itemDefinition.Resistance.x, itemDefinition.Resistance.y + 1))
		};
		InitItemAdditionalData();
	}

	public ItemController(TheLastStand.Model.Item.Item itemToCopy)
	{
		Item = new TheLastStand.Model.Item.Item(itemToCopy.ItemDefinition, this)
		{
			Level = itemToCopy.Level,
			Rarity = itemToCopy.Rarity,
			AdditionalAffixes = itemToCopy.AdditionalAffixes,
			AdditionalAffixesMalus = itemToCopy.AdditionalAffixesMalus,
			Resistance = itemToCopy.Resistance
		};
		InitItemAdditionalData();
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> MergeAffixes(IEnumerable<IAffix> affixes)
	{
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = new Dictionary<UnitStatDefinition.E_Stat, float>();
		foreach (IAffix affix in affixes)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> finalStatModifier in affix.GetFinalStatModifiers())
			{
				if (dictionary.ContainsKey(finalStatModifier.Key))
				{
					dictionary[finalStatModifier.Key] += finalStatModifier.Value;
				}
				else
				{
					dictionary.Add(finalStatModifier.Key, finalStatModifier.Value);
				}
			}
		}
		return dictionary;
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> MergeAllAffixes()
	{
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = MergeAffixes(Item.AdditionalAffixes);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in MergeAffixes(Item.AdditionalAffixesMalus))
		{
			if (dictionary.ContainsKey(item.Key))
			{
				dictionary[item.Key] -= item.Value;
			}
			else
			{
				dictionary.Add(item.Key, 0f - item.Value);
			}
		}
		return dictionary;
	}

	public void PerkAddReplacementSkill(TheLastStand.Model.Skill.Skill replacementSkill)
	{
		if (Item.PerkReplacementSkills.All((TheLastStand.Model.Skill.Skill itemSkill) => itemSkill.Id != replacementSkill.Id))
		{
			Item.PerkReplacementSkills.Add(replacementSkill);
		}
	}

	public void PerkClearAllReplacementSkills()
	{
		Item.PerkReplacementSkills.Clear();
	}

	public void RefillOverallUses()
	{
		for (int num = Item.Skills.Count - 1; num >= 0; num--)
		{
			if (Item.Skills[num].OverallUsesRemaining != -1)
			{
				Item.Skills[num].OverallUsesRemaining = Item.Skills[num].ComputeTotalUses(Item.Holder);
			}
		}
		if (Item.PerkReplacementSkills.Count <= 0)
		{
			return;
		}
		for (int num2 = Item.PerkReplacementSkills.Count - 1; num2 >= 0; num2--)
		{
			if (Item.PerkReplacementSkills[num2].OverallUsesRemaining != -1)
			{
				Item.PerkReplacementSkills[num2].OverallUsesRemaining = Item.PerkReplacementSkills[num2].ComputeTotalUses(Item.Holder);
			}
		}
	}

	public void StartTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			RefillOverallUses();
		}
		for (int num = Item.Skills.Count - 1; num >= 0; num--)
		{
			if (Item.Skills[num].SkillDefinition.UsesPerTurnCount != -1)
			{
				Item.Skills[num].SetUsesPerTurnRemaining(Item.Skills[num].UsesPerTurn);
			}
		}
		if (Item.PerkReplacementSkills.Count <= 0)
		{
			return;
		}
		for (int num2 = Item.PerkReplacementSkills.Count - 1; num2 >= 0; num2--)
		{
			if (!Item.PerkReplacementSkills[num2].IsLinkedWithAnotherSkillForUses && Item.PerkReplacementSkills[num2].SkillDefinition.UsesPerTurnCount != -1)
			{
				Item.PerkReplacementSkills[num2].SetUsesPerTurnRemaining(Item.PerkReplacementSkills[num2].UsesPerTurn);
			}
		}
	}

	private void InitItemAdditionalData()
	{
		if (Item.SkillsOverallUses != null)
		{
			foreach (KeyValuePair<string, int> skillsOverallUse in Item.SkillsOverallUses)
			{
				if (SkillDatabase.SkillDefinitions.ContainsKey(skillsOverallUse.Key))
				{
					Item.Skills.Add(new SkillController(SkillDatabase.SkillDefinitions[skillsOverallUse.Key], Item, skillsOverallUse.Value, SkillDatabase.SkillDefinitions[skillsOverallUse.Key].UsesPerTurnCount).Skill);
				}
			}
		}
		if (Item.ItemDefinition.PerksByLevel[Item.Level] == null)
		{
			return;
		}
		foreach (string item in Item.ItemDefinition.PerksByLevel[Item.Level])
		{
			if (PlayableUnitDatabase.PerkDefinitions.ContainsKey(item) && !Item.Perks.ContainsKey(item))
			{
				Item.Perks.Add(item, new PerkController(PlayableUnitDatabase.PerkDefinitions[item], null, null, null, string.Empty, isNative: false, isFromRace: false).Perk);
			}
			Item.PerksId.Add(item);
		}
	}
}
