using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class ExcludeDamageableTypeInAoeConditionDefinition : GoalConditionDefinition
{
	public const string Name = "ExcludeDamageableTypeInAoe";

	public List<DamageableType> ExcludeDamageableTypes { get; set; } = new List<DamageableType>();

	public ExcludeDamageableTypeInAoeConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("DamageableType"))
		{
			if (Enum.TryParse<DamageableType>(item.Value, out var result))
			{
				ExcludeDamageableTypes.Add(result);
			}
			else
			{
				CLoggerManager.Log("GoalConditionDefinition ExcludeDamageableTypeInAoe UnitType is incorrect: " + item.Value + " is not a valid DamageableType", LogType.Error);
			}
		}
	}
}
