using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class GainGoldBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public int GainGold { get; private set; }

	public GainGoldBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
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
				Debug.LogError("A GainGold Building ActionEffect must have a valid GainGold (int)");
			}
			else
			{
				GainGold = result;
			}
		}
	}
}
