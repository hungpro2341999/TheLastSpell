using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ContagionEffectDefinition : StatusEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Contagion";
	}

	public int Count { get; private set; } = 2;

	public override string Id => "Contagion";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Contagion;

	public ContagionEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		AffectedUnits = E_SkillUnitAffect.IgnoreCaster;
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("Count");
		if (xElement != null)
		{
			if (!int.TryParse(xElement.Value, out var result))
			{
				CLoggerManager.Log("Could not parse Contagion Count element value " + xElement.Value + " to a valid int value! Setting it to 2.", LogType.Error);
				Count = 2;
			}
			else
			{
				Count = result;
			}
		}
		else
		{
			Count = 2;
		}
	}
}
