using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa tạo trang bị (UnlockEquipmentGeneration).
/// <para>Kích hoạt tính năng sinh trang bị ngẫu nhiên theo Id cấu hình.</para>
/// </summary>
public class UnlockEquipmentGenerationMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockEquipmentGeneration";

	/// <summary>
	/// Mã định danh của cấu hình sinh trang bị được mở khóa.
	/// </summary>
	public string Id { get; private set; }

	public UnlockEquipmentGenerationMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Id");
		Id = xAttribute.Value;
	}
}
