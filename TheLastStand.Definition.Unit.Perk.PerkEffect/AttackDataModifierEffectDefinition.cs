using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AttackDataModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AttackDataModifier";
	}

	public AttackSkillActionExecutionTileData.E_AttackDataParameter AttackDataParameter { get; private set; }

	public Node ValueExpression { get; private set; }

	public AttackDataModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		if (Enum.TryParse<AttackSkillActionExecutionTileData.E_AttackDataParameter>(obj.Attribute("AttackDataParameter").Value, out var result))
		{
			AttackDataParameter = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse AttackDataParameter attribute into an E_AttackDataParameter");
		}
		XAttribute xAttribute = obj.Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
	}
}
