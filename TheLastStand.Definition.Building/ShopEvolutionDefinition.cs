using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building;

public class ShopEvolutionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<Tuple<int, int>> LevelsPerDay { get; private set; }

	public ShopEvolutionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		LevelsPerDay = new List<Tuple<int, int>>();
		foreach (XElement item in obj.Elements("Day"))
		{
			XAttribute xAttribute = item.Attribute("Index");
			XAttribute xAttribute2 = item.Attribute("Level");
			LevelsPerDay.Add(new Tuple<int, int>(int.Parse(xAttribute.Value), int.Parse(xAttribute2.Value)));
		}
	}
}
