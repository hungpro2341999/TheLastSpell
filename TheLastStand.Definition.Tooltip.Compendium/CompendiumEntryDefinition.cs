using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class CompendiumEntryDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public bool DisplayLinkedEntries { get; private set; } = true;

	public CompendiumEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		XElement xElement = obj.Element("DisplayLinkedEntries");
		if (xElement != null)
		{
			if (bool.TryParse(xElement.Value, out var result))
			{
				DisplayLinkedEntries = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse DisplayLinkedEntries in CompendiumEntryDefinition element into a bool", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "CompendiumEntryDefinition");
			}
		}
	}
}
