using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item.ItemRestriction;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class WeaponsRestrictionsTooltipDisplayer : SpriteChangeTooltipDisplayer
{
	[SerializeField]
	private Sprite customModeSpriteOff;

	[SerializeField]
	private Sprite customModeSpriteOn;

	private Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> restrictionFamilyDefinitions;

	private bool isBoundless;

	private Sprite initOffSprite;

	private Sprite initOnSprite;

	private WeaponRestrictionsTooltip WeaponRestrictionsTooltip => targetTooltip as WeaponRestrictionsTooltip;

	public void SetRestrictionFamilyDefinitions(Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> newRestrictionFamilyDefinitions)
	{
		restrictionFamilyDefinitions = newRestrictionFamilyDefinitions;
	}

	public void SetIsBoundless(bool newIsBoundless)
	{
		isBoundless = newIsBoundless;
		offSprite = (isBoundless ? customModeSpriteOff : initOffSprite);
		onSprite = (isBoundless ? customModeSpriteOn : initOnSprite);
		image.sprite = offSprite;
	}

	private void Start()
	{
		initOffSprite = offSprite;
		initOnSprite = onSprite;
		Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> familiesToShowFromManager = GetFamiliesToShowFromManager();
		SetRestrictionFamilyDefinitions(familiesToShowFromManager);
		SetIsBoundless(TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive);
	}

	private static Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> GetFamiliesToShowFromManager()
	{
		Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> dictionary = new Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>>();
		foreach (KeyValuePair<ItemDefinition.E_Category, List<ItemRestrictionFamily>> item in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory)
		{
			List<ItemRestrictionFamilyDefinition> list = new List<ItemRestrictionFamilyDefinition>();
			foreach (ItemRestrictionFamily item2 in item.Value)
			{
				if (item2.IsActive)
				{
					list.Add(item2.ItemFamilyDefinition);
				}
			}
			dictionary.Add(item.Key, list);
		}
		return dictionary;
	}

	public override void DisplayTooltip()
	{
		if (restrictionFamilyDefinitions != null)
		{
			WeaponRestrictionsTooltip.SetRestrictionFamilyDefinitions(restrictionFamilyDefinitions);
			WeaponRestrictionsTooltip.SetIsBoundless(isBoundless);
		}
		WeaponRestrictionsTooltip.FollowElement.ChangeTarget(base.transform);
		base.DisplayTooltip();
	}
}
