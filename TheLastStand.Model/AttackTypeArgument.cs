using System;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Model;

public class AttackTypeArgument : LocArgument
{
	public new static class Constants
	{
		public const string Id = "AttackTypeArgument";
	}

	public Node AttackTypeExpression { get; }

	public AttackSkillActionDefinition.E_AttackType? AttackType { get; }

	public AttackTypeArgument(string value, Node valueExpression, string style, string prefix, string suffix, Node attackTypeExpression, string attackType)
		: base(value, valueExpression, style, prefix, suffix)
	{
		AttackTypeExpression = attackTypeExpression;
		if (Enum.TryParse<AttackSkillActionDefinition.E_AttackType>(attackType, out var result))
		{
			AttackType = result;
		}
		else if (!string.IsNullOrEmpty(attackType))
		{
			CLoggerManager.Log("Could not parse " + attackType + " to a valid AttackType.", LogType.Error);
		}
	}

	public override string GetFinalValue(InterpreterContext interpreterContext)
	{
		AttackSkillActionDefinition.E_AttackType e_AttackType = AttackSkillActionDefinition.E_AttackType.None;
		if (AttackTypeExpression != null && Enum.TryParse<AttackSkillActionDefinition.E_AttackType>(AttackTypeExpression.Eval(interpreterContext).ToString(), out var result))
		{
			e_AttackType = result;
		}
		else if (AttackType.HasValue)
		{
			e_AttackType = AttackType.Value;
		}
		string text = Localizer.Get(string.Format("{0}{1}", "DamageTypeName_", e_AttackType));
		if (!string.IsNullOrEmpty(base.Style) && e_AttackType == AttackSkillActionDefinition.E_AttackType.None)
		{
			return "<style=" + base.Style + ">" + text + "</style>";
		}
		return e_AttackType.GetValueStylized(text);
	}
}
