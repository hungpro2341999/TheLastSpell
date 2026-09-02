using System;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class GroundCategoryConditionDefinition : GoalConditionDefinition
{
	public const string Name = "GroundCategory";

	public GroundDefinition.E_GroundCategory GroundCategory { get; private set; }

	public GroundCategoryConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (Enum.TryParse<GroundDefinition.E_GroundCategory>((container as XElement).Attribute("Id")?.Value, out var result))
		{
			GroundCategory = result;
		}
		else
		{
			CLoggerManager.Log("Error while parsing GroundCategory", LogType.Error, CLogLevel.NORMAL, forcePrintInUnity: true, GetType().Name);
		}
	}
}
