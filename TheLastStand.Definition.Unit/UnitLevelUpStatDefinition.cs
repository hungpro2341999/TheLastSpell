using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitLevelUpStatDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<UnitLevelUp.E_StatLevelUpRarity, int> Bonuses { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public float Weight { get; private set; }

	public UnitLevelUpStatDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("Stat");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("UnitLevelUpStat must have a Stat");
			return;
		}
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
		{
			Debug.LogError("UnitLevelUpStat must have a valid Stat");
			return;
		}
		Stat = result;
		XAttribute xAttribute2 = xElement.Attribute("Weight");
		if (xAttribute2.IsNullOrEmpty())
		{
			TPDebug.LogError("UnitLevelUpStat must have a Weight");
			return;
		}
		if (!float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			TPDebug.LogError("UnitLevelUpStat Weight must be a valid float");
			return;
		}
		Weight = result2;
		XElement xElement2 = xElement.Element("Bonuses");
		if (xElement2.IsNullOrEmpty())
		{
			TPDebug.LogError("UnitLevelUpStat must have a Bonuses");
			return;
		}
		Bonuses = new Dictionary<UnitLevelUp.E_StatLevelUpRarity, int>();
		XElement xElement3 = xElement2.Element("BigBonus");
		XElement xElement4 = xElement2.Element("MediumBonus");
		XElement xElement5 = xElement2.Element("SmallBonus");
		if (xElement3 != null)
		{
			if (!int.TryParse(xElement3.Value, out var result3))
			{
				TPDebug.LogError("UnitLevelUpStat BigBonus must be a valid integer");
				return;
			}
			Bonuses.Add(UnitLevelUp.E_StatLevelUpRarity.BigRarity, result3);
		}
		if (xElement4 != null)
		{
			if (!int.TryParse(xElement4.Value, out var result4))
			{
				TPDebug.LogError("UnitLevelUpStat MediumBonus must be a valid integer");
				return;
			}
			Bonuses.Add(UnitLevelUp.E_StatLevelUpRarity.MediumRarity, result4);
		}
		if (xElement5 != null)
		{
			if (!int.TryParse(xElement5.Value, out var result5))
			{
				TPDebug.LogError("UnitLevelUpStat SmallBonus must be a valid integer");
			}
			else
			{
				Bonuses.Add(UnitLevelUp.E_StatLevelUpRarity.SmallRarity, result5);
			}
		}
	}
}
