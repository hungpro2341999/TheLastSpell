using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Hazard;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TileHasHazardConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TileHasHazard";

	public HazardDefinition.E_HazardType HazardType { get; private set; }

	public TileHasHazardConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("HazardType");
		if (Enum.TryParse<HazardDefinition.E_HazardType>(xAttribute?.Value, out var result))
		{
			HazardType = result;
		}
		else
		{
			CLoggerManager.Log("Error while parsing HazardType : " + xAttribute?.Value + " is not a correct HazardType", LogType.Error, CLogLevel.NORMAL, forcePrintInUnity: true, GetType().Name);
		}
	}
}
