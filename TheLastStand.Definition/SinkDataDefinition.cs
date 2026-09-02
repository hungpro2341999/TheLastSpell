using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class SinkDataDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public int BasePrice { get; private set; }

	public float RerollMultiplier { get; private set; }

	public int RoundedTo { get; private set; }

	public Node FinalPrice { get; private set; }

	public SinkDataDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("SinkDataDefinition must have an Id.");
			return;
		}
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("BasePrice");
		if (xElement2.IsNullOrEmpty())
		{
			TPDebug.LogError("SinkDataDefinition " + Id + " needs a BasePrice.");
			return;
		}
		if (!int.TryParse(xElement2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			TPDebug.LogError("SinkDataDefinition " + Id + "'s BasePrice " + HasAnInvalidInt(xElement2.Value));
			return;
		}
		BasePrice = result;
		XElement xElement3 = xElement.Element("RerollMultiplier");
		if (xElement3.IsNullOrEmpty())
		{
			TPDebug.LogError("SinkDataDefinition " + Id + " needs a RerollMultiplier.");
			return;
		}
		if (!float.TryParse(xElement3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			TPDebug.LogError("SinkDataDefinition " + Id + "'s RerollMultiplier " + HasAnInvalidFloat(xElement3.Value));
			return;
		}
		RerollMultiplier = result2;
		XElement xElement4 = xElement.Element("RoundedTo");
		if (xElement4.IsNullOrEmpty())
		{
			TPDebug.LogError("SinkDataDefinition " + Id + " needs a RoundedTo.");
			return;
		}
		if (!int.TryParse(xElement4.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3))
		{
			TPDebug.LogError("SinkDataDefinition " + Id + "'s RoundedTo " + HasAnInvalidInt(xElement4.Value));
			return;
		}
		RoundedTo = result3;
		XElement xElement5 = xElement.Element("FinalPrice");
		if (xElement5.IsNullOrEmpty())
		{
			TPDebug.LogError("SinkDataDefinition " + Id + " needs a FinalPrice.");
		}
		else
		{
			FinalPrice = Parser.Parse(xElement5.Value);
		}
	}
}
