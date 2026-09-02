using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class GainMaterialsBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public int GainMaterials { get; private set; }

	public GainMaterialsBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		if (!xElement.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement.Value, out var result))
			{
				Debug.LogError("A GainMaterials Building ActionEffect must have a valid GainMaterials (int)");
			}
			else
			{
				GainMaterials = result;
			}
		}
	}
}
