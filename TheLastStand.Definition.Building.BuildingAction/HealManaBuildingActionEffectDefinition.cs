using System;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class HealManaBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	private static class Constants
	{
		public const string ActionEstimationIconId = "Mana";
	}

	public int Amount { get; private set; }

	public E_BuildingActionTargeting BuildingActionTargeting { get; private set; }

	public override string ActionEstimationIconId => "Mana";

	public HealManaBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container is XElement xElement)
		{
			XElement xElement2 = xElement.Element("Amount");
			if (xElement2 != null)
			{
				if (!int.TryParse(xElement2.Value, out var result))
				{
					CLoggerManager.Log("HealMana Amount " + HasAnInvalid("int", xElement2.Value), LogType.Error);
					return;
				}
				Amount = result;
				XElement xElement3 = xElement.Element("Target");
				if (xElement3 != null)
				{
					if (!Enum.TryParse<E_BuildingActionTargeting>(xElement3.Value, out var result2))
					{
						CLoggerManager.Log("HealMana Target " + HasAnInvalid("E_Target", xElement3.Value), LogType.Error);
					}
					else
					{
						BuildingActionTargeting = result2;
					}
				}
			}
			else
			{
				CLoggerManager.Log("HealMana must have a Bonus", LogType.Error);
			}
		}
		else
		{
			CLoggerManager.Log("HealMana doesn't have a XElement", LogType.Error);
		}
	}
}
