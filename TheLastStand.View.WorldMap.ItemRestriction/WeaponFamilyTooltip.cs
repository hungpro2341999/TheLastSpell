using TMPro;
using TPLib.Localization;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class WeaponFamilyTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI actionText;

	private ItemRestrictionFamily currentItemFamily;

	private ItemRestrictionCategoriesCollection currentCategoriesCollection;

	public void Init(ItemRestrictionFamily itemRestrictionFamily, ItemRestrictionCategoriesCollection itemRestrictionCategoriesCollection)
	{
		currentItemFamily = itemRestrictionFamily;
		currentCategoriesCollection = itemRestrictionCategoriesCollection;
	}

	protected override bool CanBeDisplayed()
	{
		if (currentItemFamily != null)
		{
			return currentCategoriesCollection != null;
		}
		return false;
	}

	protected override void RefreshContent()
	{
		if (currentItemFamily.IsSelected)
		{
			if (currentCategoriesCollection.GetCanUnSelectItemFamilyFromCategory(currentItemFamily.ItemFamilyDefinition.ItemCategory))
			{
				actionText.text = Localizer.Format("WeaponFamilyDisplay_Tooltip_SelectedWeapon", currentItemFamily.LocalizedName);
			}
			else
			{
				actionText.text = Localizer.Format("WeaponFamilyDisplay_Tooltip_LockedWeapon", currentItemFamily.LocalizedName);
			}
		}
		else
		{
			actionText.text = Localizer.Format("WeaponFamilyDisplay_Tooltip_UnselectedWeapon", currentItemFamily.LocalizedName);
		}
	}
}
