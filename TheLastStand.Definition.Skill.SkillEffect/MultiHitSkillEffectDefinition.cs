using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class MultiHitSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "MultiHit";
	}

	private Node hitsCountValueExpression;

	public int HitsCount => GetHitsCount(null);

	public override string Id => "MultiHit";

	public MultiHitSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public int GetHitsCount(InterpreterContext context)
	{
		return hitsCountValueExpression.EvalToInt(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		hitsCountValueExpression = Parser.Parse((container as XElement).Attribute("HitsCount")?.Value ?? "1", base.TokenVariables);
	}
}
