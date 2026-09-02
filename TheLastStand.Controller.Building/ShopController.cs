using System;
using TPLib;
using TheLastStand.Controller.Item;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization.Building;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Shop;

namespace TheLastStand.Controller.Building;

public class ShopController
{
	public ShopInventorySlot CurrentlyFocusedInventorySlot { get; set; }

	public ShopSlot CurrentlyFocusedSlot { get; set; }

	public Shop Shop { get; }

	public event Action<bool> OnShopToggle;

	public ShopController(SerializedShop container, ShopView shopView)
	{
		Shop = new Shop(this, shopView);
		shopView.Shop = Shop;
		shopView.UnitDropdown.OnUnitToCompareChanged += OnUnitToCompareChanged;
		GenerateShopInventorySlots();
	}

	public ShopController(ShopView shopView)
	{
		Shop = new Shop(this, shopView);
		shopView.Shop = Shop;
		shopView.UnitDropdown.OnUnitToCompareChanged += OnUnitToCompareChanged;
		GenerateShopInventorySlots();
	}

	public void AddItem(TheLastStand.Model.Item.Item newItem, bool isRebought = false, bool isSoldOut = false)
	{
		ShopSlot shopSlot = FindFreeShopSlot(isRebought);
		if (shopSlot == null)
		{
			shopSlot = new ShopSlotController(ItemDatabase.ItemSlotDefinitions[ItemSlotDefinition.E_ItemSlotId.Shop], Shop.ShopView.AddNewSlotView()).ShopSlot;
			Shop.ShopSlots.Add(shopSlot);
		}
		shopSlot.Item = newItem;
		shopSlot.ShopSlotView.ShopSlot = shopSlot;
		shopSlot.IsSoldOut = isSoldOut;
		shopSlot.ShopSlotView.transform.SetAsLastSibling();
		shopSlot.ShopSlotView.Show();
		shopSlot.ShopSlotView.Refresh();
		if (isRebought)
		{
			Shop.ShopView.OnItemSold(shopSlot);
		}
		Shop.ShopView.CheckShelves();
	}

	public bool CanOpenShopPanel()
	{
		if (ShopManager.DebugShopForceAccess)
		{
			return true;
		}
		if (TPSingleton<BuildingManager>.Instance.AccessShopBuildingCount > 0 && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingAction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill)
			{
				if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet)
				{
					return TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void ChangeUnitToCompareAndResetDropdown(int newUnitIndex)
	{
		Shop.UnitToCompareIndex = newUnitIndex;
		CurrentlyFocusedSlot?.ItemSlotView.Refresh();
		CurrentlyFocusedInventorySlot?.ItemSlotView.Refresh();
		Shop.ShopView.UnitDropdown.ResetDropdown(newUnitIndex + 1);
	}

	public void ClearItems()
	{
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			ShopSlot shopSlot = Shop.ShopSlots[i];
			shopSlot.Item = null;
			shopSlot.ShopSlotView.ShopSlot = null;
			shopSlot.ShopSlotView.Refresh();
		}
		Shop.ShopView.ClearShelves();
	}

	public void CloseShopPanel(bool toAnotherPopup = false)
	{
		if (Shop.IsOpened)
		{
			Shop.IsOpened = false;
			this.OnShopToggle?.Invoke(obj: false);
			TPSingleton<ResourceManager>.Instance.OnGoldChange -= Shop.ShopView.OnGoldChanged;
			TPSingleton<BuildingManager>.Instance.Shop.ShopView.Close(toAnotherPopup);
			if (!toAnotherPopup)
			{
				GameController.SetState(Game.E_State.Management);
			}
		}
	}

	public void OpenShopPanel(bool fromAnotherPopup = false, int selectedUnit = -1)
	{
		if (!Shop.IsOpened)
		{
			Shop.IsOpened = true;
			this.OnShopToggle?.Invoke(obj: true);
			TPSingleton<ResourceManager>.Instance.OnGoldChange += Shop.ShopView.OnGoldChanged;
			if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
			{
				ConstructionManager.ExitConstructionMode();
			}
			TPSingleton<BuildingManager>.Instance.Shop.UnitToCompareIndex = selectedUnit + 1;
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.IsDirty = true;
			TPSingleton<BuildingManager>.Instance.Shop.ShopView.Open(fromAnotherPopup);
			GameController.SetState(Game.E_State.Shopping);
		}
	}

	public void SortItems()
	{
		Shop.ShopSlots.Sort(delegate(ShopSlot a, ShopSlot b)
		{
			if (a.Item == null || b.Item == null)
			{
				return 0;
			}
			int num = Shop.Constants.ShopSortOrder.IndexOf(a.Item.ItemDefinition.Category);
			int value = Shop.Constants.ShopSortOrder.IndexOf(b.Item.ItemDefinition.Category);
			return num.CompareTo(value);
		});
		if (!Shop.ShopView.HasActiveSort)
		{
			Shop.ShopView.ResetSort();
		}
	}

	public bool TryToPayReroll()
	{
		int shopRerollPrice = Shop.ShopRerollPrice;
		if (TPSingleton<ResourceManager>.Instance.Gold >= shopRerollPrice)
		{
			Shop.ShopRerollIndex++;
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - shopRerollPrice);
			BuildingManager.RefreshShop();
			return true;
		}
		return false;
	}

