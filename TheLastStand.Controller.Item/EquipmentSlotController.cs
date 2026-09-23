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

/// <summary>
/// Controller quản lý ô trang bị (Equipment Slot) trên tướng.
/// Kế thừa từ ItemSlotController, thêm logic đặc thù cho việc TRANG BỊ/GỠ TRANG BỊ vật phẩm:
/// - Cập nhật chỉ số tướng (stats) khi equip/unequip
/// - Kích hoạt/hủy Perks liên quan đến vật phẩm
/// - Thay đổi hình ảnh tướng (body parts) theo vật phẩm
/// - Xử lý vũ khí 2 tay (Two-Handed): block slot tay trái khi trang bị
/// 
/// Các loại EquipmentSlot: RightHand, LeftHand, Head, Body, Boots, Trinket...
/// </summary>
public class EquipmentSlotController : ItemSlotController
{
	#region Properties

	/// <summary>
	/// Cast ItemSlot thành EquipmentSlot để truy cập thuộc tính đặc thù:
	/// PlayableUnit (tướng sở hữu), BlockOtherSlot (slot bị khóa bởi vũ khí 2 tay),
	/// BlockedByOtherSlot, EquipmentSlotView...
	/// </summary>
	public EquipmentSlot EquipmentSlot => base.ItemSlot as EquipmentSlot;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo EquipmentSlot từ dữ liệu save (deserialization).
	/// Tìm ItemSlotDefinition từ database theo Id trong save data.
	/// </summary>
	/// <param name="itemEquipmentSlot">Dữ liệu serialized của ô trang bị.</param>
	/// <param name="equipmentSlotView">View component hiển thị ô trong UI.</param>
	/// <param name="unit">Tướng sở hữu ô trang bị này.</param>
	public EquipmentSlotController(SerializedItemSlot itemEquipmentSlot, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemEquipmentSlot)
	{
		ItemSlotDefinition itemSlotDefinition = ItemDatabase.ItemSlotDefinitions[itemEquipmentSlot.Id];
		base.ItemSlot = new EquipmentSlot(itemEquipmentSlot, itemSlotDefinition, this, equipmentSlotView, unit);
	}

