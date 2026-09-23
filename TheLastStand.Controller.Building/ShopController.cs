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

/// <summary>
/// Bộ điều khiển hệ thống Cửa hàng (Shop):
/// Quản lý giao dịch mua/bán trang bị, làm mới hàng hóa (reroll), so sánh chỉ số trang bị với tướng,
/// tương tác với túi đồ (Inventory), cập nhật tài nguyên (Vàng), hệ thống Thành tựu và Thống kê (Analytics).
/// </summary>
public class ShopController
{
	#region Properties & Events

	/// <summary>
	/// Ô đồ trong hành trang cá nhân đang được trỏ chuột / tiêu điểm (hover/focus).
	/// </summary>
	public ShopInventorySlot CurrentlyFocusedInventorySlot { get; set; }

	/// <summary>
	/// Ô hàng hóa trong quầy Shop đang được trỏ chuột / tiêu điểm (hover/focus).
	/// </summary>
	public ShopSlot CurrentlyFocusedSlot { get; set; }

	/// <summary>
	/// Model dữ liệu trung tâm của Cửa hàng.
	/// </summary>
	public Shop Shop { get; }

	/// <summary>
	/// Sự kiện kích hoạt khi trạng thái mở/đóng của Shop thay đổi (true: Mở, false: Đóng).
	/// </summary>
	public event Action<bool> OnShopToggle;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo ShopController từ dữ liệu lưu trữ (Save Game Deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của Shop.</param>
	/// <param name="shopView">View hiển thị tương ứng của Shop trên giao diện Unity.</param>
	public ShopController(SerializedShop container, ShopView shopView)
	{
		Shop = new Shop(this, shopView);
		shopView.Shop = Shop;
		shopView.UnitDropdown.OnUnitToCompareChanged += OnUnitToCompareChanged;
		GenerateShopInventorySlots();
	}

	/// <summary>
	/// Khởi tạo ShopController mới khi bắt đầu một phiên chơi (New Game Session).
	/// </summary>
	/// <param name="shopView">View hiển thị tương ứng của Shop trên giao diện Unity.</param>
	public ShopController(ShopView shopView)
	{
		Shop = new Shop(this, shopView);
		shopView.Shop = Shop;
		shopView.UnitDropdown.OnUnitToCompareChanged += OnUnitToCompareChanged;
		GenerateShopInventorySlots();
	}

	#endregion

	#region Shop Panel Lifecycle & Validation

	/// <summary>
	/// Kiểm tra xem người chơi hiện tại có đủ điều kiện để mở giao diện Cửa hàng hay không.
	/// </summary>
	/// <returns>True nếu có thể mở Shop; ngược lại trả về False.</returns>
	public bool CanOpenShopPanel()
	{
		// Cho phép mở ngay lập tức nếu đang bật cờ gỡ lỗi (Debug Cheat)
		if (ShopManager.DebugShopForceAccess)
		{
			return true;
		}

		// Điều kiện cơ bản: Có ít nhất 1 công trình Shop và đang ở pha ban ngày (Production Turn)
		if (TPSingleton<BuildingManager>.Instance.AccessShopBuildingCount > 0 && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			// Kiểm tra các trạng thái game không bị xung đột (không trong lúc chuẩn bị dùng kỹ năng, xây dựng, hành động...)
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingAction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill)
			{
				// Nếu đang ở màn hình thông tin tướng (CharacterSheet), chỉ cho phép mở khi túi đồ đang hiển thị
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

	/// <summary>
	/// Mở bảng giao diện Cửa hàng và chuyển trạng thái game sang Shopping.
	/// </summary>
	/// <param name="fromAnotherPopup">Có phải được mở chuyển tiếp từ một cửa sổ popup khác hay không.</param>
	/// <param name="selectedUnit">Chỉ số của tướng được chọn sẵn để so sánh trang bị (-1 nếu không có).</param>
	public void OpenShopPanel(bool fromAnotherPopup = false, int selectedUnit = -1)
	{
		if (!Shop.IsOpened)
		{
			Shop.IsOpened = true;
			this.OnShopToggle?.Invoke(obj: true);

			// Lắng nghe biến động số vàng để cập nhật giao diện tiền tệ của Shop
			TPSingleton<ResourceManager>.Instance.OnGoldChange += Shop.ShopView.OnGoldChanged;

			// Thoát chế độ xây dựng nếu đang mở
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

	/// <summary>
	/// Đóng bảng giao diện Cửa hàng và đưa trạng thái game trở về Management (Quản lý).
	/// </summary>
	/// <param name="toAnotherPopup">Có phải đóng để chuyển tiếp sang một popup khác hay không.</param>
	public void CloseShopPanel(bool toAnotherPopup = false)
	{
		if (Shop.IsOpened)
		{
			Shop.IsOpened = false;
			this.OnShopToggle?.Invoke(obj: false);

			// Hủy lắng nghe biến động vàng
			TPSingleton<ResourceManager>.Instance.OnGoldChange -= Shop.ShopView.OnGoldChanged;
			TPSingleton<BuildingManager>.Instance.Shop.ShopView.Close(toAnotherPopup);

			if (!toAnotherPopup)
			{
				GameController.SetState(Game.E_State.Management);
			}
		}
	}

	#endregion

	#region Item Management & Transactions

	/// <summary>
	/// Thêm một vật phẩm vào quầy hàng của Shop (bày bán mới hoặc món đồ người chơi vừa bán có thể mua lại).
	/// </summary>
	/// <param name="newItem">Vật phẩm cần bày lên kệ.</param>
	/// <param name="isRebought">Đánh dấu nếu đây là đồ người chơi vừa bán vào quầy để có thể mua lại.</param>
	/// <param name="isSoldOut">Trạng thái ô hàng đã hết hàng hay chưa.</param>
	public void AddItem(TheLastStand.Model.Item.Item newItem, bool isRebought = false, bool isSoldOut = false)
	{
		ShopSlot shopSlot = FindFreeShopSlot(isRebought);
		if (shopSlot == null)
		{
			// Nếu không còn ô trống nào trên kệ, khởi tạo thêm ô mới
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

	/// <summary>
	/// Xóa toàn bộ vật phẩm đang bày bán trên các kệ hàng của Shop (dùng khi reroll hoặc sang ngày mới).
	/// </summary>
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

	/// <summary>
	/// Sắp xếp các vật phẩm trong Shop theo thứ tự danh mục (Vũ khí, Giáp, Phụ kiện, Thuốc tiêu hao...).
	/// </summary>
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

	/// <summary>
	/// Thực hiện thanh toán vàng để làm mới (Reroll) danh sách hàng hóa trong Shop.
	/// </summary>
	/// <returns>True nếu có đủ vàng và làm mới thành công; ngược lại trả về False.</returns>
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

	/// <summary>
	/// Mua một vật phẩm từ ô hàng trong Shop và chuyển vào túi đồ (Inventory).
	/// </summary>
	/// <param name="shopSlot">Ô hàng cần mua trong Shop.</param>
	/// <param name="destination">Ô chỉ định đích đến trong túi đồ (nếu null sẽ tự động tìm ô trống đầu tiên).</param>
	/// <returns>True nếu giao dịch mua thành công; ngược lại trả về False.</returns>
	public bool TryBuyItem(ShopSlot shopSlot, InventorySlot destination = null)
	{
		if (shopSlot.IsSoldOut)
		{
			return false;
		}

		// Xác định giá mua: Giá gốc hoặc giá bán lại nếu đã từng bán trước đó
		int num = (shopSlot.Item.HasBeenSoldBefore ? shopSlot.Item.SellingPrice : shopSlot.Item.FinalPrice);

		// Kiểm tra điều kiện: Đủ vàng, đúng trạng thái Shopping, và túi đồ còn chỗ chứa
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

			// Trừ vàng và ghi nhận tiến độ Meta condition
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

			// Cập nhật trạng thái ô hàng đã bán hết (Sold Out)
			shopSlot.IsSoldOut = true;
			shopSlot.ShopSlotView.Refresh();
			shopSlot.ShopSlotView.DisplayTooltip(display: false);
			Shop.ShopView.OnItemBought(shopSlot);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Bán một vật phẩm từ túi đồ người chơi vào Shop để lấy vàng.
	/// </summary>
	/// <param name="shopInventorySlot">Ô đồ trong túi đồ của người chơi cần bán.</param>
	public void TrySellItem(ShopInventorySlot shopInventorySlot)
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping)
		{
			// Cộng vàng bán đồ và cập nhật tiến độ Meta condition nếu bán lần đầu
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

			// Gửi sự kiện Analytics phân tích người dùng
			if (Analytics.AllowedToSendData)
			{
				Analytics.SendSaleItemEvent(shopInventorySlot.Item.ItemDefinition.Id, shopInventorySlot.Item.SellingPrice);
			}

			// Đưa món đồ vừa bán vào quầy hàng của Shop để người chơi có thể chuộc lại nếu muốn
			Shop.ShopController.AddItem(shopInventorySlot.Item, isRebought: true);

			if (InputManager.IsLastControllerJoystick)
			{
				Shop.ShopView.RefreshJoystickNavigation();
			}

			// Mở khóa thành tựu nếu người chơi bán một vật phẩm phẩm chất Epic
			TheLastStand.Model.Item.Item item = shopInventorySlot.Item;
			if (item != null && item.Rarity == ItemDefinition.E_Rarity.Epic)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_SELL_EPIC_ITEM);
			}

			// Xóa vật phẩm khỏi ô hành trang
			TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot.ShopInventorySlotView.ItemIndex].ItemSlotController.RemoveItem();
		}
	}

	#endregion

	#region Unit Comparison

	/// <summary>
	/// Đổi tướng được lựa chọn để so sánh trang bị trong Shop và làm mới danh sách thả xuống (Dropdown).
	/// </summary>
	/// <param name="newUnitIndex">Chỉ số của tướng mới cần so sánh.</param>
	public void ChangeUnitToCompareAndResetDropdown(int newUnitIndex)
	{
		Shop.UnitToCompareIndex = newUnitIndex;
		CurrentlyFocusedSlot?.ItemSlotView.Refresh();
		CurrentlyFocusedInventorySlot?.ItemSlotView.Refresh();
		Shop.ShopView.UnitDropdown.ResetDropdown(newUnitIndex + 1);
	}

	#endregion

	#region Private Helpers & Event Handlers

	/// <summary>
	/// Tìm kiếm một ô hàng trống hoặc ô đã bán hết (SoldOut) có thể tái sử dụng để bày món đồ mới.
	/// </summary>
	/// <param name="canTakeSoldOutSlot">Có cho phép dùng lại ô đã bán hết hay không.</param>
	/// <returns>ShopSlot còn trống hoặc đã bán hết nếu tìm thấy; ngược lại trả về null.</returns>
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

	/// <summary>
	/// Khởi tạo danh sách các ô đồ trong túi hàng hiển thị trên giao diện Shop (ShopInventorySlots).
	/// </summary>
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

	/// <summary>
	/// Xử lý sự kiện khi người chơi chọn đổi tướng trong Dropdown để so sánh chỉ số trang bị.
	/// </summary>
	/// <param name="newUnitIndex">Chỉ số của tướng được chọn (-1 nếu bỏ chọn).</param>
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

	#endregion
}
