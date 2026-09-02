using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Trophy;

public class DefaultTrophyDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; protected set; }

	public bool IgnoreGem { get; private set; } = true;

	public Dictionary<int, Node> MultiplierPerDay { get; protected set; } = new Dictionary<int, Node>();

	public string BackgroundPath { get; private set; }

	public DefaultTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		XElement xElement = obj.Element("BackgroundPath");
		BackgroundPath = xElement.Value;
		foreach (XElement item in obj.Elements("DamnedSoulsMultiplier"))
		{
			XAttribute xAttribute2 = item.Attribute("NightTarget");
			if (xAttribute2 != null)
			{
				if (!int.TryParse(xAttribute2.Value, out var result))
				{
					TPDebug.LogError("Attribute DayTarget should be an integer !");
				}
				if (MultiplierPerDay.ContainsKey(result))
				{
					MultiplierPerDay[result] = Parser.Parse(item.Value);
				}
				else
				{
					MultiplierPerDay.Add(result, Parser.Parse(item.Value));
				}
			}
		}
	}
}
