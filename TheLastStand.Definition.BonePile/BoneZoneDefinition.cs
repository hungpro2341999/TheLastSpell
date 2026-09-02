using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.BonePile;

public class BoneZoneDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public int MinHavenDistance { get; private set; } = -1;

	public int MaxMagicCircleDistance { get; private set; } = -1;

	public BoneZoneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("MinHavenDistance");
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse MinHavenDistance element value " + xElement2.Value + " as a valid int! (BoneZone Id=" + Id + ")");
				return;
			}
			MinHavenDistance = result;
		}
		XElement xElement3 = xElement.Element("MaxMagicCircleDistance");
		if (xElement3 != null)
		{
			if (!int.TryParse(xElement3.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse MaxMagicCircleDistance element value " + xElement3.Value + " as a valid int! (BoneZone Id=" + Id + ")");
			}
			else
			{
				MaxMagicCircleDistance = result2;
			}
		}
	}
}
