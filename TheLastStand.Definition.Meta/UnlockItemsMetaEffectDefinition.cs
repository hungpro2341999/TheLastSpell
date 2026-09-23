using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Manager.Item;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa vật phẩm (UnlockItems).
/// <para>Cho phép đưa các loại vũ khí và trang bị mới vào danh sách rơi đồ (drop pool) và mua trong shop.</para>
/// </summary>
public class UnlockItemsMetaEffectDefinition : MetaEffectDefinition
{
	/// <summary>
	/// Tên định danh của thẻ XML ("UnlockItems").
	/// </summary>
	public const string Name = "UnlockItems";

	/// <summary>
	/// Tên của các thẻ con chứa Id vật phẩm ("Item").
	/// </summary>
	public const string ChildName = "Item";

	/// <summary>
	/// Danh sách các mã định danh (Id) của vật phẩm được mở khóa.
	/// </summary>
	public readonly List<string> ItemsToUnlock = new List<string>();

	public UnlockItemsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc danh sách Id vật phẩm từ các thẻ con &lt;Item&gt;.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Item"))
		{
			if (!string.IsNullOrEmpty(item.Value))
			{
				ItemsToUnlock.Add(item.Value);
			}
		}
	}

	/// <summary>
	/// Khi hiệu ứng Meta được kích hoạt, cập nhật trạng thái mở khóa trong ItemRestrictionManager.
	/// </summary>
	/// <param name="hasBeenActivated">True nếu kích hoạt mở khóa; False nếu thu hồi.</param>
	public override void OnMetaEffectActivated(bool hasBeenActivated)
	{
		base.OnMetaEffectActivated(hasBeenActivated);
		foreach (string item in ItemsToUnlock)
		{
			if (ItemDatabase.ItemDefinitions.TryGetValue(item, out var value))
			{
				// Làm mới các họ vật phẩm bị khóa theo Category tương ứng
				ItemRestrictionManager.RefreshItemFamiliesLockedItemsFromCategory(value.Category, hasBeenActivated);
			}
		}
	}
}
