using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Meta;

public class CreateItemModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "CreateItemModifier";

	public string CreateItemId { get; private set; }

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
