using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class AfterXNightEndTriggerDefinition : PassiveTriggerDefinition
{
	public int NumberOfNightEnd { get; private set; }

	public AfterXNightEndTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnNightEnd;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("NumberOfNightEnd");
		if (xAttribute == null || !int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("The NumberOfNightEnd attribute (in AfterXNightEndTrigger) is missing or could not be parsed into an int.", LogType.Error, CLogLevel.MAJOR);
		}
		else
		{
			NumberOfNightEnd = result;
		}
	}
}
