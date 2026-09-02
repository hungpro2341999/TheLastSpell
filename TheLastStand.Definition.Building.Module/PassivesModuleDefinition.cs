using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class PassivesModuleDefinition : BuildingModuleDefinition
{
	private List<BuildingPassiveDefinition> buildingPassiveDefinitions;

	public List<BuildingPassiveDefinition> BuildingPassiveDefinitions
	{
		get
		{
			if (!TPSingleton<GlyphManager>.Exist())
			{
				return buildingPassiveDefinitions;
			}
			return TPSingleton<GlyphManager>.Instance.GetModifiedBuildingPassives(BuildingDefinition.Id, buildingPassiveDefinitions);
		}
	}

	public bool HasOnDeathEffect { get; private set; }

	public PassivesModuleDefinition(BuildingDefinition buildingDefinition, XContainer passivesDefinition)
		: base(buildingDefinition, passivesDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		buildingPassiveDefinitions = new List<BuildingPassiveDefinition>();
		foreach (XElement item in xElement.Elements("BuildingPassiveDefinition"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (!BuildingDatabase.BuildingPassiveDefinitions.TryGetValue(xAttribute.Value, out var value))
			{
				CLoggerManager.Log("Could not find building passive with the id (" + xAttribute.Value + ").", LogType.Error, CLogLevel.MAJOR);
			}
			else
			{
				buildingPassiveDefinitions.Add(value);
			}
		}
		HasOnDeathEffect = buildingPassiveDefinitions.Any((BuildingPassiveDefinition x) => x.HasOnDeathEffect);
	}
}
