using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Item;
using TheLastStand.View.CharacterSheet;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model đại diện cho một ô trang bị (Equipment Slot) trên tướng.
/// Kế thừa từ ItemSlot, thêm thuộc tính đặc thù cho ô trang bị:
/// - PlayableUnit: tướng sở hữu ô này.
/// - BlockOtherSlot / BlockedByOtherSlot: cơ chế khóa slot cho vũ khí 2 tay.
///   Khi trang bị vũ khí 2 tay ở RightHand → LeftHand bị "block" (không thể trang bị thêm).
/// 
/// Các loại slot: RightHand, LeftHand, Head, Body, Boots, Trinket...
/// </summary>
public class EquipmentSlot : ItemSlot
{
	#region Fields

	/// <summary>Backing field cho BlockOtherSlot - slot đang bị khóa bởi vũ khí 2 tay.</summary>
	private EquipmentSlot blockOtherSlot;

	#endregion Fields

	#region Properties

	/// <summary>
	/// Slot đang bị khóa bởi slot KHÁC (slot này bị block).
	/// Ví dụ: slot LeftHand bị block bởi slot RightHand khi RightHand trang bị vũ khí 2 tay.
	/// </summary>
	public EquipmentSlot BlockedByOtherSlot { get; set; }

	/// <summary>
	/// Slot mà slot này đang KHÓA (slot này block slot khác).
	/// Khi set giá trị mới:
	/// 1. Hủy liên kết với slot cũ (cũ.BlockedByOtherSlot = null).
	/// 2. Thiết lập liên kết mới (mới.BlockedByOtherSlot = this).
	/// Ví dụ: slot RightHand (có vũ khí 2 tay) block slot LeftHand.
	/// </summary>
	public EquipmentSlot BlockOtherSlot
	{
		get
		{
			return blockOtherSlot;
		}
		set
		{
			if (blockOtherSlot != value)
			{
				// Hủy liên kết cũ
				if (blockOtherSlot != null)
				{
					blockOtherSlot.BlockedByOtherSlot = null;
				}
				blockOtherSlot = value;
				// Thiết lập liên kết mới
				if (value != null)
				{
					blockOtherSlot.BlockedByOtherSlot = this;
				}
			}
		}
	}

	/// <summary>Cast ItemSlotController thành EquipmentSlotController.</summary>
	public EquipmentSlotController EquipmentSlotController => base.ItemSlotController as EquipmentSlotController;

	/// <summary>
	/// Cast ItemSlotView thành EquipmentSlotView.
	/// Setter cũng cập nhật base.ItemSlotView.
	/// </summary>
	public EquipmentSlotView EquipmentSlotView
	{
		get
		{
			return base.ItemSlotView as EquipmentSlotView;
		}
		set
		{
			base.ItemSlotView = value;
		}
	}

	/// <summary>
	/// Tướng (Playable Unit) sở hữu ô trang bị này.
	/// Dùng để truy cập stats, perks, body parts khi equip/unequip.
	/// </summary>
	public PlayableUnit PlayableUnit { get; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo EquipmentSlot từ dữ liệu save (deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu serialized của ô.</param>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot.</param>
	/// <param name="equipmentSlotController">Controller quản lý ô.</param>
	/// <param name="equipmentSlotView">View component hiển thị ô.</param>
	/// <param name="unit">Tướng sở hữu ô này.</param>
	public EquipmentSlot(SerializedItemSlot container, ItemSlotDefinition itemSlotDefinition, EquipmentSlotController equipmentSlotController, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemSlotDefinition, equipmentSlotController, equipmentSlotView)
	{
		PlayableUnit = unit;
		Deserialize(container);
	}

	/// <summary>
	/// Constructor tạo EquipmentSlot mới (chưa có item).
	/// </summary>
	/// <param name="itemSlotDefinition">Định nghĩa ô slot.</param>
	/// <param name="equipmentSlotController">Controller quản lý ô.</param>
	/// <param name="equipmentSlotView">View component hiển thị ô.</param>
	/// <param name="unit">Tướng sở hữu ô này.</param>
	public EquipmentSlot(ItemSlotDefinition itemSlotDefinition, EquipmentSlotController equipmentSlotController, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemSlotDefinition, equipmentSlotController, equipmentSlotView)
	{
		PlayableUnit = unit;
	}

	#endregion Constructors
}
