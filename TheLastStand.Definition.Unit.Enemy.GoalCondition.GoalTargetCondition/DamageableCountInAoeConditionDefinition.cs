using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class DamageableCountInAoeConditionDefinition : GoalConditionDefinition
{
	public const string Name = "DamageableCountInAoe";

	public List<DamageableType> DamageableTypesToCount = new List<DamageableType>();

	public int Max { get; private set; } = int.MaxValue;

	public int Min { get; private set; }

	public DamageableCountInAoeConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Min");
		if (xAttribute != null)
		{
			Min = int.Parse(xAttribute.Value);
		}
		XAttribute xAttribute2 = obj.Attribute("Max");
		if (xAttribute2 != null)
		{
			Max = int.Parse(xAttribute2.Value);
		}
		foreach (XElement item in obj.Elements("DamageableTypeToCount"))
		{
			if (Enum.TryParse<DamageableType>(item.Value, out var result))
			{
				DamageableTypesToCount.Add(result);
			}
			else
			{
				CLoggerManager.Log("GoalConditionDefinition DamageableCountInAoe UnitType is incorrect: " + item.Value + " is not a valid DamageableType", LogType.Error);
			}
		}
	}
}
