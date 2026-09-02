using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using TPLib.Localization;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class WeaponRestrictionsTooltipCategoryPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI itemCategoryTitle;

	[SerializeField]
	private Image itemCategoryIcon;

	[SerializeField]
	private TextMeshProUGUI selectedFamiliesDescription;

	private ItemDefinition.E_Category currentItemCategory;

	private List<ItemRestrictionFamilyDefinition> currentFamilyDefinitions;

	public void SetContent(ItemDefinition.E_Category itemCategory, List<ItemRestrictionFamilyDefinition> categoryDefinitions)
	{
		currentItemCategory = itemCategory;
		currentFamilyDefinitions = categoryDefinitions;
		RefreshIcon();
		RefreshText();
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshText));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshText));
	}

	private void RefreshIcon()
	{
		if (currentItemCategory != ItemDefinition.E_Category.None)
		{
			switch (currentItemCategory)
			{
			case ItemDefinition.E_Category.MagicWeapon:
				itemCategoryIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_MagicalDamage");
				break;
			case ItemDefinition.E_Category.MeleeWeapon:
				itemCategoryIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_PhysicalDamage");
				break;
			case ItemDefinition.E_Category.RangeWeapon:
				itemCategoryIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_RangedDamage");
				break;
			case ItemDefinition.E_Category.MeleeWeapon | ItemDefinition.E_Category.RangeWeapon:
				break;
			}
		}
	}

	private void RefreshText()
	{
		if (currentItemCategory == ItemDefinition.E_Category.None)
		{
			return;
		}
		itemCategoryTitle.text = Localizer.Get(string.Format("{0}{1}", "ItemRestrictionCategoryTooltipName_", currentItemCategory));
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ItemRestrictionFamilyDefinition currentFamilyDefinition in currentFamilyDefinitions)
		{
			stringBuilder.Append("<style=DarkShopKW>• " + Localizer.Get(currentFamilyDefinition.LocalKey) + "</style>\r\n");
		}
		selectedFamiliesDescription.text = stringBuilder.ToString();
	}
}
