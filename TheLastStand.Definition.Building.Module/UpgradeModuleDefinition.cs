using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class UpgradeModuleDefinition : BuildingModuleDefinition
{
	public List<BuildingUpgradeDefinition> BuildingUpgradeDefinitions { get; private set; }

	public string UpgradeOf { get; set; }

	public UpgradeModuleDefinition(BuildingDefinition buildingDefinition, XContainer upgradeDefinition)
		: base(buildingDefinition, upgradeDefinition)
	{
	}

	public List<string> GetPreviousUpgrades()
	{
		List<string> list = new List<string>();
		BuildingDefinition buildingDefinition = BuildingDefinition;
		while (buildingDefinition.UpgradeModuleDefinition.UpgradeOf != null)
		{
			list.Add(buildingDefinition.UpgradeModuleDefinition.UpgradeOf);
			buildingDefinition = BuildingDatabase.BuildingDefinitions[buildingDefinition.UpgradeModuleDefinition.UpgradeOf];
		}
		return list;
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("UpgradeOf");
		if (xElement2 != null)
		{
			if (xElement2.IsNullOrEmpty())
			{
				Debug.LogError("Building " + BuildingDefinition.Id + " has an invalid UpgradeOf !");
				return;
			}
			UpgradeOf = xElement2.Value;
		}
		XElement xElement3 = xElement.Element("BuildingUpgradeDefinitions");
		if (xElement3 == null)
		{
			return;
		}
		BuildingUpgradeDefinitions = new List<BuildingUpgradeDefinition>();
		foreach (XElement item in xElement3.Elements("BuildingUpgradeDefinition"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				Debug.LogError("BuildingDefinition " + BuildingDefinition.Id + " BuildingUpgradeDefinition must have an attribute Id");
			}
			if (BuildingDatabase.BuildingUpgradeDefinitions.TryGetValue(xAttribute.Value, out var value))
			{
				BuildingUpgradeDefinitions.Add(value);
				continue;
			}
			Debug.LogError("BuildingUpgradeDefinition " + xAttribute.Value + " not found");
			break;
		}
	}
}
