using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Manager.Item;
using UnityEngine;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class WeaponFamiliesCountDisplay : MonoBehaviour
{
	[SerializeField]
	private ItemFamiliesActiveCountDisplay meleeWeaponCountDisplay;

	[SerializeField]
	private ItemFamiliesActiveCountDisplay rangeWeaponCountDisplay;

	[SerializeField]
	private ItemFamiliesActiveCountDisplay magicWeaponCountDisplay;

	private List<ItemFamiliesActiveCountDisplay> weaponFamiliesCountDisplays;

	public void Init()
	{
		weaponFamiliesCountDisplays = new List<ItemFamiliesActiveCountDisplay> { meleeWeaponCountDisplay, rangeWeaponCountDisplay, magicWeaponCountDisplay };
		List<ItemDefinition.E_Category> list = new List<ItemDefinition.E_Category>
		{
			ItemDefinition.E_Category.MeleeWeapon,
			ItemDefinition.E_Category.RangeWeapon,
			ItemDefinition.E_Category.MagicWeapon
		};
		for (int i = 0; i < list.Count(); i++)
		{
			weaponFamiliesCountDisplays[i].Init(TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories, list[i]);
		}
	}

	public void Refresh()
	{
		foreach (ItemFamiliesActiveCountDisplay weaponFamiliesCountDisplay in weaponFamiliesCountDisplays)
		{
			weaponFamiliesCountDisplay.Refresh();
		}
	}
}
