using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class SetNightProgressionPhaseActionDefinition : ABossPhaseActionDefinition
{
	public float Value { get; private set; }

	public SetNightProgressionPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		if (float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Value = result;
		}
		else
		{
			CLoggerManager.Log("SetNightProgressionPhaseActionDefinition Could not parse Value attribute into a valid float : " + xAttribute.Value, LogType.Error, CLogLevel.MAJOR);
		}
	}
}
