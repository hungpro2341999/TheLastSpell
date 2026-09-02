using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Manager.Item;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class WeaponRestrictionsTooltip : TooltipBase
{
	[SerializeField]
	private List<WeaponRestrictionsTooltipCategoryPanel> weaponCategoryPanels;

	[SerializeField]
	private Image boundlessModeIcon;

	private Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> restrictionFamilyDefinitions;

	private bool isBoundless;

	public void SetRestrictionFamilyDefinitions(Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> newRestrictionFamilyDefinitions)
	{
		restrictionFamilyDefinitions = newRestrictionFamilyDefinitions;
	}

	public void SetIsBoundless(bool newIsBoundless)
	{
		isBoundless = newIsBoundless;
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshText));
	}

	protected override bool CanBeDisplayed()
	{
		if (restrictionFamilyDefinitions != null)
		{
			return restrictionFamilyDefinitions.Count > 0;
		}
		return false;
	}

	protected override void RefreshContent()
	{
		RefreshText();
		RefreshBoundless();
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshText));
	}

	private void RefreshText()
	{
		if (restrictionFamilyDefinitions == null)
		{
			return;
		}
		int count = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.ItemCategoriesCollectionDefinition.itemCategoryDefinitions.Count;
		for (int i = 0; i < weaponCategoryPanels.Count; i++)
		{
			if (i < count)
			{
				weaponCategoryPanels[i].gameObject.SetActive(value: true);
				ItemRestrictionCategoryDefinition itemRestrictionCategoryDefinition = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.ItemCategoriesCollectionDefinition.itemCategoryDefinitions[i];
				weaponCategoryPanels[i].SetContent(itemRestrictionCategoryDefinition.ItemCategory, restrictionFamilyDefinitions[itemRestrictionCategoryDefinition.ItemCategory]);
			}
			else
			{
				weaponCategoryPanels[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void RefreshBoundless()
	{
		boundlessModeIcon.enabled = isBoundless;
	}
}
