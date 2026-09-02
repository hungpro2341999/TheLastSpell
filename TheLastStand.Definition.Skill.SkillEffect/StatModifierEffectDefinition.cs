using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class StatModifierEffectDefinition : StatusEffectDefinition
{
	public Node ModifierValueExpression { get; protected set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	protected StatModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetModifierValue(InterpreterContext context = null)
	{
		return ModifierValueExpression.EvalToFloat(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (Enum.TryParse<UnitStatDefinition.E_Stat>((container as XElement).Element("Stat").Attribute("Id").Value, out var result))
		{
			Stat = result;
		}
		else
		{
			Debug.LogError($"Unknown stat {result}");
		}
	}
}
