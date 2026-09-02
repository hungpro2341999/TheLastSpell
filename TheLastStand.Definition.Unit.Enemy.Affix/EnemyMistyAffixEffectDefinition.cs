using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyMistyAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public bool CanLightFogExistOnSelf { get; private set; }

	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Misty;

	public Node Range { get; private set; }

	public EnemyMistyAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("CanLightFogExistOnSelf");
		CanLightFogExistOnSelf = xElement != null;
		XElement xElement2 = obj.Element("Range");
		Range = Parser.Parse(xElement2.Value, base.TokenVariables);
	}
}
