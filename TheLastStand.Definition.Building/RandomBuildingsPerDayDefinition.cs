using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class RandomBuildingsPerDayDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public Dictionary<int, Dictionary<RandomBuildingsDirectionsDefinition, int>> RandomBuildingsPerDayDefinitions { get; } = new Dictionary<int, Dictionary<RandomBuildingsDirectionsDefinition, int>>();

	public RandomBuildingsPerDayDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		foreach (XElement item in obj.Elements("RandomBuildingsDirectionsDefinitions"))
		{
			XAttribute xAttribute2 = item.Attribute("DayNumber");
			if (xAttribute2.IsNullOrEmpty())
			{
				CLoggerManager.Log("RandomBuildingsDirectionsPerDayDefinition must have a DayNumber attribute!", LogType.Error);
				continue;
			}
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("DayNumber must be a valid int value!", LogType.Error);
				continue;
			}
			Dictionary<RandomBuildingsDirectionsDefinition, int> dictionary = new Dictionary<RandomBuildingsDirectionsDefinition, int>();
			foreach (XElement item2 in item.Elements("RandomBuildingsDirectionsDefinition"))
			{
				XAttribute xAttribute3 = item2.Attribute("Id");
				RandomBuildingsDirectionsDefinition key = BuildingDatabase.RandomBuildingsDirectionsDefinitions[xAttribute3.Value];
				XAttribute xAttribute4 = item2.Attribute("Weight");
				int value = 1;
				if (xAttribute4 != null)
				{
					if (!int.TryParse(xAttribute4.Value, out var result2))
					{
						CLoggerManager.Log("Could not parse RandomBuildingsDirectionsDefinition weight " + xAttribute4.Value + " to a valid int! Setting it to 100.", LogType.Error);
					}
					value = result2;
				}
				dictionary.Add(key, value);
			}
			RandomBuildingsPerDayDefinitions.Add(result, dictionary);
		}
	}
}
