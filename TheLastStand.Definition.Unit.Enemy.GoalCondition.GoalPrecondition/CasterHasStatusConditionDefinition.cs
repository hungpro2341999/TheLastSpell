using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class CasterHasStatusConditionDefinition : GoalConditionDefinition
{
	public const string Name = "CasterHasStatus";

	public Status.E_StatusType StatusType;

	public CasterHasStatusConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("StatusType");
		if (xAttribute != null)
		{
			if (Enum.TryParse<Status.E_StatusType>(xAttribute.Value, out var result))
			{
				StatusType = result;
			}
			else
			{
				CLoggerManager.Log("GoalConditionDefinition CasterHasStatus StatusType is incorrect: " + xAttribute.Value + " is not a valid StatusType", LogType.Error);
			}
		}
	}
}
