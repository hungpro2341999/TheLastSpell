using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;

namespace TheLastStand.View.Item;

public class ItemTooltipSkillCycle : MonoBehaviour
{
	[SerializeField]
	private ItemTooltip[] itemTooltips;

	[SerializeField]
	private CompendiumPanel compendiumPanel;

	[SerializeField]
	private FollowElement followElement;

	private int tooltipsCount;

	public int SkillTabIndex { get; private set; }

	public int PerkSkillTabIndex { get; private set; }

	public bool CompendiumFollowRight { get; set; } = true;

	public CompendiumPanel CompendiumPanel => compendiumPanel;

	public void Reset()
	{
		SkillTabIndex = 0;
		PerkSkillTabIndex = 0;
		for (int i = 0; i < tooltipsCount; i++)
		{
			if (itemTooltips[i].Displayed)
			{
				itemTooltips[i].Refresh();
			}
		}
	}

	public void UpdateCompendium()
	{
		CompendiumPanel.Clear();
		List<ItemTooltip> list = new List<ItemTooltip>();
		for (int i = 0; i < tooltipsCount; i++)
		{
			if (itemTooltips[i].Displayed && itemTooltips[i].Item != null && itemTooltips[i].Item.Skills != null)
			{
				list.Add(itemTooltips[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].AddAttackTypeEntries();
		}
		for (int k = 0; k < list.Count; k++)
		{
			list[k].AddSkillEffectEntries();
		}
		for (int l = 0; l < list.Count; l++)
		{
			list[l].AddPerkEffectEntries();
		}
		bool flag = compendiumPanel.CompendiumEntries.Count > 0;
		if (flag && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
		{
			compendiumPanel.Display();
		}
		followElement.enabled = flag;
	}

	private void Awake()
	{
		tooltipsCount = itemTooltips.Length;
		for (int i = 0; i < tooltipsCount; i++)
		{
			itemTooltips[i].ItemTooltipSkillCycle = this;
		}
	}

	private void Update()
	{
		bool flag = false;
		if (InputManager.GetButtonDown(48))
		{
			for (int i = 0; i < tooltipsCount; i++)
			{
				if (itemTooltips[i].Item != null && (itemTooltips[i].Item.Skills != null || itemTooltips[i].Item.Perks != null))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			int skillTabIndex = SkillTabIndex;
			SkillTabIndex++;
			PerkSkillTabIndex = 0;
			bool flag2 = true;
			for (int j = 0; j < tooltipsCount; j++)
			{
				if (itemTooltips[j].Displayed && itemTooltips[j].Item != null && itemTooltips[j].Item.Skills != null && itemTooltips[j].Item.Perks != null && SkillTabIndex < itemTooltips[j].Item.Skills.Count + itemTooltips[j].Item.Perks.Count)
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				SkillTabIndex = 0;
			}
			if (SkillTabIndex != skillTabIndex)
			{
				CompendiumPanel.Clear();
			}
			for (int k = 0; k < tooltipsCount; k++)
			{
				if (itemTooltips[k].Displayed)
				{
					itemTooltips[k].Refresh();
				}
			}
		}
		if (SkillTabIndex != 0)
		{
			bool flag3 = true;
			for (int l = 0; l < tooltipsCount; l++)
			{
				if (itemTooltips[l].Displayed)
				{
					flag3 = false;
					break;
				}
			}
			if (flag3)
			{
				SkillTabIndex = 0;
				PerkSkillTabIndex = 0;
				CompendiumPanel.Clear();
			}
		}
		HandlePerkSkillCycle();
	}

	private void HandlePerkSkillCycle()
	{
		if (!InputManager.GetButtonDown(140))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < tooltipsCount; i++)
		{
			if (itemTooltips[i].Item != null && itemTooltips[i].Item.Perks != null && itemTooltips[i].CurrentPerk != null && itemTooltips[i].CurrentPerk.PerkDefinition.SkillsToShow.Count > 1)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		int perkSkillTabIndex = PerkSkillTabIndex;
		PerkSkillTabIndex++;
		bool flag2 = true;
		for (int j = 0; j < tooltipsCount; j++)
		{
			if (itemTooltips[j].Displayed && itemTooltips[j].Item != null && itemTooltips[j].Item.Perks != null && itemTooltips[j].CurrentPerk != null && PerkSkillTabIndex < itemTooltips[j].CurrentPerk.PerkDefinition.SkillsToShow.Count)
			{
				flag2 = false;
				break;
			}
		}
		if (flag2)
		{
			PerkSkillTabIndex = 0;
		}
		if (PerkSkillTabIndex != perkSkillTabIndex)
		{
			CompendiumPanel.Clear();
		}
		for (int k = 0; k < tooltipsCount; k++)
		{
			if (itemTooltips[k].Displayed)
			{
				itemTooltips[k].Refresh();
			}
		}
	}
}
