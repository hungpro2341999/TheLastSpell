using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Tinh chỉnh tạo vật phẩm (CreateItemModifier).
/// <para>Cho phép thay đổi số lượng hoặc công thức sinh (Count Node) cho một định nghĩa CreateItem cụ thể.</para>
/// </summary>
public class CreateItemModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "CreateItemModifier";

	/// <summary>
	/// Mã định danh của CreateItemDefinition chịu tác động.
	/// </summary>
	public string CreateItemId { get; private set; }

	/// <summary>
	/// Nút cây biểu thức toán học (Expression Node) tính toán số lượng vật phẩm sinh ra.
	/// </summary>
	public Node Count { get; private set; }

	public CreateItemModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("CreateItemId");
		CreateItemId = xElement.Value;
		XElement xElement2 = obj.Element("Count");
		Count = (xElement2.IsNullOrEmpty() ? Parser.Parse("1") : Parser.Parse(xElement2.Value));
	}
}
