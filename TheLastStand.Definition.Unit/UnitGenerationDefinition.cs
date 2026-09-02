using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> PlayableUnitGenerationDefinitionArchetypeIds { get; set; }

	public UnitGenerationLevelDefinition UnitGenerationLevelDefinition { get; set; }

	public UnitGenerationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("PlayableUnitGenerationDefinitions");
		if (xElement == null)
		{
			Debug.LogError("UnitGenerationStartDefinition does not contain any PlayableUnitGenerationDefinitions");
			return;
		}
		PlayableUnitGenerationDefinitionArchetypeIds = new List<string>();
		foreach (XElement item in xElement.Elements("PlayableUnitGenerationDefinition"))
		{
			XAttribute xAttribute = item.Attribute("ArchetypeId");
			if (xAttribute == null)
			{
				Debug.LogError("PlayableUnitGenerationDefinition must have an attribute ArchetypeId");
			}
			else
			{
				PlayableUnitGenerationDefinitionArchetypeIds.Add(xAttribute.Value);
			}
		}
		XElement xElement2 = container.Element("UnitGenerationLevelDefinition");
		if (xElement2 == null)
		{
			Debug.LogError("UnitGenerationStartDefinition does not contain any UnitGenerationLevelDefinition");
			return;
		}
		XAttribute xAttribute2 = xElement2.Attribute("Id");
		if (xAttribute2.IsNullOrEmpty())
		{
			Debug.LogError("UnitGenerationLevelDefinition must have a valid Id");
		}
		else if (!PlayableUnitDatabase.UnitGenerationLevelDefinitions.ContainsKey(xAttribute2.Value))
		{
			Debug.LogError("PlayableUnitManager.UnitGenerationLevelDefinitions does not contain this id: " + xAttribute2.Value);
		}
		else
		{
			UnitGenerationLevelDefinition = PlayableUnitDatabase.UnitGenerationLevelDefinitions[xAttribute2.Value];
		}
	}
}
