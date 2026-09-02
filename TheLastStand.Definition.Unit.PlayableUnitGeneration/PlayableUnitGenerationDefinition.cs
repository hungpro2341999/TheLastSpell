using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.PlayableUnitGeneration;

public class PlayableUnitGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string ArchetypeId { get; private set; }

	public List<string> BackgroundTraitAvailableIds { get; private set; }

	public Dictionary<ItemSlotDefinition.E_ItemSlotId, EquipmentGenerationDefinition> EquipmentGenerationDefinitions { get; private set; }

	public Dictionary<UnitStatDefinition.E_Stat, StatGenerationDefinition> StatGenerationDefinitions { get; private set; }

	public int BaseGenerationLevel { get; set; }

	public PlayableUnitGenerationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("ArchetypeId");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("PlayableUnitGenerationDefinition must have an attribute ArchetypeId");
			return;
		}
		ArchetypeId = xAttribute.Value;
		StatGenerationDefinitions = new Dictionary<UnitStatDefinition.E_Stat, StatGenerationDefinition>(UnitStatDefinition.SharedStatComparer);
		foreach (XElement item in xElement.Element("StatGenerationDefinitions").Elements("StatGenerationDefinition"))
		{
			StatGenerationDefinition statGenerationDefinition = new StatGenerationDefinition(item, ArchetypeId);
			StatGenerationDefinitions.Add(statGenerationDefinition.Stat, statGenerationDefinition);
		}
		EquipmentGenerationDefinitions = new Dictionary<ItemSlotDefinition.E_ItemSlotId, EquipmentGenerationDefinition>();
		XElement xElement2 = xElement.Element("EquipmentGenerationDefinitions");
		foreach (XElement item2 in xElement2.Elements("EquipmentGenerationDefinition"))
		{
			EquipmentGenerationDefinition equipmentGenerationDefinition = new EquipmentGenerationDefinition(item2);
			EquipmentGenerationDefinitions.Add(equipmentGenerationDefinition.Slot, equipmentGenerationDefinition);
		}
		if (int.TryParse(xElement2.Element("BaseGenerationLevel").Value, out var result))
		{
			BaseGenerationLevel = result;
		}
		else
		{
			Debug.LogError("Could not parse InitialGenerationLevel value, setting it to 0!");
			BaseGenerationLevel = 0;
		}
		XElement xElement3 = xElement.Element("UnitTraitDefinitions");
		if (xElement3 == null)
		{
			TPDebug.LogError("PlayableUnitGenerationDefinition must have an element UnitTraitDefinitions");
			return;
		}
		BackgroundTraitAvailableIds = new List<string>();
		foreach (XElement item3 in xElement3.Elements("UnitTraitDefinition"))
		{
			XAttribute xAttribute2 = item3.Attribute("Id");
			if (xAttribute2.IsNullOrEmpty())
			{
				TPDebug.LogError("PlayableUnitGenerationDefinition must have an attribute Id");
			}
			else
			{
				BackgroundTraitAvailableIds.Add(xAttribute2.Value);
			}
		}
	}
}
