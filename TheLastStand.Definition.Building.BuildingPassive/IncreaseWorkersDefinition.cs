using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class IncreaseWorkersDefinition : BuildingPassiveEffectDefinition
{
	public Node Value { get; set; }

	public IncreaseWorkersDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Value = Parser.Parse(xElement.Element("Value").Value);
	}
}
