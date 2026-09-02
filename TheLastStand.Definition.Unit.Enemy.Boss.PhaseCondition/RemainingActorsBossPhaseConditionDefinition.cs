using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;

public class RemainingActorsBossPhaseConditionDefinition : TheLastStand.Framework.Serialization.Definition, IBossPhaseConditionDefinition
{
	public string ActorId { get; private set; }

	public int Amount { get; private set; }

	public RemainingActorsBossPhaseConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("ActorId");
		if (xAttribute != null && xAttribute.Value != null)
		{
			ActorId = xAttribute.Value;
		}
		XAttribute xAttribute2 = obj.Attribute("Amount");
		if (xAttribute2 != null && xAttribute2.Value != null)
		{
			if (int.TryParse(xAttribute2.Value, out var result))
			{
				Amount = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into int", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}
