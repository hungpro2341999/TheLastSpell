using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class DamageableAroundConditionDefinition : GoalConditionDefinition
{
	public const string Name = "DamageableAround";

	public int MinAmount { get; private set; } = 1;

	public DamageableType DamageableType { get; private set; }

	public int MaxRange { get; private set; } = 1;

	public int MinRange { get; private set; }

	public DamageableAroundConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("MinAmount");
		if (xAttribute != null)
		{
			MinAmount = int.Parse(xAttribute.Value);
		}
		XAttribute xAttribute2 = obj.Attribute("DamageableType");
		if (Enum.TryParse<DamageableType>(xAttribute2.Value, out var result))
		{
			DamageableType = result;
		}
		else
		{
			CLoggerManager.Log("GoalConditionDefinition DamageableAround UnitType is incorrect: " + xAttribute2.Value + " is not a valid DamageableType", LogType.Error);
		}
		XAttribute xAttribute3 = obj.Attribute("MinRange");
		if (xAttribute3 != null)
		{
			MinRange = int.Parse(xAttribute3.Value);
		}
		XAttribute xAttribute4 = obj.Attribute("MaxRange");
		if (xAttribute4 != null)
		{
			MaxRange = int.Parse(xAttribute4.Value);
		}
	}
}
