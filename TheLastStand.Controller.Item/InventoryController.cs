using TPLib;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.CharacterSheet.Inventory;

namespace TheLastStand.Controller.Item;

public class InventoryController
{
	public Inventory Inventory { get; }

	public InventoryController(ISerializedData container, InventoryView inventoryView)
	{
		Inventory = new Inventory(container, this, inventoryView);
		Inventory.InventoryView.Inventory = Inventory;
		GenerateInventorySlots();
		Inventory.Deserialize(container);
	}

	public InventoryController(InventoryView inventoryView)
	{
		Inventory = new Inventory(this, inventoryView);
		Inventory.InventoryView.Inventory = Inventory;
		GenerateInventorySlots();
	}

	public void AddItem(TheLastStand.Model.Item.Item item, InventorySlot inventorySlot = null, bool isNewItem = false)
	{
		if (inventorySlot == null)
		{
			inventorySlot = GetFirstAvailableSlot();
		}
		if (inventorySlot == null)
		{
			TPSingleton<ItemManager>.Instance.LogWarning("No inventory slot found to add item " + item.ItemDefinition.Id + " (inventory may be full), aborting.");
			return;
		}
		inventorySlot.ItemSlotController.SetItem(item);
		inventorySlot.IsNewItem = isNewItem;
	}

	public bool CanOpenInventory()
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CharacterSheet && !CharacterSheetManager.CanOpenCharacterSheetPanel() && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Shopping)
		{
			return false;
		}
		if (InventoryManager.DebugForceInventoryAccess)
		{
			return true;
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			return false;
		}
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Management:
		case Game.E_State.CharacterSheet:
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.BuildingPreparingAction:
		case Game.E_State.BuildingPreparingSkill:
		case Game.E_State.Construction:
		case Game.E_State.Shopping:
			return true;
		default:
			return false;
		}
	}

	public InventorySlot GetFirstAvailableSlot()
	{
		foreach (InventorySlot inventorySlot in Inventory.InventorySlots)
		{
			if (inventorySlot.Item == null)
			{
				return inventorySlot;
			}
		}
		return null;
	}

	public void MarkAllItemsAsSeen()
	{
		for (int num = Inventory.InventorySlots.Count - 1; num >= 0; num--)
		{
			Inventory.InventorySlots[num].IsNewItem = false;
		}
	}

	public void OnEquipmentSlotDoubleClick(EquipmentSlot equipmentSlot)
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened && equipmentSlot.Item != null)
		{
			if (TPSingleton<InventoryManager>.Instance.Inventory.ItemCount < TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots.Count)
			{
				Inventory.InventoryController.AddItem(equipmentSlot.Item);
			}
			if (equipmentSlot.BlockOtherSlot != null)
			{
				equipmentSlot.BlockOtherSlot = null;
			}
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened)
			{
				Inventory.InventoryView.IsDirty = true;
			}
			PlayableUnit playableUnit = equipmentSlot.PlayableUnit;
			playableUnit.PlayableUnitController.RefreshStats();
			playableUnit.PlayableUnitView?.RefreshBodyParts();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshSkills(playableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar();
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
		}
	}

	public void OnInventorySlotDoubleClick(InventorySlot inventorySlot, EquipmentSlot targetEquipmentSlot = null)
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened && inventorySlot.Item != null)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.EquipItem(inventorySlot.Item, targetEquipmentSlot);
			PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
			TPSingleton<CharacterSheetPanel>.Instance.RefreshSkills(selectedPlayableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar(selectedPlayableUnit);
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
		}
	}

	public void OnEquipmentSlotSelected(EquipmentSlot equipmentSlot)
	{
		if (equipmentSlot != null)
		{
			OnInventorySlotDoubleClick(TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace, equipmentSlot);
		}
	}

	public void StartTurn()
	{
		for (int i = 0; i < Inventory.InventorySlots.Count; i++)
		{
			if (Inventory.InventorySlots[i].Item != null)
			{
				Inventory.InventorySlots[i].Item.ItemController.StartTurn();
			}
		}
	}

	private void GenerateInventorySlots()
	{
		for (int i = 0; i < Inventory.InventoryView.ItemsPanelTransform.childCount; i++)
		{
			InventorySlotView component = Inventory.InventoryView.ItemsPanelTransform.GetChild(i).GetComponent<InventorySlotView>();
			InventorySlot inventorySlot = new InventorySlotController(ItemDatabase.ItemSlotDefinitions[ItemSlotDefinition.E_ItemSlotId.Inventory], component, Inventory.InventoryView).InventorySlot;
			inventorySlot.InventorySlotView.InventorySlot = inventorySlot;
			Inventory.InventorySlots.Add(inventorySlot);
		}
	}
}
