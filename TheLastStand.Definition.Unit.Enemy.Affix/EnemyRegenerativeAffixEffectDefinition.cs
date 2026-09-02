using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyRegenerativeAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Regenerative;

	public Node HealthTotalPercentage { get; private set; }

	public EnemyRegenerativeAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = (container as XElement).Element("Percentage");
		HealthTotalPercentage = Parser.Parse(xElement.Value, base.TokenVariables);
	}
}
