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

public class ShopSlotView : ItemSlotView, ISubmitHandler, IEventSystemHandler
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
	private GameObject slotBoxParent;

	[SerializeField]
	private GameObject soldOutParent;

	[SerializeField]
	private Image carpetImage;

	[SerializeField]
	private DataSpriteTable carpetOnSprites;

	[SerializeField]
	private DataSpriteTable carpetOffSprites;

	[SerializeField]
	private List<LocalizedFont> localizedFonts;

	public ShopSlot ShopSlot
	{
		get
		{
			return base.ItemSlot as ShopSlot;
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

	public override void OnBeginDrag(PointerEventData eventData)
	{
		ShopSlot shopSlot = ShopSlot;
		if (shopSlot == null || !shopSlot.IsSoldOut)
		{
			base.OnBeginDrag(eventData);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		InventoryManager.InventoryView.DraggableItem.TargetItemSlot = base.ItemSlot;
		base.OnPointerEnter(eventData);
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Shopping)
		{
			return;
		}
		ShopSlot shopSlot = ShopSlot;
		if (shopSlot != null && shopSlot.Item != null && !shopSlot.IsSoldOut)
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.CurrentlyFocusedSlot = ShopSlot;
			priceBox.enabled = true;
			if (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
			{
				DisplayTooltip(display: true);
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CurrentlyFocusedSlot == ShopSlot)
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.CurrentlyFocusedSlot = null;
			DisplayTooltip(display: false);
		}
		priceBox.enabled = false;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (ShopSlot == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		slotBoxParent.SetActive(!ShopSlot.IsSoldOut);
		priceParent.SetActive(!ShopSlot.IsSoldOut);
		soldOutParent.SetActive(ShopSlot.IsSoldOut);
		carpetImage.sprite = (ShopSlot.IsSoldOut ? carpetOffSprites.GetSpriteAt(0) : carpetOnSprites.GetSpriteAt((int)(ShopSlot.Item.Rarity - 1)));
		ClearSlot();
		if (!ShopSlot.IsSoldOut)
		{
			if (ShopSlot.Item == null)
			{
				base.ItemIcon.enabled = false;
				base.ItemIconBG.enabled = false;
			}
			else
			{
				base.ItemIcon.sprite = ItemView.GetUiSprite(base.ItemSlot.Item.ItemDefinition.ArtId);
				base.ItemIcon.enabled = true;
				base.ItemIconBG.sprite = ItemView.GetUiSprite(base.ItemSlot.Item.ItemDefinition.ArtId, isBG: true);
				base.ItemIconBG.enabled = true;
				base.ItemIconBG.color = iconRarityColors.GetColorAt((int)(base.ItemSlot.Item.Rarity - 1));
				DisplayRarity(ShopSlot.Item.Rarity);
				priceText.text = (ShopSlot.Item.HasBeenSoldBefore ? ShopSlot.Item.SellingPrice.ToString() : ShopSlot.Item.FinalPrice.ToString());
				priceText.enabled = true;
				priceBox.sprite = priceBoxSprites.GetSpriteAt((int)(ShopSlot.Item.Rarity - 1));
				RefreshPriceColor();
				RefreshLocalizedFonts();
			}
			if (base.HasFocus)
			{
				DisplayTooltip(display: true);
			}
		}
	}

	public void RefreshPriceColor()
	{
		if (ShopSlot?.Item != null && !ShopSlot.IsSoldOut)
		{
			int num = (ShopSlot.Item.HasBeenSoldBefore ? ShopSlot.Item.SellingPrice : ShopSlot.Item.FinalPrice);
			priceText.color = ((TPSingleton<ResourceManager>.Instance.Gold >= num) ? TPSingleton<ShopView>.Instance.ValidPriceColor._Color : TPSingleton<ShopView>.Instance.InvalidPriceColor._Color);
		}
	}

	public void RefreshLocalizedFonts()
	{
		localizedFonts?.ForEach(delegate(LocalizedFont x)
		{
			x.RefreshFont();
		});
	}

	public void Show()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void Toggle(bool toggle)
	{
		priceText.enabled = toggle;
	}

	public void OnJoystickSelect()
	{
		OnPointerEnter(null);
		TPSingleton<ShopView>.Instance.OnShelvesItemSelected(base.transform as RectTransform);
	}

	private void OnTooltipsToggled(bool show)
	{
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CurrentlyFocusedSlot == ShopSlot)
		{
			DisplayTooltip(show);
		}
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
		return ShopSlot?.Item;
	}

	protected override void OnDoubleClick()
	{
		if (base.ItemSlot.Item != null)
		{
			base.OnDoubleClick();
			if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.TryBuyItem(ShopSlot))
			{
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
			}
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
}
