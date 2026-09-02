using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.View.Generic;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet;

public class EquipmentSlotView : ItemSlotView
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private Image slotImage;

	[SerializeField]
	private Sprite slotPotentialTargetSprite;

	[SerializeField]
	private Image chainImage;

	private Sprite slotNormalSprite;

	private Navigation? slotNavigationBackup;

	private bool isDraggableItemHover;

	public static bool ForceAllowBeginDrag { get; private set; }

	public override bool CanBeginDrag
	{
		get
		{
			if (!(TPSingleton<GameManager>.Instance != null) || TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				return ForceAllowBeginDrag;
			}
			return true;
		}
	}

	public EquipmentSlot EquipmentSlot
	{
		get
		{
			return base.ItemSlot as EquipmentSlot;
		}
		set
		{
			base.ItemSlot = value;
		}
	}

	public override void DisplayRarity(ItemDefinition.E_Rarity rarity, float offsetColorValue = 0f)
	{
		base.DisplayRarity(rarity, offsetColorValue);
		ToggleRarityParticles(TPSingleton<CharacterSheetPanel>.Instance.IsOpened);
	}

	public override void DisplayTooltip(bool display)
	{
		DisplayTooltip(display, GetItem(excludeSimulatedItem: false));
	}

	private void DisplayTooltip(bool display, TheLastStand.Model.Item.Item item)
	{
		if (display)
		{
			CharacterSheetPanel.ItemTooltip.FollowElement.ChangeFollowDatas(followDatas);
			CharacterSheetPanel.ItemTooltip.SetContent(item, TileObjectSelectionManager.SelectedPlayableUnit);
			CharacterSheetPanel.ItemTooltip.Display();
		}
		else
		{
			CharacterSheetPanel.ItemTooltip.Hide();
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver)
		{
			base.OnBeginDrag(eventData);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver)
		{
			base.OnEndDrag(eventData);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (!base.HasFocus)
		{
			base.HasFocus = true;
			TPSingleton<CharacterSheetPanel>.Instance.FocusedEquipmentSlotView = this;
		}
		InventoryManager.InventoryView.DraggableItem.TargetItemSlot = base.ItemSlot;
		TheLastStand.Model.Item.Item item = GetItem(excludeSimulatedItem: false);
		if ((item != null && !InventoryManager.InventoryView.DraggableItem.Displayed) || IsSlotValid(InventoryManager.InventoryView.DraggableItem.ItemSlot))
		{
			slotImage.enabled = true;
			slotImage.sprite = ((base.ItemSlot != null) ? slotPotentialTargetSprite : lockedHighlightSprite);
			TPSingleton<UIManager>.Instance.PlayAudioClipWithoutInterrupting(UIManager.ButtonHoverAudioClip);
		}
		if (item != null && TileObjectSelectionManager.HasPlayableUnitSelected && !InventoryManager.InventoryView.DraggableItem.Displayed && (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			DisplayTooltip(display: true, item);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		TPSingleton<CharacterSheetPanel>.Instance.FocusedEquipmentSlotView = null;
		CharacterSheetPanel.ItemTooltip.Hide();
		if (InventoryManager.InventoryView.DraggableItem.TargetItemSlot == base.ItemSlot)
		{
			InventoryManager.InventoryView.DraggableItem.TargetItemSlot = null;
		}
		slotImage.enabled = base.ItemSlot != null;
		slotImage.sprite = slotNormalSprite;
	}

	public void OnJoystickSelect()
	{
		OnPointerEnter(null);
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnJoystickSubmit()
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace != null)
		{
			if (!TPSingleton<HUDJoystickNavigationManager>.Instance.SlotSelectionToggleThisFrame)
			{
				OnSlotJoystickSelected();
				TPSingleton<CharacterSheetPanel>.Instance.OnEquipmentSlotJoystickSelectionOver();
			}
			return;
		}
		OnDoubleClick();
		if (joystickHighlighter != null)
		{
			joystickHighlighter.HideButtons = GetItem() == null;
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.UpdateGamepadInputDisplays();
		}
	}

	public void OnJoystickSelectionBegin()
	{
		if (!slotNavigationBackup.HasValue)
		{
			slotNavigationBackup = joystickSelectable.navigation;
			if (!IsSelectableSlotValid(slotNavigationBackup.Value.selectOnDown))
			{
				joystickSelectable.SetSelectOnDown(null);
			}
			if (!IsSelectableSlotValid(slotNavigationBackup.Value.selectOnUp))
			{
				joystickSelectable.SetSelectOnUp(null);
			}
			if (!IsSelectableSlotValid(slotNavigationBackup.Value.selectOnRight))
			{
				joystickSelectable.SetSelectOnRight(null);
			}
			if (!IsSelectableSlotValid(slotNavigationBackup.Value.selectOnLeft))
			{
				joystickSelectable.SetSelectOnLeft(null);
			}
		}
	}

	public void OnSlotJoystickSelected()
	{
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.OnEquipmentSlotSelected(EquipmentSlot);
	}

	public void RestoreJoystickNavigation()
	{
		if (slotNavigationBackup.HasValue)
		{
			joystickSelectable.navigation = slotNavigationBackup.Value;
			slotNavigationBackup = null;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		base.gameObject.SetActive(value: true);
		chainImage.enabled = base.ItemSlot == null;
		slotImage.enabled = base.ItemSlot != null;
		if (base.ItemSlot == null)
		{
			levelBadge.Refresh(-1);
			base.ItemIcon.enabled = false;
			base.ItemIcon.sprite = null;
			base.ItemIconBG.enabled = false;
			base.ItemIconBG.sprite = null;
			base.ItemIcon.color = Color.white;
			base.BackgroundImage.enabled = false;
			base.BackgroundImage.sprite = null;
			return;
		}
		DraggedItem draggableItem = InventoryManager.InventoryView.DraggableItem;
		TheLastStand.Model.Item.Item item = TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView?.ItemSlot?.Item;
		ClearSlot();
		TheLastStand.Model.Item.Item item2 = GetItem(excludeSimulatedItem: false);
		if (item2 == null)
		{
			base.ItemIcon.enabled = false;
			base.ItemIcon.sprite = null;
			base.ItemIconBG.sprite = null;
			ChangeBackgroundColor(Color.white);
		}
		else
		{
			base.ItemIcon.sprite = ItemView.GetUiSprite(item2.ItemDefinition.ArtId);
			base.ItemIcon.enabled = true;
			base.ItemIconBG.sprite = ItemView.GetUiSprite(item2.ItemDefinition.ArtId, isBG: true);
			base.ItemIconBG.color = iconRarityColors.GetColorAt((int)(item2.Rarity - 1));
			if (EquipmentSlot.BlockedByOtherSlot != null)
			{
				Color color = new Color(1f, 1f, 1f, 0.5f);
				ChangeBackgroundColor(color);
				base.ItemIcon.color = color;
				DisplayRarity(item2.Rarity, -0.5f);
			}
			else
			{
				ChangeBackgroundColor(Color.white);
				DisplayRarity(item2.Rarity);
			}
		}
		if (base.HasFocus && TileObjectSelectionManager.HasPlayableUnitSelected && (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			DisplayTooltip(display: true, item2);
		}
		RefreshSlotValidity(draggableItem != null && draggableItem.Displayed, item != null);
	}

	public void RefreshSlotValidity(bool isDragging, bool isHovering)
	{
		bool flag = true;
		if (isDragging)
		{
			flag = !chainImage.enabled && IsSlotValid(InventoryManager.InventoryView.DraggableItem.ItemSlot);
		}
		else if (isHovering)
		{
			flag = !chainImage.enabled && IsSlotValid(TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView.ItemSlot);
		}
		slotImage.color = (flag ? Color.white : itemSlotInvalidColor._Color);
		base.ItemIconBG.enabled = GetItem() != null && flag;
		base.ItemIcon.color = ((!flag) ? itemSlotInvalidColor._Color : ((EquipmentSlot?.BlockedByOtherSlot != null) ? new Color(1f, 1f, 1f, 0.5f) : Color.white));
		ChangeBackgroundColor((!flag) ? itemSlotInvalidColor._Color : ((EquipmentSlot?.BlockedByOtherSlot != null) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white));
		if (base.ItemSlot != null)
		{
			TPSingleton<CharacterSheetPanel>.Instance.SetBackgroundColorForSlot(base.ItemSlot.ItemSlotDefinition.Id, flag ? Color.white : itemSlotInvalidColor._Color);
		}
	}

	protected override void ClearSlot()
	{
		base.ClearSlot();
		TheLastStand.Model.Item.Item item = GetItem(excludeSimulatedItem: false);
		TheLastStand.Model.Item.Item item2 = GetItem();
		if (item != null)
		{
			DisplayRarity(item.Rarity, (item2 != null) ? 0f : (-0.5f));
		}
		else
		{
			DisplayRarity(ItemDefinition.E_Rarity.None);
		}
	}

	protected override TheLastStand.Model.Item.Item GetItem()
	{
		return GetItem();
	}

	protected override void OnDoubleClick()
	{
		if (base.ItemSlot?.Item != null && !InventoryManager.InventoryView.DraggableItem.Displayed)
		{
			base.OnDoubleClick();
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.OnEquipmentSlotDoubleClick(EquipmentSlot);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		slotNormalSprite = slotImage.sprite;
		GenerateRaritiesParticlesDictionary();
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	protected override void ToggleRarityParticlesHook(bool enable)
	{
		if (enable)
		{
			if (!isParticleSystemHooked)
			{
				TPSingleton<CharacterSheetPanel>.Instance.OnCharacterSheetToggle += base.ToggleRarityParticles;
			}
		}
		else if (isParticleSystemHooked)
		{
			TPSingleton<CharacterSheetPanel>.Instance.OnCharacterSheetToggle -= base.ToggleRarityParticles;
		}
		isParticleSystemHooked = enable;
	}

	private TheLastStand.Model.Item.Item GetItem(bool excludeSimulatedItem = true)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			return null;
		}
		if (base.ItemSlot == null)
		{
			return null;
		}
		if (excludeSimulatedItem || EquipmentSlot.BlockedByOtherSlot == null)
		{
			return EquipmentSlot.Item;
		}
		return EquipmentSlot.BlockedByOtherSlot.Item;
	}

	private bool IsSelectableSlotValid(Selectable neighbourSelectable)
	{
		if (neighbourSelectable != null && neighbourSelectable.TryGetComponent<EquipmentSlotView>(out var component))
		{
			return component.IsSlotValid(TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace);
		}
		return false;
	}

	private bool IsSlotValid(ItemSlot itemSlot)
	{
		if (itemSlot?.Item != null)
		{
			EquipmentSlot equipmentSlot = EquipmentSlot;
			if (equipmentSlot != null && equipmentSlot.BlockedByOtherSlot == null && EquipmentSlot.EquipmentSlotController.IsItemCompatible(itemSlot.Item))
			{
				if (EquipmentSlot.Item != null)
				{
					return itemSlot.ItemSlotController.IsItemCompatible(EquipmentSlot.Item);
				}
				return true;
			}
		}
		return false;
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

	[DevConsoleCommand("EquipmentSlotForceAllowDragItem")]
	public static void DebugEquipmentSlotForceAllowDragItem(bool allowDragItem = true)
	{
		ForceAllowBeginDrag = allowDragItem;
	}
}
