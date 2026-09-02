using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Apocalypse.LightFogSpawner;

public class LightFogSpawnersGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int InitialCount { get; private set; }

	public int ScalingCount { get; private set; }

	public int Period { get; private set; }

	public List<Tuple<int, string>> BuildingToSpawnIds { get; private set; }

	public LightFogSpawnersGenerationDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			InitialCount = 0;
			ScalingCount = 0;
			Period = 1;
			return;
		}
		XElement xElement2 = xElement.Element("InitialCount");
		XAttribute xAttribute = xElement2.Attribute("Value");
		int.TryParse(xAttribute.Value, out var result);
		InitialCount = result;
		xElement2 = xElement.Element("ScalingCount");
		xAttribute = xElement2.Attribute("Value");
		int.TryParse(xAttribute.Value, out var result2);
		ScalingCount = result2;
		xAttribute = xElement2.Attribute("Period");
		if (!int.TryParse(xAttribute.Value, out var result3))
		{
			result3 = 1;
		}
		Period = result3;
		BuildingToSpawnIds = new List<Tuple<int, string>>
		{
			new Tuple<int, string>(0, "LightFogSpawner_Alive")
		};
		xElement2 = xElement.Element("LightFogSpawnersToSpawn");
		if (xElement2 == null)
		{
			return;
		}
		foreach (XElement item in xElement2.Elements("LightFogSpawnerToSpawn"))
		{
			xAttribute = item.Attribute("DayIndex");
			int.TryParse(xAttribute.Value, out var result4);
			xAttribute = item.Attribute("BuildingId");
			if (result4 == 0 && BuildingToSpawnIds.Count > 0)
			{
				BuildingToSpawnIds.RemoveAt(0);
			}
			BuildingToSpawnIds.Add(new Tuple<int, string>(result4, xAttribute.Value));
		}
	}
}
