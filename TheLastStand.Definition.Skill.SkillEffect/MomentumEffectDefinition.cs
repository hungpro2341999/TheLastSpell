using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class MomentumEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const float MomentumCap = 4f;

		public const string Id = "Momentum";
	}

	private Node damageBonusPerTileValueExpression;

	public override string Id => "Momentum";

	public float DamageBonusPerTile => GetDamageBonusPerTile(null);

	public MomentumEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetDamageBonusPerTile(InterpreterContext context)
	{
		return damageBonusPerTileValueExpression.EvalToFloat(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		damageBonusPerTileValueExpression = Parser.Parse((container as XElement)?.Element("DamageBonusPerTile")?.Value ?? "0", base.TokenVariables);
	}
}
