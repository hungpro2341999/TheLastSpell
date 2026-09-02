using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit;

public class LinkedHairDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Name { get; private set; }

	public int Weight { get; private set; }

	public LinkedHairDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (int.TryParse(xElement.Attribute("Weight").Value, out var result))
		{
			Weight = result;
		}
		Name = xElement.Value;
	}
}
