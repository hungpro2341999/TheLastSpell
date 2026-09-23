using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa nâng cấp công trình (UnlockBuildingUpgrade).
/// <para>Cho phép người chơi nâng cấp công trình lên bậc mới với các chức năng nâng cao.</para>
/// </summary>
public class UnlockBuildingUpgradeMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockBuildingUpgrade";

	/// <summary>
	/// Mã định danh của nhánh nâng cấp công trình được mở khóa.
	/// </summary>
	public string UpgradeId { get; private set; }

	public UnlockBuildingUpgradeMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		UpgradeId = xElement.Value;
	}

	public override string ToString()
	{
		return "UnlockBuildingUpgrade (" + UpgradeId + ")";
	}
}
