using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class UnlockContextualSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "UnlockContextualSkill";
	}

	private Node overallUsesExpression;

	public string ContextualSkillId { get; private set; }

	public UnlockContextualSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("ContextualSkillId");
		ContextualSkillId = xAttribute.Value;
		overallUsesExpression = Parser.Parse(obj.Attribute("OverallUses")?.Value ?? "-1", base.TokenVariables);
	}

	public int GetOverallUses(InterpreterContext context)
	{
		return overallUsesExpression.EvalToInt(context);
	}
}
