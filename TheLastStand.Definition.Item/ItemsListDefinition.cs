using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.DLC;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Danh sách vật phẩm (loot table) với weight (tỉ lệ xuất hiện) cho mỗi item.
/// Giống "bảng loot" trong RPG — quyết định item nào có thể xuất hiện và xác suất bao nhiêu.
/// 
/// Đặc điểm quan trọng:
/// - Danh sách có thể LỒNG NHAU: entry có thể là ItemId hoặc ItemsListId khác.
/// - Hỗ trợ DLC: danh sách có thể bị khóa nếu người chơi chưa mua DLC.
/// - "Odd" = weight (tỉ lệ), KHÔNG phải phần trăm. Xác suất = Odd / tổng Odd.
/// 
/// Ví dụ XML:
/// <code>
/// &lt;ItemsList Id="AllWeapons"&gt;
///   &lt;Item Id="IronSword" Odd="100"/&gt;
///   &lt;Item Id="WoodBow" Odd="80"/&gt;
///   &lt;Item Id="TierTwoWeapons" Odd="50"/&gt;  &lt;!-- Danh sách lồng --&gt;
/// &lt;/ItemsList&gt;
/// </code>
/// 
/// Ví dụ DLC:
/// <code>
/// &lt;ItemsList Id="DLCWeapons" DLCId="dlc_warriors"&gt;
///   &lt;Item Id="FlameAxe" Odd="100"/&gt;
/// &lt;/ItemsList&gt;
/// </code>
/// </summary>
public class ItemsListDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>ID DLC liên kết. Null = base game, không cần DLC.</summary>
	public string DLCId { get; private set; }

	/// <summary>ID duy nhất của danh sách. Ví dụ: "AllWeapons", "TierOneSwords".</summary>
	public string Id { get; private set; }

	/// <summary>True nếu danh sách rỗng (không có item nào).</summary>
	public bool IsEmpty => ItemsWithOdd.Count == 0;

	/// <summary>True nếu danh sách yêu cầu DLC cụ thể.</summary>
	public bool IsLinkedToDLC => !string.IsNullOrEmpty(DLCId);

	/// <summary>
	/// Dictionary chứa các entry: Key = ItemId hoặc ItemsListId, Value = weight (tỉ lệ).
	/// Ví dụ: { "IronSword": 100, "WoodBow": 80, "TierTwoWeapons": 50 }
	/// 
	/// Lưu ý: Key có thể là ID của ItemDefinition HOẶC ID của ItemsListDefinition khác (lồng nhau).
	/// ItemManager xử lý đệ quy khi gặp nested list.
	/// </summary>
	public Dictionary<string, int> ItemsWithOdd { get; } = new Dictionary<string, int>();

	public ItemsListDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc danh sách item từ XML.
	/// Nếu danh sách liên kết DLC mà người chơi chưa mua → bỏ qua (không đọc items).
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		// Đọc Id
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("xItemCategoriesListDefinition must have a valid Id", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		// Đọc DLCId (optional)
		XAttribute xAttribute2 = xElement.Attribute("DLCId");
		if (!xAttribute2.IsNullOrEmpty())
		{
			DLCId = xAttribute2.Value;
		}
		// Nếu yêu cầu DLC mà chưa mua → bỏ qua toàn bộ items
		if (IsLinkedToDLC && !TPSingleton<DLCManager>.Instance.IsDLCOwned(DLCId))
		{
			return;
		}
		// Đọc từng Item entry: Id + Odd (weight)
		foreach (XElement item in xElement.Elements("Item"))
		{
			XAttribute xAttribute3 = item.Attribute("Odd");
			if (xAttribute3.IsNullOrEmpty() || !int.TryParse(xAttribute3.Value, out var result))
			{
				CLoggerManager.Log(Id + " Invalid odd!", LogType.Error);
				continue;
			}
			XAttribute xAttribute4 = item.Attribute("Id");
			if (xAttribute4.IsNullOrEmpty())
			{
				CLoggerManager.Log(Id + " Invalid category!", LogType.Error);
			}
			else
			{
				ItemsWithOdd.Add(xAttribute4.Value, result);
			}
		}
	}
}
