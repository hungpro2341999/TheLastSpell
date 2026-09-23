using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa các Affix (UnlockAffixes).
/// <para>Cho phép các dòng thuộc tính bổ trợ (Affix) mới xuất hiện khi tạo hoặc rớt trang bị.</para>
/// </summary>
public class UnlockAffixesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockAffixes";

	public const string ChildName = "Affix";

	/// <summary>
	/// Danh sách các Id Affix được mở khóa.
	/// </summary>
	public List<string> AffixesToUnlock = new List<string>();

	public UnlockAffixesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Affix"))
		{
			if (item.Value != string.Empty)
			{
				AffixesToUnlock.Add(item.Value);
			}
		}
	}
}
