using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building;

public class RandomBuildingsDirectionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public bool DisableRotation { get; private set; }

	public Dictionary<SpawnDirectionsDefinition.E_Direction, RandomBuildingsGenerationDefinition> GenerationDefinitionByDirection { get; } = new Dictionary<SpawnDirectionsDefinition.E_Direction, RandomBuildingsGenerationDefinition>();

	public RandomBuildingsDirectionsDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		DisableRotation = xElement.Element("DisableRotation") != null;
		foreach (XElement item in xElement.Elements())
		{
			if (Enum.TryParse<SpawnDirectionsDefinition.E_Direction>(item.Name.LocalName, out var result))
			{
				RandomBuildingsGenerationDefinition value = BuildingDatabase.RandomBuildingsGenerationDefinitions[item.Value];
				GenerationDefinitionByDirection.Add(result, value);
			}
		}
	}
}
