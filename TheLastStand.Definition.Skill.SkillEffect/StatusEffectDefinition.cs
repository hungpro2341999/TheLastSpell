using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class StatusEffectDefinition : AffectingUnitSkillEffectDefinition
{
	private Node baseChanceValueExpression;

	private Node turnsCountValueExpression;

	public float BaseChance => GetBaseChance(null);

	public abstract Status.E_StatusType StatusType { get; }

	public int TurnsCount => GetTurnsCount(null);

	public StatusEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetBaseChance(InterpreterContext context)
	{
		return baseChanceValueExpression.EvalToFloat(context);
	}

	public int GetTurnsCount(InterpreterContext context)
	{
		return turnsCountValueExpression.EvalToInt(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		baseChanceValueExpression = Parser.Parse(xElement.Element("BaseChance")?.Value ?? "1", base.TokenVariables);
		turnsCountValueExpression = Parser.Parse(xElement.Element("TurnsCount")?.Value ?? "1", base.TokenVariables);
	}
}
