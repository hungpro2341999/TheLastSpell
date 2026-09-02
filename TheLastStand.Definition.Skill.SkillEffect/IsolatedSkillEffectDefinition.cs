using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class IsolatedSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Isolated";
	}

	private Node damageMultiplierValueExpression;

	public override string Id => "Isolated";

	public IsolatedSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetDamageMultiplier(InterpreterContext context)
	{
		return damageMultiplierValueExpression.EvalToFloat(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		damageMultiplierValueExpression = Parser.Parse((container as XElement)?.Element("DamageMultiplier")?.Value ?? "1", base.TokenVariables);
	}
}
