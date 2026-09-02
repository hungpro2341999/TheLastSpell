using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class WeaponRestrictionsCategoryPanel : MonoBehaviour
{
	[SerializeField]
	private float scrollSensitivity = 0.1f;

	[SerializeField]
	private RectTransform weaponFamilyDisplaysContainer;

	[SerializeField]
	private RectTransform weaponFamiliesViewport;

	[SerializeField]
	private Scrollbar weaponFamiliesScrollbar;

	[SerializeField]
	private GridLayoutGroup gridLayoutGroup;

	[SerializeField]
	private WeaponFamilyDisplay weaponFamilyDisplayPrefab;

	[SerializeField]
	private LayoutNavigationInitializer weaponFamiliesGridNavigationInitializer;

	public List<WeaponFamilyDisplay> WeaponFamilyDisplays { get; } = new List<WeaponFamilyDisplay>();

	public ItemDefinition.E_Category CurrentItemCategory { get; private set; }

	public GridLayoutGroup GridLayoutGroup => gridLayoutGroup;

	public void Init(ItemDefinition.E_Category itemCategory)
	{
		CurrentItemCategory = itemCategory;
		InstantiateWeaponFamilyDisplays();
		InitWeaponFamilyDisplays();
	}

	public bool TryAddNewWeaponFamilyDisplays()
	{
		int unlockedFamiliesNb = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.GetUnlockedFamiliesNb(CurrentItemCategory);
		if (unlockedFamiliesNb > WeaponFamilyDisplays.Count)
		{
			InstantiateWeaponFamilyDisplays(unlockedFamiliesNb);
			InitWeaponFamilyDisplays();
			return true;
		}
		return false;
	}

	public void OnGlyphsTopButtonClick()
	{
		weaponFamiliesScrollbar.value -= scrollSensitivity;
	}

	public void OnGlyphsBotButtonClick()
	{
		weaponFamiliesScrollbar.value += scrollSensitivity;
	}

	public void Refresh()
	{
		foreach (WeaponFamilyDisplay weaponFamilyDisplay in WeaponFamilyDisplays)
		{
			weaponFamilyDisplay.Refresh();
		}
	}

	public WeaponFamilyDisplay GetClosestWeaponFamilyDisplayFromRowIndex(int rowIndex, bool getClosestFromLeft)
	{
		int count = WeaponFamilyDisplays.Count;
		while (rowIndex >= 0)
		{
			int num = 0;
			int num2 = rowIndex * gridLayoutGroup.constraintCount;
			if (getClosestFromLeft)
			{
				num = num2;
			}
			else
			{
				int num3 = rowIndex * gridLayoutGroup.constraintCount + (gridLayoutGroup.constraintCount - 1);
				while (num3 >= count && num3 != num2)
				{
					num3--;
					if (num3 < count && WeaponFamilyDisplays[num3].gameObject.activeSelf)
					{
						return WeaponFamilyDisplays[num3];
					}
				}
				num = num3;
			}
			if (num < count && WeaponFamilyDisplays[num].gameObject.activeSelf)
			{
				return WeaponFamilyDisplays[num];
			}
			rowIndex--;
		}
		return null;
	}

	private void InitWeaponFamilyDisplays()
	{
		if (!TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory.TryGetValue(CurrentItemCategory, out var value))
		{
			return;
		}
		int count = WeaponFamilyDisplays.Count;
		List<ItemRestrictionFamily> list = value.FindAll((ItemRestrictionFamily anItemFamily) => anItemFamily.HasUnlockedItems);
		int count2 = list.Count;
		for (int num = 0; num < count; num++)
		{
			if (num < count2)
			{
				WeaponFamilyDisplays[num].gameObject.SetActive(value: true);
				WeaponFamilyDisplays[num].Init(list[num], TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories);
			}
			else
			{
				WeaponFamilyDisplays[num].gameObject.SetActive(value: false);
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)weaponFamiliesGridNavigationInitializer.transform);
		weaponFamiliesGridNavigationInitializer.InitNavigation(reset: true);
	}

	private void InstantiateWeaponFamilyDisplays(int unlockedWeaponFamilies = 0)
	{
		if (unlockedWeaponFamilies == 0)
		{
			unlockedWeaponFamilies = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.GetUnlockedFamiliesNb(CurrentItemCategory);
		}
		int num = unlockedWeaponFamilies - WeaponFamilyDisplays.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				CreatedWeaponFamilyDisplay();
			}
		}
	}

	private void CreatedWeaponFamilyDisplay()
	{
		WeaponFamilyDisplay weaponFamilyDisplay = Object.Instantiate(weaponFamilyDisplayPrefab, weaponFamilyDisplaysContainer);
		weaponFamilyDisplay.GetComponent<JoystickSelectable>().AddListenerOnSelect(delegate
		{
			OnJoystickSelect(weaponFamilyDisplay.transform as RectTransform);
		});
		WeaponFamilyDisplays.Add(weaponFamilyDisplay);
	}

	private void OnJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(source, weaponFamiliesViewport, weaponFamiliesScrollbar, 0.02f, 0f, 0.1f);
	}
}
