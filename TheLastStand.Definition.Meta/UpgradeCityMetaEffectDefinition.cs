using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Nâng cấp thành phố (UpgradeCity).
/// <para>Thiết lập cấp độ mới cho thành phố, mở rộng tính năng và mở thêm công trình.</para>
/// </summary>
public class UpgradeCityMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UpgradeCity";

	/// <summary>
	/// Mã định danh của thành phố được nâng cấp.
	/// </summary>
	public string CityId { get; private set; }

	/// <summary>
	/// Cấp độ (Level) mới được nâng lên của thành phố.
	/// </summary>
	public int Level { get; private set; }

	public UpgradeCityMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("Id");
		CityId = xElement.Value;
		if (!int.TryParse(obj.Element("Level").Value, out var result))
		{
			CLoggerManager.Log("Could not cast the level value into an int !", LogType.Error);
		}
		else
		{
			Level = result;
		}
	}
}
