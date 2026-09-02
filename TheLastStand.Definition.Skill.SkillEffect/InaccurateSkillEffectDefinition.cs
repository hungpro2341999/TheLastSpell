using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class InaccurateSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Inaccurate";
	}

	public Node malusValueExpression;

	public override string Id => "Inaccurate";

	public float Malus => GetMalus(null);

	public InaccurateSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetMalus(InterpreterContext context)
	{
		return malusValueExpression.EvalToFloat(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		malusValueExpression = Parser.Parse((container as XElement)?.Element("Malus")?.Value ?? "0", base.TokenVariables);
	}
}
