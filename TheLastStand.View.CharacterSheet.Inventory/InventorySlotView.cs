using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.CharacterSheet.Inventory;

public class InventorySlotView : ItemSlotView
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	public int ItemIndex { get; set; }

	public InventorySlot InventorySlot
	{
		get
		{
			return base.ItemSlot as InventorySlot;
		}
		set
		{
			base.ItemSlot = value;
		}
	}

	public override void DisplayRarity(ItemDefinition.E_Rarity rarity, float offsetColorValue = 0f)
	{
		base.DisplayRarity(rarity, offsetColorValue);
		ToggleRarityParticles(InventorySlot.InventoryView.IsOpened && TPSingleton<CharacterSheetPanel>.Instance.IsOpened);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);
		ClearSlot();
		TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		InventoryManager.InventoryView.DraggableItem.TargetItemSlot = base.ItemSlot;
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView = this;
		if (!InventoryManager.InventoryView.DraggableItem.Displayed && (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			DisplayTooltip(display: true);
		}
		if (base.ItemSlot?.Item != null)
		{
			TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView == this)
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView = null;
			InventoryManager.InventoryView.DraggableItem.TargetItemSlot = null;
			DisplayTooltip(display: false);
			if (base.ItemSlot?.Item != null && TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
			{
				TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
			}
		}
	}

	public void OnJoystickSelect()
	{
		OnPointerEnter(null);
		InventorySlot.InventoryView.OnSlotViewJoystickSelect(this);
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnJoystickSubmit()
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.SlotSelectionToggleThisFrame)
		{
			return;
		}
		TheLastStand.Model.Item.Item item = GetItem();
		if (item == null)
		{
			return;
		}
		List<EquipmentSlot> compatibleSlots = TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetCompatibleSlots(item);
		if (compatibleSlots.Count <= 1)
		{
			OnDoubleClick();
			if (joystickHighlighter != null)
			{
				joystickHighlighter.HideButtons = GetItem() == null;
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.UpdateGamepadInputDisplays();
			}
			return;
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace = InventorySlot;
		TPSingleton<HUDJoystickNavigationManager>.Instance.StartCoroutine(TPSingleton<HUDJoystickNavigationManager>.Instance.ToggleSlotSelectionCoroutine());
		foreach (EquipmentSlot item2 in compatibleSlots)
		{
			item2.EquipmentSlotView.OnJoystickSelectionBegin();
		}
		EventSystem.current.SetSelectedGameObject(compatibleSlots[0].EquipmentSlotView.gameObject);
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearSlot();
		TPSingleton<BuildingManager>.Instance.Shop.ShopInventorySlots[ItemIndex].Item = base.ItemSlot.Item;
		TPSingleton<BuildingManager>.Instance.Shop.ShopInventorySlots[ItemIndex].ShopInventorySlotView.Refresh();
		if (base.ItemSlot.Item == null)
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
		}
		if (base.HasFocus && (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			DisplayTooltip(display: true);
		}
	}

	protected override void ClearSlot()
	{
		base.ClearSlot();
		DisplayRarity(GetItem()?.Rarity ?? ItemDefinition.E_Rarity.None);
	}

	protected override TheLastStand.Model.Item.Item GetItem()
	{
		return base.ItemSlot.Item;
	}

	protected override void OnDoubleClick()
	{
		if (base.ItemSlot.Item != null && !InventoryManager.InventoryView.DraggableItem.Displayed)
		{
			base.OnDoubleClick();
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.OnInventorySlotDoubleClick(InventorySlot);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
		}
	}

	protected override void ToggleRarityParticlesHook(bool enable)
	{
		if (enable)
		{
			if (!isParticleSystemHooked)
			{
				TPSingleton<CharacterSheetPanel>.Instance.OnCharacterSheetToggle += OnCharacterSheetToggled;
				InventorySlot.InventoryView.OnInventoryToggle += base.ToggleRarityParticles;
			}
		}
		else if (isParticleSystemHooked)
		{
			TPSingleton<CharacterSheetPanel>.Instance.OnCharacterSheetToggle -= OnCharacterSheetToggled;
			InventorySlot.InventoryView.OnInventoryToggle -= base.ToggleRarityParticles;
		}
		isParticleSystemHooked = enable;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	public override void DisplayTooltip(bool display)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CharacterSheet && !TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened)
		{
			return;
		}
		if (base.ItemSlot == null)
		{
			CLoggerManager.Log("ItemSlot can't be null! Aborting...", this, LogType.Error);
			return;
		}
		UnregisterComparedItems();
		if (display)
		{
			EquipmentSlot equipmentSlot = null;
			PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
			if (base.ItemSlot.Item != null)
			{
				equipmentSlot = selectedPlayableUnit.PlayableUnitController.GetBestItemSlotToCompare(InventorySlot.Item);
				RegisterComparedItems(equipmentSlot, selectedPlayableUnit);
				CharacterSheetPanel.ItemTooltip.FollowElement.ChangeFollowDatas(followDatas);
			}
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.SetContent(base.ItemSlot.Item, selectedPlayableUnit);
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Display();
			if (equipmentSlot?.Item != null)
			{
				CharacterSheetPanel.ItemTooltip.SetContent(equipmentSlot.Item, selectedPlayableUnit);
				CharacterSheetPanel.ItemTooltip.Display();
			}
			else
			{
				CharacterSheetPanel.ItemTooltip.Hide();
			}
		}
		else
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Hide();
			CharacterSheetPanel.ItemTooltip.Hide();
		}
	}

	private void OnCharacterSheetToggled(bool enable)
	{
		if (!enable)
		{
			ToggleRarityParticles(enable: false);
		}
	}

	private void OnDestroy()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (base.HasFocus)
		{
			DisplayTooltip(showTooltips);
		}
	}
}
