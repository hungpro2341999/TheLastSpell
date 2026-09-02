using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class EvolutiveLevelArtSetStagePhaseActionDefinition : ABossPhaseActionDefinition
{
	public int StageIndex { get; private set; }

	public EvolutiveLevelArtSetStagePhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		if (int.TryParse(xAttribute.Value, out var result))
		{
			StageIndex = result;
		}
		else
		{
			CLoggerManager.Log("EvolutiveLevelArtSetStagePhaseActionDefinition Could not parse Value attribute into a valid int : " + xAttribute.Value, LogType.Error, CLogLevel.MAJOR);
		}
	}
}
