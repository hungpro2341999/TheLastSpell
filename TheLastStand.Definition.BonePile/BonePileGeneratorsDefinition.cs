using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.BonePile;

public class BonePileGeneratorsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<string, int> Buildings { get; } = new Dictionary<string, int>();

	public Dictionary<string, List<Tuple<int, int>>> BonePileEvolutionDefinitions { get; } = new Dictionary<string, List<Tuple<int, int>>>();

	public Dictionary<string, BonePileGeneratorDefinition> GeneratorsByZoneId { get; } = new Dictionary<string, BonePileGeneratorDefinition>();

	public Dictionary<string, BonePileCountProgressionDefinition> CountProgressionDefinitions { get; } = new Dictionary<string, BonePileCountProgressionDefinition>();

	public BonePileGeneratorsDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		foreach (XElement item in xElement.Element("BuildingIds").Elements("Building"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			XAttribute xAttribute2 = item.Attribute("MinPercentage");
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse BonePile building " + xAttribute.Value + " MinPercentage attribute value " + xAttribute2.Value + " to a valid int!");
				return;
			}
			Buildings.Add(xAttribute.Value, result);
		}
		foreach (XElement item2 in xElement.Element("LevelEvolutions").Elements("LevelEvolution"))
		{
			XAttribute xAttribute3 = item2.Attribute("Id");
			List<Tuple<int, int>> list = new List<Tuple<int, int>>();
			BonePileEvolutionDefinitions.Add(xAttribute3.Value, list);
			foreach (XElement item3 in item2.Elements("Day"))
			{
				XAttribute xAttribute4 = item3.Attribute("Index");
				if (!int.TryParse(xAttribute4.Value, out var result2))
				{
					CLoggerManager.Log("Could not parse Day Index attribute value " + xAttribute4.Value + " to a valid int!", LogType.Error);
					return;
				}
				XAttribute xAttribute5 = item3.Attribute("Level");
				if (!int.TryParse(xAttribute5.Value, out var result3))
				{
					CLoggerManager.Log("Could not parse Day Level attribute value " + xAttribute5.Value + " to a valid int!", LogType.Error);
					return;
				}
				list.Add(new Tuple<int, int>(result2, result3));
			}
		}
		foreach (XElement item4 in xElement.Elements("BonePileGeneratorDefinition"))
		{
			BonePileGeneratorDefinition bonePileGeneratorDefinition = new BonePileGeneratorDefinition(item4);
			GeneratorsByZoneId.Add(bonePileGeneratorDefinition.ZoneId, bonePileGeneratorDefinition);
		}
		foreach (XElement item5 in xElement.Element("BonePileCountProgressions").Elements("BonePileCountProgression"))
		{
			BonePileCountProgressionDefinition bonePileCountProgressionDefinition = new BonePileCountProgressionDefinition(item5);
			CountProgressionDefinitions.Add(bonePileCountProgressionDefinition.CityId, bonePileCountProgressionDefinition);
		}
		foreach (KeyValuePair<string, BonePileCountProgressionDefinition> countProgressionDefinition in CountProgressionDefinitions)
		{
			BonePileCountProgressionDefinition value = countProgressionDefinition.Value;
			if (value.HasTemplate)
			{
				if (!CountProgressionDefinitions.TryGetValue(value.TemplateCityId, out var value2))
				{
					CLoggerManager.Log("No template definition with Id " + value.TemplateCityId + " was found for BonePileCountProgressionDefinition!");
				}
				else
				{
					value.DeserializeUsingTemplate(value2);
				}
			}
		}
	}
}
