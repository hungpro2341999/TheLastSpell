using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemLevelsListDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; set; }

	public Dictionary<int, int> ItemLevelsWithOdd { get; set; } = new Dictionary<int, int>();

	public ItemLevelsListDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("xItemLevelsListDefinition must have a valid Id");
			return;
		}
		Id = xAttribute.Value;
		foreach (XElement item in xElement.Elements("ItemLevel"))
		{
			XAttribute xAttribute2 = item.Attribute("Odd");
			if (xAttribute2.IsNullOrEmpty() || !int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError(Id + " Invalid odd!");
				continue;
			}
			XAttribute xAttribute3 = item.Attribute("Id");
			if (xAttribute3.IsNullOrEmpty() || !int.TryParse(xAttribute3.Value, out var result2))
			{
				Debug.LogError(Id + " Invalid level!");
			}
			else
			{
				ItemLevelsWithOdd.Add(result2, result);
			}
		}
	}
}
