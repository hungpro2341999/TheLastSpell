using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class PropagationSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Propagation";
	}

	private Node propagationCountValueExpression;

	public override string Id => "Propagation";

	public int PropagationsCount => GetPropagationsCount(null);

	public PropagationSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public int GetPropagationsCount(InterpreterContext context)
	{
		return propagationCountValueExpression.EvalToInt(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		propagationCountValueExpression = Parser.Parse((container as XElement).Attribute("PropagationsCount")?.Value ?? "1", base.TokenVariables);
	}
}
