using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class ConstructionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public float RepairCostRatio { get; private set; }

	public Dictionary<string, List<BuildingDefinition.E_BuildingCategory>> RepairCategoryButtons { get; } = new Dictionary<string, List<BuildingDefinition.E_BuildingCategory>>();

	public ConstructionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("RepairCostRatio");
		if (xElement2 == null)
		{
			CLoggerManager.Log("ConstructionDefinition must have a RepairCostRatio", LogType.Error);
			return;
		}
		if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log("ConstructionDefinition RepairCostRatio must be a valid float", LogType.Error);
			return;
		}
		RepairCostRatio = result * 0.01f;
		foreach (XElement item in xElement.Element("RepairCategoryButtons").Elements("RepairCategoryButton"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			List<BuildingDefinition.E_BuildingCategory> list = new List<BuildingDefinition.E_BuildingCategory>();
			foreach (XElement item2 in item.Elements("FlagId"))
			{
				if (!Enum.TryParse<BuildingDefinition.E_BuildingCategory>(item2.Value, out var result2))
				{
					CLoggerManager.Log("Could not parse " + item2.Value + " as a valid E_BuildingCategory.");
				}
				else
				{
					list.Add(result2);
				}
			}
			RepairCategoryButtons.Add(xAttribute.Value, list);
		}
	}
}
