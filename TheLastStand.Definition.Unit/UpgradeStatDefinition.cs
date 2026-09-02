using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UpgradeStatDefinition : TheLastStand.Framework.Serialization.Definition
{
	public UnitStatDefinition.E_Stat Stat { get; set; }

	public int Bonus { get; set; }

	public UpgradeStatDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
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
				}
				else
				{
					Bonus = result2;
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
}
