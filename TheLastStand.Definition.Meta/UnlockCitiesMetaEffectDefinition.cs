using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa thành phố/bản đồ (UnlockCities).
/// <para>Cho phép người chơi mở khóa các thành phố mới để bắt đầu chiến dịch (Gildenberg, Glenwald, Elderlicht...).</para>
/// </summary>
public class UnlockCitiesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockCities";

	public const string ChildName = "City";

	/// <summary>
	/// Danh sách các Id thành phố được mở khóa.
	/// </summary>
	public List<string> CitiesToUnlock = new List<string>();

	public UnlockCitiesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("City"))
		{
			if (item.Value != string.Empty)
			{
				CitiesToUnlock.Add(item.Value);
			}
		}
	}
}
