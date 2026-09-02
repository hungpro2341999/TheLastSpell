using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Skill;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class SkillModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "SkillModifier";
	}

	public TheLastStand.Model.Skill.Skill.E_ComputationStat ComputationStat { get; private set; }

	public Node ValueExpression { get; private set; }

	public bool AffectBase { get; private set; }

	public SkillModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("ComputationStat");
		if (Enum.TryParse<TheLastStand.Model.Skill.Skill.E_ComputationStat>(xAttribute.Value, out var result))
		{
			ComputationStat = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse ComputationStat attribute into an E_ComputationStat : " + xAttribute.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SkillModifierEffectDefinition");
		}
		XAttribute xAttribute2 = obj.Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute2.Value, base.TokenVariables);
		XAttribute xAttribute3 = obj.Attribute("AffectBase");
		AffectBase = xAttribute3 != null && bool.Parse(xAttribute3.Value);
	}
}
