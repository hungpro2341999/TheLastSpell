using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class UpgradeStatBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public static class Constants
	{
		public const string GainHealthMax = "GainHealthMax";

		public const string GainManaMax = "GainManaMax";
	}

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public int Bonus { get; private set; }

	public E_BuildingActionTargeting BuildingActionTargeting { get; private set; }

	public override string ActionEstimationIconId => Stat switch
	{
		UnitStatDefinition.E_Stat.HealthTotal => "GainHealthMax", 
		UnitStatDefinition.E_Stat.ManaTotal => "GainManaMax", 
		UnitStatDefinition.E_Stat.MovePointsTotal => UnitStatDefinition.E_Stat.MovePoints.ToString(), 
		UnitStatDefinition.E_Stat.ActionPointsTotal => UnitStatDefinition.E_Stat.ActionPoints.ToString(), 
		_ => Stat.ToString(), 
	};

	public UpgradeStatBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container is XElement xElement)
		{
			XElement xElement2 = xElement.Element("Stat");
			if (xElement2 != null)
			{
				if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xElement2.Value, out var result))
				{
					CLoggerManager.Log("UpgradeStat Stat " + HasAnInvalid("E_Stat", xElement2.Value), LogType.Error);
					return;
				}
				Stat = result;
				XElement xElement3 = xElement.Element("Bonus");
				if (xElement3 != null)
				{
					if (!int.TryParse(xElement3.Value, out var result2))
					{
						CLoggerManager.Log("UpgradeStat Bonus " + HasAnInvalid("int", xElement3.Value), LogType.Error);
						return;
					}
					Bonus = result2;
					XElement xElement4 = xElement.Element("Target");
					if (xElement4 != null)
					{
						if (!Enum.TryParse<E_BuildingActionTargeting>(xElement4.Value, out var result3))
						{
							CLoggerManager.Log("UpgradeStat Target " + HasAnInvalid("E_Target", xElement4.Value), LogType.Error);
						}
						else
						{
							BuildingActionTargeting = result3;
						}
					}
					else
					{
						CLoggerManager.Log("UpgradeStat must have a Target", LogType.Error);
					}
				}
				else
				{
					CLoggerManager.Log("UpgradeStat must have a Bonus", LogType.Error);
				}
			}
			else
			{
				CLoggerManager.Log("UpgradeStat must have a stat", LogType.Error);
			}
		}
		else
		{
			CLoggerManager.Log("UpgradeStat doesn't have a XElement", LogType.Error);
		}
	}
}
