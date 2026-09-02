using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

[Serializable]
public class UnitGenerationLevelDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; set; }

	public Dictionary<int, SealedUnitGenerationLevelDefinition> SealDefinitions { get; set; } = new Dictionary<int, SealedUnitGenerationLevelDefinition>();

	public UnitGenerationLevelDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("The UnitGenerationDefinition has no Id!");
			return;
		}
		Id = xAttribute.Value;
		for (int i = 0; i < 7; i++)
		{
			SealedUnitGenerationLevelDefinition sealedUnitGenerationLevelDefinition = ((i == 0 || SealDefinitions[0] == null) ? new SealedUnitGenerationLevelDefinition(this) : SealDefinitions[0].ShallowCopy());
			XElement xElement2 = null;
			if (i == 0)
			{
				xElement2 = xElement.Element("Default");
			}
			else
			{
				foreach (XElement item in xElement.Elements("SealOpenOverride"))
				{
					if (item.Attribute("Seal") != null && int.TryParse(item.Attribute("Seal").Value, out var result) && result == i)
					{
						xElement2 = item;
						break;
					}
				}
			}
			if (xElement2 != null)
			{
				sealedUnitGenerationLevelDefinition.Deserialize(xElement2);
				sealedUnitGenerationLevelDefinition.Seal = i;
				SealDefinitions.Add(sealedUnitGenerationLevelDefinition.Seal, sealedUnitGenerationLevelDefinition);
			}
		}
	}
}
