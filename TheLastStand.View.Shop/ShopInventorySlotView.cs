using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.View.Generic;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Shop;

public class ShopInventorySlotView : ItemSlotView, ISubmitHandler, IEventSystemHandler
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private GameObject priceParent;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private Image priceBox;

	[SerializeField]
	private DataSpriteTable priceBoxSprites;

	[SerializeField]
	private List<LocalizedFont> localizedFonts;

	public int ItemIndex { get; set; }

	public ShopInventorySlot ShopInventorySlot
	{
		get
		{
			return base.ItemSlot as ShopInventorySlot;
		}
		set
		{
			base.ItemSlot = value;
		}
	}

	public override void DisplayRarity(ItemDefinition.E_Rarity rarity, float offsetColorValue = 0f)
	{
		base.DisplayRarity(rarity, offsetColorValue);
		ToggleRarityParticles(TPSingleton<ShopView>.Instance.Shop.IsOpened);
	}

	public override void DisplayTooltip(bool display)
	{
		DisplayTooltip(display, TPSingleton<BuildingManager>.Instance.Shop.UnitToCompareIndex, followDatas);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		InventoryManager.InventoryView.DraggableItem.TargetItemSlot = base.ItemSlot;
		base.OnPointerEnter(eventData);
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping)
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.CurrentlyFocusedInventorySlot = ShopInventorySlot;
			if (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
			{
				DisplayTooltip(display: true);
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (InventoryManager.InventoryView.DraggableItem.TargetItemSlot == base.ItemSlot)
		{
			InventoryManager.InventoryView.DraggableItem.TargetItemSlot = null;
		}
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CurrentlyFocusedInventorySlot == ShopInventorySlot)
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.CurrentlyFocusedInventorySlot = null;
			DisplayTooltip(display: false);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearSlot();
		if (base.ItemSlot.Item == null)
		{
			base.ItemIcon.enabled = false;
			base.ItemIconBG.enabled = false;
			priceText.enabled = false;
			priceParent.SetActive(value: false);
		}
		else
		{
			priceParent.SetActive(value: true);
			priceText.enabled = true;
			priceBox.sprite = priceBoxSprites.GetSpriteAt((int)(base.ItemSlot.Item.Rarity - 1));
			base.ItemIcon.sprite = ItemView.GetUiSprite(base.ItemSlot.Item.ItemDefinition.ArtId);
			base.ItemIcon.enabled = true;
			base.ItemIconBG.sprite = ItemView.GetUiSprite(base.ItemSlot.Item.ItemDefinition.ArtId, isBG: true);
			base.ItemIconBG.enabled = true;
			base.ItemIconBG.color = iconRarityColors.GetColorAt((int)(base.ItemSlot.Item.Rarity - 1));
			DisplayRarity(base.ItemSlot.Item.Rarity);
		}
		RefreshPrice();
		RefreshLocalizedFonts();
		if (base.HasFocus)
		{
			DisplayTooltip(display: true);
		}
	}

	public void RefreshPrice()
	{
		priceText.text = ((base.ItemSlot.Item == null) ? "0" : base.ItemSlot.Item.SellingPrice.ToString());
	}

	public void RefreshLocalizedFonts()
	{
		localizedFonts?.ForEach(delegate(LocalizedFont x)
		{
			x.RefreshFont();
		});
	}

	public void Toggle(bool toggle)
	{
		priceText.enabled = toggle && base.ItemSlot.Item != null;
	}

	public void OnJoystickSelect()
	{
		OnPointerEnter(null);
		TPSingleton<ShopView>.Instance.OnInventoryItemSelected(base.transform as RectTransform);
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OnDoubleClick();
	}

	protected override void ClearSlot()
	{
		base.ClearSlot();
		DisplayRarity(GetItem()?.Rarity ?? ItemDefinition.E_Rarity.None);
	}

	protected override TheLastStand.Model.Item.Item GetItem()
	{
		return base.ItemSlot?.Item;
	}

	protected override void OnDoubleClick()
	{
		if (base.ItemSlot?.Item != null)
		{
			base.OnDoubleClick();
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.TrySellItem(ShopInventorySlot);
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
		}
	}

	protected override void ToggleRarityParticlesHook(bool enable)
	{
		if (enable)
		{
			if (!isParticleSystemHooked)
			{
				TPSingleton<ShopView>.Instance.Shop.ShopController.OnShopToggle += base.ToggleRarityParticles;
			}
		}
		else if (isParticleSystemHooked)
		{
			TPSingleton<ShopView>.Instance.Shop.ShopController.OnShopToggle -= base.ToggleRarityParticles;
		}
		isParticleSystemHooked = enable;
	}

	private void OnEnable()
	{
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDisable()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool show)
	{
		if (InventoryManager.InventoryView.DraggableItem.TargetItemSlot == base.ItemSlot)
		{
			DisplayTooltip(show);
		}
	}
}
