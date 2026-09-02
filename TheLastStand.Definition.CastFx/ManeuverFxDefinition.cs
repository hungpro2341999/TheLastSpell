using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.CastFx;

public class ManeuverFxDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node Delay { get; private set; }

	public Node Speed { get; private set; }

	public ManeuverFxDefinition(XContainer container)
		: base(container)
	{
	}

	public ManeuverFxDefinition()
		: this(null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container?.Element("Delay");
		Delay = ((xElement != null) ? Parser.Parse(xElement.Value) : new NodeNumber(0.0));
		XElement xElement2 = container?.Element("Speed");
		Speed = ((xElement2 != null) ? Parser.Parse(xElement2.Value) : new NodeNumber(-1.0));
	}
}
