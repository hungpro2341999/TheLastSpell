using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightTurnsTriggerDefinition : PassiveTriggerDefinition
{
	public int NumberOfNightTurns { get; private set; }

	public AfterXNightTurnsTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnEndNightTurnPlayable;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("NumberOfNightTurns");
		if (xAttribute == null || !int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("The NumberOfNightTurns attribute (in AfterXNightTurnsTrigger) is missing or could not be parsed into an int.", LogType.Error, CLogLevel.MAJOR);
		}
		else
		{
			NumberOfNightTurns = result;
		}
	}
}
