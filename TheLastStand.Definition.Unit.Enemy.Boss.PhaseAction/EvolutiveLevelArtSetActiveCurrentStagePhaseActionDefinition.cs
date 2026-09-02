using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition : ABossPhaseActionDefinition
{
	public bool Value { get; private set; }

	public EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		if (bool.TryParse(xAttribute.Value, out var result))
		{
			Value = result;
		}
		else
		{
			CLoggerManager.Log("Unable to parse " + xAttribute.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
