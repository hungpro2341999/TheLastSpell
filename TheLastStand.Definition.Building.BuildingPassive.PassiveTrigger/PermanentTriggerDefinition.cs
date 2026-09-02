using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class PermanentTriggerDefinition : PassiveTriggerDefinition
{
	public bool TriggerOnLoad { get; private set; }

	public PermanentTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.Permanent;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("TriggerOnLoad");
		if (bool.TryParse(xAttribute.Value, out var result))
		{
			TriggerOnLoad = result;
		}
		else
		{
			CLoggerManager.Log("PermanentTriggerDefinition : Unable to parse TriggerOnLoad value " + xAttribute?.Value + " into a bool.", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
