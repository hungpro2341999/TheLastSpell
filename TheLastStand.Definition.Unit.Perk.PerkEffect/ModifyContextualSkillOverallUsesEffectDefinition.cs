using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ModifyContextualSkillOverallUsesEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ModifyContextualSkillOverallUses";
	}

	private Node overallUsesExpression;

	public string ContextualSkillId { get; private set; }

	public bool RefillOverallUses { get; private set; }

	public ModifyContextualSkillOverallUsesEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		ContextualSkillId = obj.Attribute("ContextualSkillId")?.Value.Replace(base.TokenVariables);
		XAttribute xAttribute = obj.Attribute("OverallUses");
		overallUsesExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
		XAttribute xAttribute2 = obj.Attribute("RefillOverallUses");
		if (xAttribute2 != null)
		{
			if (!bool.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse ModifyContextualSkillOverallUsesEffectDefinition RefillOverallUses value to a valid bool.");
			}
			RefillOverallUses = result;
		}
	}

	public int GetOverallUses(InterpreterContext context)
	{
		return overallUsesExpression.EvalToInt(context);
	}
}
