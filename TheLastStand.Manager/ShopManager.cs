using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Item;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.View;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ToDoList;

namespace TheLastStand.Manager;

public class ShopManager : Manager<ShopManager>
{
	public Dictionary<ItemDefinition.E_Category, int> ItemsCountPerCategory { get; private set; }

	public static bool DebugShopForceAccess { get; private set; }

	public static void CloseShop()
	{
		TPSingleton<BuildingManager>.Instance.Shop.ShopController.CloseShopPanel();
	}

	public static void OpenShop()
	{
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel())
		{
			bool flag = TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet;
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.OpenShopPanel(flag, TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit));
			UIManager.HideInfoPanels();
			if (flag)
			{
				CharacterSheetManager.CloseCharacterSheetPanel(toAnotherPopup: true);
			}
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.MarkAllItemsAsSeen();
		}
	}

	public int GetAvailableItemsCount(ItemDefinition.E_Category category)
	{
		ItemsCountPerCategory.TryGetValue(category, out var value);
		return value;
	}

	public void Init()
	{
		InitItemsCountPerCategory();
	}

	public void RefreshItemsCountPerCategory()
	{
		HashSet<string> allLockedItemsIds = ItemManager.GetAllLockedItemsIds();
		HashSet<ItemDefinition.E_Category> hashSet = new HashSet<ItemDefinition.E_Category>();
		foreach (ItemDefinition.E_Category key in ItemsCountPerCategory.Keys)
		{
			hashSet.Add(key);
		}
		foreach (ItemDefinition.E_Category item in hashSet)
		{
			int num = 0;
			foreach (string item2 in BuildingDatabase.ShopItemsByCategory[item])
			{
				if (!allLockedItemsIds.Contains(item2))
				{
					num++;
				}
			}
			ItemsCountPerCategory[item] = num;
		}
	}

	private void InitItemsCountPerCategory()
	{
		ItemsCountPerCategory = new Dictionary<ItemDefinition.E_Category, int>();
		foreach (KeyValuePair<ItemDefinition.E_Category, HashSet<string>> item in BuildingDatabase.ShopItemsByCategory)
		{
			ItemsCountPerCategory.Add(item.Key, 0);
		}
		RefreshItemsCountPerCategory();
	}

	private void Update()
	{
		if (InputManager.GetButtonDown(30))
		{
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping)
			{
				CloseShop();
			}
			else
			{
				OpenShop();
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Shopping)
		{
			return;
		}
		if (InputManager.GetButtonDown(80))
		{
			CloseShop();
		}
		if (InputManager.GetButtonDown(0))
		{
			PlayableUnitManager.SelectNextUnit();
			int newUnitIndex = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.ChangeUnitToCompareAndResetDropdown(newUnitIndex);
			return;
		}
		if (InputManager.GetButtonDown(11))
		{
			PlayableUnitManager.SelectPreviousUnit();
			int newUnitIndex2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.ChangeUnitToCompareAndResetDropdown(newUnitIndex2);
			return;
		}
		if (InputManager.GetButtonDown(60))
		{
			TPSingleton<PlayableUnitManager>.Instance.ChangeEquipment();
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.CheckSkillButtonsFocus();
			int newUnitIndex3 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.ChangeUnitToCompareAndResetDropdown(newUnitIndex3);
			return;
		}
		int unitIndexHotkeyPressed = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
		if (unitIndexHotkeyPressed != -1)
		{
			TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed]);
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.ChangeUnitToCompareAndResetDropdown(unitIndexHotkeyPressed);
		}
	}

	[DevConsoleCommand("ShopForceAccess")]
	public static void DebugForceShopAccess(bool shopForceAccess = true)
	{
		DebugShopForceAccess = shopForceAccess;
		TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
		GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
	}
}
