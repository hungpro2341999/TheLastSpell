using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Tooltip.Compendium;

public abstract class ACompendiumEntryDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public HashSet<string> LinkedEntries { get; private set; }

	protected ACompendiumEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		LinkedEntries = new HashSet<string>();
		XElement xElement = obj.Element("LinkedEntries");
		if (xElement == null)
		{
			return;
		}
		foreach (XElement item in xElement.Elements("LinkedEntry"))
		{
			XAttribute xAttribute2 = item.Attribute("Id");
			LinkedEntries.Add(xAttribute2.Value);
		}
	}
}
