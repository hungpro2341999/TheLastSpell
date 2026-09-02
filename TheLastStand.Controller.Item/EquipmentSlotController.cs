using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Item;
using TheLastStand.View.CharacterSheet;

namespace TheLastStand.Controller.Item;

public class EquipmentSlotController : ItemSlotController
{
	public EquipmentSlot EquipmentSlot => base.ItemSlot as EquipmentSlot;

	public EquipmentSlotController(SerializedItemSlot itemEquipmentSlot, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemEquipmentSlot)
	{
		ItemSlotDefinition itemSlotDefinition = ItemDatabase.ItemSlotDefinitions[itemEquipmentSlot.Id];
		base.ItemSlot = new EquipmentSlot(itemEquipmentSlot, itemSlotDefinition, this, equipmentSlotView, unit);
	}

	public EquipmentSlotController(ItemSlotDefinition itemSlotDefinition, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemSlotDefinition, equipmentSlotView)
	{
		base.ItemSlot = new EquipmentSlot(itemSlotDefinition, this, equipmentSlotView, unit);
	}

	public bool CanEquipTwoHandedWeapon(TheLastStand.Model.Item.Item twoHandedWeapon)
	{
		if (!twoHandedWeapon.IsTwoHandedWeapon)
		{
			TPSingleton<InventoryManager>.Instance.LogError("The item is not a two handed weapon!", CLogLevel.MAJOR);
			return false;
		}
		if (EquipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.RightHand)
		{
			return EquipmentSlot.PlayableUnit.EquipmentSlots.ContainsKey(ItemSlotDefinition.E_ItemSlotId.LeftHand);
		}
		return false;
	}

	public override void SetItem(TheLastStand.Model.Item.Item item, bool onLoad = false)
	{
		if (base.ItemSlot.Item != null && EquipmentSlot.BlockOtherSlot != null)
		{
			EquipmentSlot.BlockOtherSlot.BlockedByOtherSlot = null;
			EquipmentSlot.BlockOtherSlot.EquipmentSlotView.Refresh();
			EquipmentSlot.BlockOtherSlot = null;
		}
		bool flag = !ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(EquipmentSlot.ItemSlotDefinition.Id) || EquipmentSlot.PlayableUnit.EquipmentSlots[EquipmentSlot.ItemSlotDefinition.Id].IndexOf(EquipmentSlot) == EquipmentSlot.PlayableUnit.EquippedWeaponSetIndex;
		if (EquipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.Head && !EquipmentSlot.PlayableUnit.HelmetDisplayed)
		{
			flag = false;
		}
		if (base.ItemSlot.Item != null && flag)
		{
			EquipmentSlot.PlayableUnit.PlayableUnitController.OverrideBodyParts(base.ItemSlot.Item.ItemDefinition.BodyPartsDefinitions, clear: true);
		}
		if (base.ItemSlot.Item != null)
		{
			EquipmentSlot.PlayableUnit.PlayableUnitStatsController.OnItemUnequiped(base.ItemSlot.Item, onLoad);
			if (item == null || base.ItemSlot.Item != item || item.ItemSlot == null || base.ItemSlot.ItemSlotDefinition.Id != item.ItemSlot.ItemSlotDefinition.Id)
			{
				EquipmentSlot.PlayableUnit.PlayableUnitPerksController.OnItemUnequipped(EquipmentSlot);
			}
		}
		base.SetItem(item);
		if (item != null)
		{
			if (base.ItemSlot.Item == item)
			{
				EquipmentSlot.PlayableUnit.PlayableUnitStatsController.OnItemEquiped(item, onLoad);
				EquipmentSlot.PlayableUnit.PlayableUnitPerksController.OnItemEquipped(EquipmentSlot, item);
			}
			if (item.IsTwoHandedWeapon && TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				int num = TileObjectSelectionManager.SelectedPlayableUnit.EquipmentSlots[EquipmentSlot.ItemSlotDefinition.Id].IndexOf(EquipmentSlot);
				bool flag2 = false;
				foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot2 in TileObjectSelectionManager.SelectedPlayableUnit.EquipmentSlots)
				{
					for (int i = 0; i < equipmentSlot2.Value.Count; i++)
					{
						EquipmentSlot equipmentSlot = equipmentSlot2.Value[i];
						if (equipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.LeftHand && i == num)
						{
							equipmentSlot.ItemSlotController.SwapItems();
							EquipmentSlot.BlockOtherSlot = equipmentSlot;
							equipmentSlot.BlockedByOtherSlot = EquipmentSlot;
							equipmentSlot.EquipmentSlotView.Refresh();
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
			}
		}
		if (base.ItemSlot.Item != null && flag)
		{
			EquipmentSlot.PlayableUnit.PlayableUnitController.OverrideBodyParts(base.ItemSlot.Item.ItemDefinition.BodyPartsDefinitions);
		}
	}
}
