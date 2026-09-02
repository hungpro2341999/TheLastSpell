using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.View.Item;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class ItemTooltipDisplayer : IconTooltipDisplayer
{
	private ItemDefinition ItemDefinition;

	private ItemTooltip ItemTooltip => targetTooltip as ItemTooltip;

	public override void DisplayTooltip()
	{
		int lowerExistingLevelFromInitValue = ItemDefinition.GetLowerExistingLevelFromInitValue(0);
		ItemTooltip.SetContent(new ItemController(ItemDefinition, lowerExistingLevelFromInitValue, ItemDefinition.E_Rarity.Common).Item, null, newUseDefaultValues: true);
		base.DisplayTooltip();
	}

	public override void HideTooltip()
	{
		base.HideTooltip();
		((RectTransform)ItemTooltip.ItemTooltipSkillCycle.transform).pivot = oldPivot;
	}

	public void Init(ItemDefinition newItemDefinition, ItemTooltip itemTooltip = null, bool isLightShop = false)
	{
		ItemDefinition = newItemDefinition;
		Init(itemTooltip, isLightShop);
	}

	protected override void UpdateAnchor()
	{
		base.UpdateAnchor();
		((RectTransform)ItemTooltip.ItemTooltipSkillCycle.transform).pivot = (isFromLightShop ? Vector2.one : Vector2.up);
	}
}
