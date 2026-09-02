using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class UnitPerkTemplateDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, int> RequiredPerksCountPerTier { get; private set; }

	public Dictionary<int, UnitPerkCollectionSetDefinition> UnitPerkCollectionSetDefinitions { get; private set; }

	public int TierCount { get; private set; }

	public UnitPerkTemplateDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement unitPerkTiersElement = obj.Element("UnitPerkTiers");
		DeserializeUnitPerkTiers(unitPerkTiersElement);
		XElement unitPerkCollectionSetDefinitionsElement = obj.Element("UnitPerkCollectionSetDefinitions");
		DeserializeUnitPerkCollectionSetDefinitions(unitPerkCollectionSetDefinitionsElement);
		TierCount = 0;
		foreach (KeyValuePair<int, int> item in RequiredPerksCountPerTier)
		{
			if (item.Key > TierCount)
			{
				TierCount = item.Key;
			}
		}
	}

	private void DeserializeUnitPerkTiers(XElement unitPerkTiersElement)
	{
		RequiredPerksCountPerTier = new Dictionary<int, int>();
		foreach (XElement item in unitPerkTiersElement.Elements("UnitPerkTier"))
		{
			XAttribute xAttribute = item.Attribute("Index");
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("Could not parse Index attribute into an int : \"" + xAttribute.Value + "\". Skip.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
				continue;
			}
			XElement xElement = item.Element("RequiredPerksCount");
			if (!int.TryParse(xElement.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse RequiredPerksCount element into an int : \"" + xElement.Value + "\". Skip.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
			}
			else if (RequiredPerksCountPerTier.ContainsKey(result))
			{
				CLoggerManager.Log($"Tried to add the same tier index ({result}) several times in UnitPerkTiers element. Skip.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
			}
			else
			{
				RequiredPerksCountPerTier.Add(result, result2);
			}
		}
	}

	private void DeserializeUnitPerkCollectionSetDefinitions(XElement unitPerkCollectionSetDefinitionsElement)
	{
		UnitPerkCollectionSetDefinitions = new Dictionary<int, UnitPerkCollectionSetDefinition>();
		foreach (XElement item in unitPerkCollectionSetDefinitionsElement.Elements("UnitPerkCollectionSetDefinition"))
		{
			UnitPerkCollectionSetDefinition unitPerkCollectionSetDefinition = new UnitPerkCollectionSetDefinition(item);
			if (UnitPerkCollectionSetDefinitions.ContainsKey(unitPerkCollectionSetDefinition.Index))
			{
				CLoggerManager.Log($"Tried to add the same tier index ({unitPerkCollectionSetDefinition.Index}) several times in UnitPerkCollectionSetDefinitions element. Skip.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
			}
			else
			{
				UnitPerkCollectionSetDefinitions[unitPerkCollectionSetDefinition.Index] = unitPerkCollectionSetDefinition;
			}
		}
	}
}
