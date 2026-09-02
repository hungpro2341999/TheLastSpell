using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Controller.Item;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Item;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet.Inventory;
using UnityEngine;

namespace TheLastStand.Manager.Item;

public class InventoryManager : Manager<InventoryManager>, ISerializable, IDeserializable
{
	[SerializeField]
	private InventoryView inventoryView;

	[SerializeField]
	private bool debugForceInventoryAccess;

	public static InventoryView InventoryView => TPSingleton<InventoryManager>.Instance.inventoryView;

	public Inventory Inventory { get; private set; }

	public static bool DebugForceInventoryAccess => TPSingleton<InventoryManager>.Instance.debugForceInventoryAccess;

	public static void StartTurn()
	{
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.StartTurn();
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container != null)
		{
			Inventory = new InventoryController(container, inventoryView).Inventory;
		}
		else
		{
			Inventory = new InventoryController(inventoryView).Inventory;
		}
	}

	public ISerializedData Serialize()
	{
		return Inventory.Serialize();
	}

	[DevConsoleCommand(Name = "InventoryForceAccess")]
	public static void Debug_ForceInventoryAccess(bool forceInventoryAccess = true)
	{
		TPSingleton<InventoryManager>.Instance.debugForceInventoryAccess = forceInventoryAccess;
		GameView.BottomScreenPanel.Refresh();
	}

	[DevConsoleCommand(Name = "InventoryClear")]
	public static void Debug_ClearInventory()
	{
		foreach (InventorySlot inventorySlot in TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots)
		{
			inventorySlot.ItemSlotController.RemoveItem();
		}
	}
}