	/// <summary>
	/// Constructor tạo EquipmentSlot mới (khi tạo tướng mới hoặc setup UI).
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot (RightHand, LeftHand, Head...).</param>
	/// <param name="equipmentSlotView">View component hiển thị ô trong UI.</param>
	/// <param name="unit">Tướng sở hữu ô trang bị này.</param>
	public EquipmentSlotController(ItemSlotDefinition itemSlotDefinition, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemSlotDefinition, equipmentSlotView)
	{
		base.ItemSlot = new EquipmentSlot(itemSlotDefinition, this, equipmentSlotView, unit);
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Kiểm tra xem có thể trang bị vũ khí 2 tay (Two-Handed) vào slot này không.
	/// Điều kiện:
	/// 1. Vật phẩm phải là vũ khí 2 tay (IsTwoHandedWeapon).
	/// 2. Slot này phải là tay phải (RightHand).
	/// 3. Tướng phải có slot tay trái (LeftHand) để bị block.
	/// </summary>
	/// <param name="twoHandedWeapon">Vũ khí 2 tay cần kiểm tra.</param>
	/// <returns>True nếu có thể trang bị vũ khí 2 tay vào slot này.</returns>
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

	/// <summary>
	/// Override SetItem - xử lý toàn bộ logic khi trang bị/gỡ trang bị vật phẩm trên tướng.
	/// 
	/// Luồng xử lý chi tiết:
	/// 1. GỠ ITEM CŨ (nếu có):
	///    a. Hủy block slot khác (nếu item cũ là vũ khí 2 tay)
	///    b. Xóa body parts override (nếu đang ở weapon set đang dùng)
	///    c. Gọi OnItemUnequiped: cập nhật stats tướng
	///    d. Gọi OnItemUnequipped: hủy Perks liên quan
	/// 2. GỌI BASE SetItem: cập nhật ItemSlot.Item
	/// 3. TRANG BỊ ITEM MỚI (nếu có):
	///    a. Gọi OnItemEquiped: cập nhật stats tướng
	///    b. Gọi OnItemEquipped: kích hoạt Perks
	///    c. Xử lý vũ khí 2 tay: tìm và block slot tay trái tương ứng
	/// 4. Cập nhật body parts theo item mới
	/// </summary>
	/// <param name="item">Vật phẩm cần trang bị (null = gỡ trang bị).</param>
	/// <param name="onLoad">True nếu đang load save game.</param>
	public override void SetItem(TheLastStand.Model.Item.Item item, bool onLoad = false)
	{
		// --- 1. Hủy block slot nếu item cũ là vũ khí 2 tay ---
		if (base.ItemSlot.Item != null && EquipmentSlot.BlockOtherSlot != null)
		{
			EquipmentSlot.BlockOtherSlot.BlockedByOtherSlot = null;
			EquipmentSlot.BlockOtherSlot.EquipmentSlotView.Refresh();
			EquipmentSlot.BlockOtherSlot = null;
		}

		// Kiểm tra xem slot này có thuộc weapon set đang hiển thị không
		bool flag = !ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(EquipmentSlot.ItemSlotDefinition.Id) || EquipmentSlot.PlayableUnit.EquipmentSlots[EquipmentSlot.ItemSlotDefinition.Id].IndexOf(EquipmentSlot) == EquipmentSlot.PlayableUnit.EquippedWeaponSetIndex;
		// Nếu slot Head và helmet đang bị ẩn → không cập nhật body parts
		if (EquipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.Head && !EquipmentSlot.PlayableUnit.HelmetDisplayed)
		{
			flag = false;
		}

		// --- 2. Gỡ body parts của item cũ (nếu có và đang active) ---
		if (base.ItemSlot.Item != null && flag)
		{
			EquipmentSlot.PlayableUnit.PlayableUnitController.OverrideBodyParts(base.ItemSlot.Item.ItemDefinition.BodyPartsDefinitions, clear: true);
		}

		// --- 3. Gọi OnItemUnequiped/OnItemUnequipped cho item cũ ---
		if (base.ItemSlot.Item != null)
		{
			EquipmentSlot.PlayableUnit.PlayableUnitStatsController.OnItemUnequiped(base.ItemSlot.Item, onLoad);
			// Chỉ trigger OnItemUnequipped nếu thực sự thay đổi item (không phải swap cùng slot)
			if (item == null || base.ItemSlot.Item != item || item.ItemSlot == null || base.ItemSlot.ItemSlotDefinition.Id != item.ItemSlot.ItemSlotDefinition.Id)
			{
				EquipmentSlot.PlayableUnit.PlayableUnitPerksController.OnItemUnequipped(EquipmentSlot);
			}
		}

		// --- 4. Gọi base SetItem (cập nhật ItemSlot.Item, hủy liên kết cũ, refresh view) ---
		base.SetItem(item);

		// --- 5. Xử lý item mới ---
		if (item != null)
		{
			if (base.ItemSlot.Item == item)
			{
				// Cập nhật stats và perks cho item mới
				EquipmentSlot.PlayableUnit.PlayableUnitStatsController.OnItemEquiped(item, onLoad);
				EquipmentSlot.PlayableUnit.PlayableUnitPerksController.OnItemEquipped(EquipmentSlot, item);
			}

			// --- 6. Xử lý vũ khí 2 tay: block slot tay trái ---
			if (item.IsTwoHandedWeapon && TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				int num = TileObjectSelectionManager.SelectedPlayableUnit.EquipmentSlots[EquipmentSlot.ItemSlotDefinition.Id].IndexOf(EquipmentSlot);
				bool flag2 = false;
				// Tìm slot LeftHand cùng index (cùng weapon set) để block
				foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot2 in TileObjectSelectionManager.SelectedPlayableUnit.EquipmentSlots)
				{
					for (int i = 0; i < equipmentSlot2.Value.Count; i++)
					{
						EquipmentSlot equipmentSlot = equipmentSlot2.Value[i];
						if (equipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.LeftHand && i == num)
						{
							// Gỡ item ở tay trái và khóa slot
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

		// --- 7. Cập nhật body parts theo item mới (nếu có và đang active) ---
		if (base.ItemSlot.Item != null && flag)
		{
			EquipmentSlot.PlayableUnit.PlayableUnitController.OverrideBodyParts(base.ItemSlot.Item.ItemDefinition.BodyPartsDefinitions);
		}
	}

	#endregion Public Methods
}
