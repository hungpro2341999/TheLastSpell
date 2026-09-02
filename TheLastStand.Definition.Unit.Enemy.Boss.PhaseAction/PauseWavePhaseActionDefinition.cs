using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class PauseWavePhaseActionDefinition : ABossPhaseActionDefinition
{
	public bool PauseState { get; private set; }

	public PauseWavePhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (bool.TryParse((container as XElement).Attribute("State").Value, out var result))
		{
			PauseState = result;
		}
		else
		{
			CLoggerManager.Log("Invalid state value for PauseWave PhaseAction", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
