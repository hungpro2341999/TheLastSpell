using TheLastStand.Definition.Item;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemIcon : OraculumUnlockIcon
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private Image foreground;

	[SerializeField]
	private ItemTooltipDisplayer itemTooltipDisplayer;

	[SerializeField]
	private Color darkShopBackgroundColor = Color.white;

	[SerializeField]
	private Color lightShopBackgroundColor = Color.white;

	public void Init(ItemDefinition itemDefinition, MetaUpgradeLineView containerUpgrade, ItemTooltip itemTooltip, bool isLightShop)
	{
		background.sprite = ItemView.GetUiSprite(itemDefinition.Id, isBG: true);
		foreground.sprite = ItemView.GetUiSprite(itemDefinition.Id);
		background.color = (isLightShop ? lightShopBackgroundColor : darkShopBackgroundColor);
		itemTooltipDisplayer.Init(itemDefinition, itemTooltip, isLightShop);
		SetMetaUpgrade(containerUpgrade);
	}

	private void OnDisable()
	{
		if (itemTooltipDisplayer.IsDisplayingTargetTooltip)
		{
			itemTooltipDisplayer.HideTooltip();
		}
	}
}
