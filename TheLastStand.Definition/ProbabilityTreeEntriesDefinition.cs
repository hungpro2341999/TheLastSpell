using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class ProbabilityTreeEntriesDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; set; }

	public Dictionary<int, int> ProbabilityLevels { get; set; } = new Dictionary<int, int>();

	public ProbabilityTreeEntriesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("ProbabilityLevelsElement must have a valid Id");
			return;
		}
		Id = xAttribute.Value;
		foreach (XElement item in xElement.Elements("Probability"))
		{
			XAttribute xAttribute2 = item.Attribute("Weight");
			int result2;
			if (xAttribute2.IsNullOrEmpty() || !int.TryParse(xAttribute2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				Debug.LogError("ProbabilityLevels " + Id + " Invalid weight!");
			}
			else if (!int.TryParse(item.Value, out result2))
			{
				Debug.LogError("ProbabilityLevels " + Id + " Invalid value!");
			}
			else
			{
				ProbabilityLevels.Add(result2, result);
			}
		}
	}
}
