using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Khóa vật phẩm (LockItems).
/// <para>Khóa không cho các vật phẩm hoặc vũ khí cụ thể xuất hiện trong danh sách rơi đồ và cửa hàng.</para>
/// </summary>
public class LockItemsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "LockItems";

	public const string ChildName = "Item";

	/// <summary>
	/// Danh sách các Id vật phẩm bị khóa.
	/// </summary>
	public readonly List<string> ItemsToLock = new List<string>();

	public LockItemsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Item"))
		{
			if (!string.IsNullOrEmpty(item.Value))
			{
				ItemsToLock.Add(item.Value);
			}
		}
	}
}
