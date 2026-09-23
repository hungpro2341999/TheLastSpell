using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa công trình (UnlockBuilding).
/// <para>Cho phép người chơi xây dựng một loại công trình mới trong thành phố.</para>
/// </summary>
public class UnlockBuildingMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockBuilding";

	/// <summary>
	/// Mã định danh của công trình được mở khóa (ví dụ: "Watchtower", "Inn"...).
	/// </summary>
	public string BuildingId { get; private set; }

	public UnlockBuildingMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingId = xElement.Value;
	}

	public override string ToString()
	{
		return "UnlockBuilding (" + BuildingId + ")";
	}
}
