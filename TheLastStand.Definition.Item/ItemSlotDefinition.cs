using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Bản thiết kế 1 ô slot trang bị trên tướng.
/// Định nghĩa slot chấp nhận item category nào và kiểu tay nào.
/// 
/// Ví dụ:
/// - Slot "RightHand": chấp nhận MeleeWeapon, RangeWeapon, MagicWeapon — Hands: OneHand, TwoHands
/// - Slot "Body": chấp nhận tất cả BodyArmor — Hands: None
/// - Slot "Usables": chấp nhận Potion, Scroll — Hands: None
/// 
/// Ví dụ XML:
/// <code>
/// &lt;ItemSlot Id="RightHand"&gt;
///   &lt;Categories&gt;
///     &lt;Category&gt;MeleeWeapon&lt;/Category&gt;
///     &lt;Category&gt;RangeWeapon&lt;/Category&gt;
///     &lt;Category&gt;MagicWeapon&lt;/Category&gt;
///   &lt;/Categories&gt;
///   &lt;Hands&gt;
///     &lt;Hand&gt;OneHand&lt;/Hand&gt;
///     &lt;Hand&gt;TwoHands&lt;/Hand&gt;
///   &lt;/Hands&gt;
/// &lt;/ItemSlot&gt;
/// </code>
/// </summary>
public class ItemSlotDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Enum flags xác định loại slot (có thể kết hợp bằng bitwise OR).
	/// 
	/// Slot vật lý trên tướng: RightHand, LeftHand, Body, Foot, Head, Trinket, Usables.
	/// Slot hệ thống: Inventory (túi đồ), Shop (cửa hàng), RewardItem (phần thưởng).
	/// </summary>
	[Flags]
	public enum E_ItemSlotId
	{
		None = 0,
		/// <summary>Tay phải — vũ khí chính.</summary>
		RightHand = 1,
		/// <summary>Tay trái — shield/off-hand weapon.</summary>
		LeftHand = 2,
		/// <summary>Giáp thân.</summary>
		Body = 4,
		/// <summary>Giày.</summary>
		Foot = 8,
		/// <summary>Mũ/helmet.</summary>
		Head = 0x10,
		/// <summary>Phụ kiện (trinket).</summary>
		Trinket = 0x20,
		/// <summary>Ô dùng vật phẩm tiêu hao (potion/scroll).</summary>
		Usables = 0x40,
		/// <summary>Túi đồ chung của cả đội (hệ thống).</summary>
		Inventory = 0x80,
		/// <summary>Cửa hàng (hệ thống).</summary>
		Shop = 0x100,
		/// <summary>Phần thưởng sau trận (hệ thống).</summary>
		RewardItem = 0x200,
		/// <summary>Kết hợp: RightHand | LeftHand — cả 2 slot tay.</summary>
		WeaponSlot = 3,
		/// <summary>Kết hợp: Body | Foot | Head | Trinket — tất cả slot giáp.</summary>
		ArmorSlot = 0x3C,
		/// <summary>Kết hợp: WeaponSlot | ArmorSlot | Usables — tất cả slot trang bị.</summary>
		EquipmentSlot = 0x7F
	}

	/// <summary>Hằng số string IDs cho các slot — dùng khi tham chiếu slot bằng tên.</summary>
	public static class Constants
	{
		public static class Ids
		{
			public const string NoSlot = "NoSlot";
			public const string Body = "Body";
			public const string Foot = "Foot";
			public const string Free = "Free";
			public const string Head = "Head";
			public const string LeftHand = "LeftHand";
			public const string RightHand = "RightHand";
			public const string Trinket = "Trinket";
			public const string Inventory = "Inventory";
			public const string Shop = "Shop";
			public const string RewardItem = "RewardItem";
		}
	}

	/// <summary>Loại slot. Ví dụ: RightHand, Body, Inventory.</summary>
	public E_ItemSlotId Id { get; private set; }

	/// <summary>
	/// Categories mà slot này chấp nhận (flags — có thể nhiều category).
	/// Ví dụ: RightHand chấp nhận MeleeWeapon | RangeWeapon | MagicWeapon.
	/// </summary>
	public ItemDefinition.E_Category Categories { get; private set; }

	/// <summary>
	/// Kiểu tay mà slot này chấp nhận.
	/// Ví dụ: RightHand chấp nhận [OneHand, TwoHands].
	/// Body trả về [None] (không phải item cầm tay).
	/// </summary>
	public List<ItemDefinition.E_Hands> Hands { get; private set; } = new List<ItemDefinition.E_Hands>();

	public ItemSlotDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc slot definition từ XML.
	/// Thứ tự: Id → Categories → Hands (optional, default = None).
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		// Đọc Id
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("The ItemSlotDefinition has no Id!");
			return;
		}
		if (!Enum.TryParse<E_ItemSlotId>(xAttribute.Value, out var result))
		{
			Debug.LogError("An ItemSlotDefinition has an invalid Id " + xAttribute.Value + "!");
		}
		Id = result;
		// Đọc Categories — category nào được đặt vào slot này
		XElement xElement2 = xElement.Element("Categories");
		if (xElement2.IsNullOrEmpty())
		{
			Debug.LogError($"The ItemSlotDefinition {Id} must have a Categories element!");
			return;
		}
		foreach (XElement item in xElement2.Elements("Category"))
		{
			if (!Enum.TryParse<ItemDefinition.E_Category>(item.Value, out var result2))
			{
				Debug.LogError($"The ItemSlotDefinition {Id} has an invalid category {item.Value}!");
			}
			else if (Categories.HasFlag(result2))
			{
				Debug.LogError($"The ItemSlotDefinition {Id} already has the category {result2}!");
			}
			else
			{
				Categories |= result2;
			}
		}
		// Đọc Hands (optional — nếu không có → mặc định None)
		XElement xElement3 = xElement.Element("Hands");
		if (xElement3.IsNullOrEmpty())
		{
			Hands.Add(ItemDefinition.E_Hands.None);
			return;
		}
		foreach (XElement item2 in xElement3.Elements("Hand"))
		{
			if (!Enum.TryParse<ItemDefinition.E_Hands>(item2.Value, out var result3))
			{
				Debug.LogError($"The ItemSlotDefinition {Id} has an invalid hand {item2.Value}!");
			}
			else if (Hands.Contains(result3))
			{
				Debug.LogError($"The ItemSlotDefinition {Id} already has the hand {result3}!");
			}
			else
			{
				Hands.Add(result3);
			}
		}
	}
}
