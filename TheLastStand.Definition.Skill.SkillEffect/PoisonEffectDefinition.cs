using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class PoisonEffectDefinition : StatusEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Poison";
	}

	private Node damagePerTurnValueExpression;

	public float DamagePerTurn => GetDamagePerTurn(null);

	public bool IgnoreDamageScale { get; private set; }

	public override string Id => "Poison";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Poison;

	public PoisonEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetDamagePerTurn(InterpreterContext context)
	{
		return damagePerTurnValueExpression.EvalToFloat(context);
	}

	public override void Deserialize(XContainer container)
	{
		AffectedUnits = E_SkillUnitAffect.IgnoreCaster;
		base.Deserialize(container);
		XElement xElement = container as XElement;
		damagePerTurnValueExpression = Parser.Parse(xElement?.Element("DamagePerTurn")?.Value ?? "0", base.TokenVariables);
		if (xElement?.Element("IgnorePoisonDamageScale") != null)
		{
			IgnoreDamageScale = true;
		}
	}
}
