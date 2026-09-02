using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy;

public class TierDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Index { get; private set; }

	public float LifetimeStatsWeight { get; private set; }

	public TierDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Index");
		if (int.TryParse(xAttribute.Value, out var result))
		{
			Index = result;
		}
		else
		{
			CLoggerManager.Log("TierDefinition: Could not parse index " + xAttribute.Value + " to an int.");
		}
		XElement xElement = obj.Element("LifetimeStatsWeight");
		if (float.TryParse(xElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			LifetimeStatsWeight = result2;
		}
		else
		{
			CLoggerManager.Log("TierDefinition: Could not parse LifetimeStatsWeight " + xElement.Value + " to a float.");
		}
	}
}
