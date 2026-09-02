using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.View.MetaShops;
using UnityEngine;

namespace TheLastStand.Manager;

public class DarkShopManager : AMetaShopManager<DarkShopManager>
{
	[SerializeField]
	private TextMeshProUGUI damnedSoulsText;

	protected override string ActivatedNumberLocalizationKey => "Meta_DarkShop_ActivatedNumber";

	public bool IsAnyUpgradeAffordable()
	{
		return TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Find((MetaUpgrade upgrade) => upgrade.MetaUpgradeDefinition.DamnedSoulsShop && ApplicationManager.Application.DamnedSouls >= upgrade.SoulsLeftToFulfill) != null;
	}

	public override void RefreshShop()
	{
		base.RefreshShop();
		RefreshUpgradesByState(TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades, MetaUpgradeLineView.E_State.Unlocked);
		RefreshUpgradesByState(TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades, MetaUpgradeLineView.E_State.Activated);
		RefreshUpgradesByState(TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades, MetaUpgradeLineView.E_State.Locked);
		base.SortedLines = base.Lines.Values.ToList();
		RefreshShopView();
		RefreshShopJoystickNavigation();
	}

	public override void RefreshShopView()
	{
		base.RefreshShopView();
		UpdateDamnedSoulsText();
	}

	public override void RefreshTexts()
	{
		base.RefreshTexts();
		UpdateActivatedMetasText();
		UpdateDamnedSoulsText();
	}

	public void SpendDamnedSouls(uint value)
	{
		ApplicationManager.Application.DamnedSouls -= value;
		UpdateDamnedSoulsText();
	}

	protected override void ApplySpecificSorting()
	{
		base.SortedLines = new List<MetaUpgradeLineView>();
		foreach (KeyValuePair<MetaUpgrade, MetaUpgradeLineView> line in base.Lines)
		{
			if (TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Contains(line.Key))
			{
				base.SortedLines.Add(line.Value);
			}
		}
		base.SortedLines.Sort(DarkMetaUpgradeLineViewsComparer);
		int num = 0;
		foreach (MetaUpgradeLineView sortedLine in base.SortedLines)
		{
			sortedLine.transform.SetSiblingIndex(num++);
		}
	}

	protected override int GetDefaultSortingStartingIndex()
	{
		return 0;
	}

	protected override bool IsRelatedShopOpen()
	{
		return TPSingleton<OraculumView>.Instance.IsInDarkShop;
	}

	protected override bool IsValidUpgradeForShop(MetaUpgrade metaUpgrade)
	{
		if (metaUpgrade.MetaUpgradeDefinition.IsLinkedToDLC && !metaUpgrade.IsLinkedDLCOwned)
		{
			return false;
		}
		return metaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop;
	}

	private static int DarkMetaUpgradeLineViewsComparer(MetaUpgradeLineView line1, MetaUpgradeLineView line2)
	{
		if (line1.MetaUpgrade.SoulsLeftToFulfill < line2.MetaUpgrade.SoulsLeftToFulfill)
		{
			return -1;
		}
		if (line1.MetaUpgrade.SoulsLeftToFulfill > line2.MetaUpgrade.SoulsLeftToFulfill)
		{
			return 1;
		}
		return 0;
	}

	private void UpdateDamnedSoulsText()
	{
		damnedSoulsText.text = ApplicationManager.Application.DamnedSouls.ToString();
	}
}
