using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class RepelFogBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public int Amount { get; private set; }

	public RepelFogBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = (xContainer as XElement).Element("Amount");
		int result;
		if (xElement.IsNullOrEmpty())
		{
			Debug.LogError("A RepelFog Building ActionEffect must have an Amount element");
		}
		else if (!int.TryParse(xElement.Value, out result))
		{
			Debug.LogError("A RepelFog Building ActionEffect must have a valid Amount (int)");
		}
		else
		{
			Amount = result;
		}
	}
}
