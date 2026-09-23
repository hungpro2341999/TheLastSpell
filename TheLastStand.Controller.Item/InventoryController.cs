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

/// <summary>
/// Controller quản lý toàn bộ Inventory (túi đồ/kho đồ) của người chơi.
/// Inventory là nơi lưu trữ tạm thời vật phẩm chưa trang bị cho tướng.
/// 
/// Chức năng chính:
/// - Thêm/gỡ vật phẩm vào Inventory
/// - Kiểm tra xem Inventory có thể mở không (tùy thuộc trạng thái game)
/// - Xử lý double-click trang bị/gỡ trang bị nhanh (EquipmentSlot ↔ Inventory)
/// - Đánh dấu vật phẩm mới (IsNewItem) để hiển thị indicator
/// - Gọi StartTurn cho tất cả item trong Inventory
/// - Tự động tạo các InventorySlot từ UI prefabs
/// </summary>
public class InventoryController
{
	#region Properties

	/// <summary>
	/// Model Inventory mà controller này quản lý.
	/// Chứa: danh sách InventorySlots, ItemCount, InventoryView reference.
	/// </summary>
	public Inventory Inventory { get; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo Inventory từ dữ liệu save (deserialization).
	/// 1. Tạo Model Inventory và liên kết với View.
	/// 2. Tạo các InventorySlot từ UI prefabs.
	/// 3. Deserialize dữ liệu item vào các slot tương ứng.
	/// </summary>
	/// <param name="container">Dữ liệu serialized của Inventory.</param>
	/// <param name="inventoryView">View component hiển thị Inventory trong UI.</param>
	public InventoryController(ISerializedData container, InventoryView inventoryView)
	{
		Inventory = new Inventory(container, this, inventoryView);
		Inventory.InventoryView.Inventory = Inventory;
		GenerateInventorySlots();
		Inventory.Deserialize(container);
	}

	/// <summary>
	/// Constructor tạo Inventory mới (trống, chưa có item).
	/// 1. Tạo Model Inventory và liên kết với View.
	/// 2. Tạo các InventorySlot từ UI prefabs.
	/// </summary>
	/// <param name="inventoryView">View component hiển thị Inventory trong UI.</param>
	public InventoryController(InventoryView inventoryView)
	{
		Inventory = new Inventory(this, inventoryView);
		Inventory.InventoryView.Inventory = Inventory;
		GenerateInventorySlots();
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Thêm vật phẩm vào Inventory.
	/// Nếu không chỉ định slot cụ thể, tự động tìm ô trống đầu tiên.
	/// Nếu Inventory đầy (không tìm được ô trống) → log warning và bỏ qua.
	/// </summary>
	/// <param name="item">Vật phẩm cần thêm.</param>
	/// <param name="inventorySlot">Ô slot chỉ định (null = tự tìm ô trống).</param>
	/// <param name="isNewItem">True nếu đây là vật phẩm mới (hiển thị indicator "New!").</param>
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

	/// <summary>
	/// Kiểm tra xem Inventory có thể mở được không.
	/// Phụ thuộc vào trạng thái hiện tại của game:
	/// - Phải đang trong Cycle Day (ban ngày).
	/// - Phải đang ở một trong các trạng thái cho phép: Management, CharacterSheet,
	///   UnitPreparingSkill, BuildingPreparingAction, Construction, Shopping.
	/// - Hoặc DebugForceInventoryAccess = true (chế độ debug).
	/// </summary>
	/// <returns>True nếu có thể mở Inventory.</returns>
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

	/// <summary>
	/// Tìm ô Inventory trống đầu tiên (Item == null).
	/// </summary>
	/// <returns>InventorySlot trống đầu tiên, hoặc null nếu Inventory đầy.</returns>
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

	/// <summary>
	/// Đánh dấu tất cả vật phẩm trong Inventory là "đã xem" (IsNewItem = false).
	/// Gọi khi người chơi mở Inventory để tắt indicator "New!" trên các item.
	/// </summary>
	public void MarkAllItemsAsSeen()
	{
		for (int num = Inventory.InventorySlots.Count - 1; num >= 0; num--)
		{
			Inventory.InventorySlots[num].IsNewItem = false;
		}
	}

	/// <summary>
	/// Xử lý double-click trên ô trang bị (Equipment Slot) → GỠ trang bị nhanh.
	/// Chuyển vật phẩm từ EquipmentSlot → Inventory (nếu còn chỗ trống).
	/// Điều kiện: đang ở CharacterSheet + Inventory đang mở + slot có item.
	/// 
	/// Sau khi gỡ:
	/// - Refresh stats, body parts, skills, avatar của tướng.
	/// - Phát âm thanh thành công.
	/// </summary>
	/// <param name="equipmentSlot">Ô trang bị được double-click.</param>
	public void OnEquipmentSlotDoubleClick(EquipmentSlot equipmentSlot)
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened && equipmentSlot.Item != null)
		{
			// Chuyển item vào Inventory nếu còn chỗ
			if (TPSingleton<InventoryManager>.Instance.Inventory.ItemCount < TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots.Count)
			{
				Inventory.InventoryController.AddItem(equipmentSlot.Item);
			}
			// Hủy block slot (nếu item là vũ khí 2 tay)
			if (equipmentSlot.BlockOtherSlot != null)
			{
				equipmentSlot.BlockOtherSlot = null;
			}
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened)
			{
				Inventory.InventoryView.IsDirty = true;
			}
			// Refresh UI của tướng
			PlayableUnit playableUnit = equipmentSlot.PlayableUnit;
			playableUnit.PlayableUnitController.RefreshStats();
			playableUnit.PlayableUnitView?.RefreshBodyParts();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshSkills(playableUnit);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar();
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
		}
	}

	/// <summary>
	/// Xử lý double-click trên ô Inventory → TRANG BỊ nhanh.
	/// Trang bị vật phẩm từ Inventory cho tướng đang được chọn.
	/// Nếu có targetEquipmentSlot → trang bị vào slot đó; nếu không → auto-equip.
	/// Điều kiện: đang ở CharacterSheet + Inventory đang mở + slot có item.
	/// </summary>
	/// <param name="inventorySlot">Ô Inventory được double-click.</param>
	/// <param name="targetEquipmentSlot">Ô trang bị đích (null = tự tìm slot phù hợp).</param>
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

	/// <summary>
	/// Xử lý khi người chơi chọn ô trang bị bằng joystick (gamepad navigation).
	/// Trang bị vật phẩm đang chờ đặt (InventorySlotToPlace) vào EquipmentSlot được chọn.
	/// </summary>
	/// <param name="equipmentSlot">Ô trang bị được chọn bằng joystick.</param>
	public void OnEquipmentSlotSelected(EquipmentSlot equipmentSlot)
	{
		if (equipmentSlot != null)
		{
			OnInventorySlotDoubleClick(TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace, equipmentSlot);
		}
	}

	/// <summary>
	/// Xử lý logic đầu lượt cho tất cả vật phẩm trong Inventory.
	/// Gọi ItemController.StartTurn() cho mỗi item (nạp lại lượt sử dụng kỹ năng).
	/// </summary>
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

	#endregion Public Methods

	#region Private Methods

	/// <summary>
	/// Tự động tạo các InventorySlot từ UI prefabs.
	/// Duyệt tất cả child objects trong ItemsPanelTransform của InventoryView,
	/// lấy component InventorySlotView, tạo InventorySlotController + InventorySlot tương ứng,
	/// và thêm vào danh sách Inventory.InventorySlots.
	/// Số lượng ô Inventory = số child trong ItemsPanelTransform.
	/// </summary>
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

	#endregion Private Methods
}
