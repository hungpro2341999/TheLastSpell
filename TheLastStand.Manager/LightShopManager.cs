using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.View.MetaShops;

namespace TheLastStand.Manager;

public class LightShopManager : AMetaShopManager<LightShopManager>
{
	protected override string ActivatedNumberLocalizationKey => "Meta_LightShop_ActivatedNumber";

	public bool IsAnyUpgradeAffordable()
	{
		return TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Find((MetaUpgrade o) => !o.MetaUpgradeDefinition.DamnedSoulsShop && o.IsAvailableWithCurrentDLCs) != null;
	}

	public override void RefreshShop()
	{
		base.RefreshShop();
		RefreshUpgradesByState(TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades, MetaUpgradeLineView.E_State.Unlocked);
		RefreshUpgradesByState(TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades, MetaUpgradeLineView.E_State.Unlocked);
		RefreshUpgradesByState(TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades, MetaUpgradeLineView.E_State.Activated);
		RefreshUpgradesByState(TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades, MetaUpgradeLineView.E_State.Locked);
		base.SortedLines = base.Lines.Values.ToList();
		RefreshShopView();
		RefreshShopJoystickNavigation();
	}

	protected override void ApplySpecificSorting()
	{
		base.SortedLines = new List<MetaUpgradeLineView>();
		foreach (KeyValuePair<MetaUpgrade, MetaUpgradeLineView> line in base.Lines)
		{
			if (TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Contains(line.Key) || TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Contains(line.Key))
			{
				base.SortedLines.Add(line.Value);
			}
		}
		base.SortedLines.Sort(LightMetaUpgradeLineViewsComparer);
		int num = 0;
		foreach (MetaUpgradeLineView sortedLine in base.SortedLines)
		{
			sortedLine.transform.SetSiblingIndex(num++);
		}
	}

	protected override int GetDefaultSortingStartingIndex()
	{
		return TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Count;
	}

	protected override bool IsRelatedShopOpen()
	{
		return TPSingleton<OraculumView>.Instance.IsInLightShop;
	}

	protected override bool IsValidUpgradeForShop(MetaUpgrade metaUpgrade)
	{
		if (metaUpgrade.MetaUpgradeDefinition.IsLinkedToDLC && !metaUpgrade.IsLinkedDLCOwned)
		{
			return false;
		}
		return !metaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop;
	}

	private static int LightMetaUpgradeLineViewsComparer(MetaUpgradeLineView line1, MetaUpgradeLineView line2)
	{
		if (line1.Slider.value > line2.Slider.value)
		{
			return -1;
		}
		if (line1.Slider.value < line2.Slider.value)
		{
			return 1;
		}
		return 0;
	}
}
