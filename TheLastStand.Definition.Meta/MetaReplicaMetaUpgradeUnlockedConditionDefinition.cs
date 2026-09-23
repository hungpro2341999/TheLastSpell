using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Điều kiện câu thoại Meta: Yêu cầu nâng cấp Meta cụ thể đã được mở khóa (MetaUpgradeUnlocked).
/// </summary>
public class MetaReplicaMetaUpgradeUnlockedConditionDefinition : MetaReplicaConditionDefinition
{
	public class Constants
	{
		public const string Name = "MetaUpgradeUnlocked";
	}

	/// <summary>
	/// Mã định danh của nâng cấp Meta cần phải được mở khóa trước.
	/// </summary>
	public string UpgradeId { get; private set; }

	public MetaReplicaMetaUpgradeUnlockedConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		UpgradeId = xElement.Value;
	}
}