	public bool TryBuyItem(ShopSlot shopSlot, InventorySlot destination = null)
	{
		if (shopSlot.IsSoldOut)
		{
			return false;
		}
		int num = (shopSlot.Item.HasBeenSoldBefore ? shopSlot.Item.SellingPrice : shopSlot.Item.FinalPrice);
		if (TPSingleton<ResourceManager>.Instance.Gold >= num && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping && TPSingleton<InventoryManager>.Instance.Inventory.ItemCount < TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots.Count)
		{
			bool hasBeenSoldBefore = shopSlot.Item.HasBeenSoldBefore;
			if (destination == null)
			{
				TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.AddItem(shopSlot.Item, null, isNewItem: true);
			}
			else
			{
				if (destination.Item != null)
				{
					return false;
				}
				destination.ItemSlotController.SetItem(shopSlot.Item);
				destination.IsNewItem = true;
			}
			if (hasBeenSoldBefore)
			{
				TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - num, updateGoldMetaConditions: false);
			}
			else
			{
				TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - num);
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.GoldSpentInShop, num);
				TPSingleton<MetaConditionManager>.Instance.IncreaseBoughtItems(shopSlot.Item.ItemDefinition, num);
			}
			shopSlot.IsSoldOut = true;
			shopSlot.ShopSlotView.Refresh();
			shopSlot.ShopSlotView.DisplayTooltip(display: false);
			Shop.ShopView.OnItemBought(shopSlot);
			return true;
		}
		return false;
	}

	public void TrySellItem(ShopInventorySlot shopInventorySlot)
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping)
		{
			if (shopInventorySlot.Item.HasBeenSoldBefore)
			{
				TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + shopInventorySlot.Item.SellingPrice, updateGoldMetaConditions: false);
			}
			else
			{
				TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + shopInventorySlot.Item.SellingPrice);
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.SoldItems, 1.0);
				shopInventorySlot.Item.HasBeenSoldBefore = true;
			}
			if (Analytics.AllowedToSendData)
			{
				Analytics.SendSaleItemEvent(shopInventorySlot.Item.ItemDefinition.Id, shopInventorySlot.Item.SellingPrice);
			}
			Shop.ShopController.AddItem(shopInventorySlot.Item, isRebought: true);
			if (InputManager.IsLastControllerJoystick)
			{
				Shop.ShopView.RefreshJoystickNavigation();
			}
			TheLastStand.Model.Item.Item item = shopInventorySlot.Item;
			if (item != null && item.Rarity == ItemDefinition.E_Rarity.Epic)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_SELL_EPIC_ITEM);
			}
			TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot.ShopInventorySlotView.ItemIndex].ItemSlotController.RemoveItem();
		}
	}

	private ShopSlot FindFreeShopSlot(bool canTakeSoldOutSlot = true)
	{
		ShopSlot result = null;
		int i = 0;
		for (int count = Shop.ShopSlots.Count; i < count; i++)
		{
			if (canTakeSoldOutSlot && Shop.ShopSlots[i].IsSoldOut)
			{
				return Shop.ShopSlots[i];
			}
			if (Shop.ShopSlots[i].ShopSlotView.ShopSlot == null)
			{
				result = Shop.ShopSlots[i];
				if (!canTakeSoldOutSlot)
				{
					return result;
				}
			}
		}
		return result;
	}

	private void GenerateShopInventorySlots()
	{
		for (int i = 0; i < Shop.ShopView.InventoryItemsPanelTransform.childCount; i++)
		{
			ShopInventorySlotView component = Shop.ShopView.InventoryItemsPanelTransform.GetChild(i).GetComponent<ShopInventorySlotView>();
			ShopInventorySlot shopInventorySlot = new ShopInventorySlotController(ItemDatabase.ItemSlotDefinitions[ItemSlotDefinition.E_ItemSlotId.Inventory], component).ShopInventorySlot;
			shopInventorySlot.ShopInventorySlotView.ShopInventorySlot = shopInventorySlot;
			Shop.ShopInventorySlots.Add(shopInventorySlot);
		}
	}

	private void OnUnitToCompareChanged(int newUnitIndex)
	{
		Shop.UnitToCompareIndex = newUnitIndex;
		if (newUnitIndex == -1)
		{
			TileObjectSelectionManager.DeselectUnit();
		}
		else
		{
			PlayableUnitManager.SelectUnitAtIndex(newUnitIndex);
		}
	}
}
