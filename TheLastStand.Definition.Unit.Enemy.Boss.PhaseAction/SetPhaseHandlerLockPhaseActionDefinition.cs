using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class SetPhaseHandlerLockPhaseActionDefinition : ABossPhaseActionDefinition
{
	public string HandlerId { get; private set; }

	public bool LockValue { get; private set; } = true;

	public SetPhaseHandlerLockPhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("HandlerId");
		HandlerId = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("LockValue");
		if (bool.TryParse(xAttribute2.Value, out var result))
		{
			LockValue = result;
		}
		else
		{
			CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
