using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa đặc điểm/tính cách tướng (UnlockTraits).
/// <para>Cho phép các đặc điểm (Traits) mới xuất hiện khi sinh tướng ngẫu nhiên.</para>
/// </summary>
public class UnlockTraitsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockTraits";

	public const string ChildName = "Trait";

	/// <summary>
	/// Danh sách các Id Trait được mở khóa.
	/// </summary>
	public List<string> TraitsToUnlock = new List<string>();

	public UnlockTraitsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		foreach (XElement item in (container as XElement).Elements("Trait"))
		{
			if (item.Value != string.Empty)
			{
				TraitsToUnlock.Add(item.Value);
			}
		}
	}
}
