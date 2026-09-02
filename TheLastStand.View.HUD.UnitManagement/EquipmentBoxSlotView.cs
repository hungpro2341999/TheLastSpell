using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class EquipmentBoxSlotView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private Image equippedSlotImage;

	[SerializeField]
	private Image equippedSlotBGImage;

	[SerializeField]
	private Image equippedSlotLockedImage;

	[SerializeField]
	private DataColorTable rarityColors;

	[SerializeField]
	private bool shouldAttenuateWhenTwoHandedWeapon;

	private bool hasFocus;

	public TheLastStand.Model.Item.Item item { get; private set; }

	public void DisplayTooltip(bool display)
	{
		if (display && TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.FollowElement.ChangeFollowDatas(followDatas);
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.SetContent(item, TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Display();
		}
		else
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Hide();
		}
	}

	public void DisplayLockedImage(bool display)
	{
		equippedSlotLockedImage.enabled = display;
	}

	public void Refresh(TheLastStand.Model.Item.Item item, bool isTwoHandedWeapon = false)
	{
		this.item = item;
		equippedSlotImage.enabled = this.item != null;
		equippedSlotBGImage.enabled = this.item != null;
		if (hasFocus)
		{
			OnPointerEnter(null);
		}
		if (this.item != null)
		{
			equippedSlotImage.sprite = ItemView.GetUiSprite(this.item.ItemDefinition.ArtId);
			equippedSlotBGImage.sprite = ItemView.GetUiSprite(this.item.ItemDefinition.ArtId, isBG: true);
			Color colorAt = rarityColors.GetColorAt((int)(this.item.Rarity - 1));
			equippedSlotImage.color = ((shouldAttenuateWhenTwoHandedWeapon && isTwoHandedWeapon) ? Color.white.WithA(0.5f) : Color.white);
			equippedSlotBGImage.color = ((shouldAttenuateWhenTwoHandedWeapon && isTwoHandedWeapon) ? colorAt.WithA(0.5f) : colorAt);
		}
	}

	public void RefreshColor(Color color, bool isTwoHandedWeapon = false)
	{
		if (isTwoHandedWeapon && shouldAttenuateWhenTwoHandedWeapon)
		{
			color.a = 0.5f;
		}
		equippedSlotImage.color = color;
		if (item != null)
		{
			equippedSlotBGImage.color = rarityColors.GetColorAt((int)(item.Rarity - 1)) * color;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hasFocus = true;
		DisplayTooltip(display: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (hasFocus)
		{
			hasFocus = false;
			DisplayTooltip(display: false);
		}
	}
}
