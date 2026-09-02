using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypseTierDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int ApocalypseLevelCompletedToUnlock { get; private set; }

	public string Id { get; private set; }

	public ApocalypseTierDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute == null || string.IsNullOrEmpty(xAttribute.Value))
		{
			CLoggerManager.Log("An apocalypse's tier Id is not defined or empty !", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		if (!int.TryParse(xElement.Element("ApocalypseLevelCompletedToUnlock")?.Value, out var result))
		{
			CLoggerManager.Log("The apocalypse tier " + Id + " ApocalypseLevelCompletedToUnlock " + HasAnInvalidInt(xAttribute.Value) + " !", LogType.Error);
		}
		else
		{
			ApocalypseLevelCompletedToUnlock = result;
		}
	}
}
