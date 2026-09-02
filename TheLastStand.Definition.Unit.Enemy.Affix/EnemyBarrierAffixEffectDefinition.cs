using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyBarrierAffixEffectDefinition : EnemyStatusAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Barrier;

	public EnemyBarrierAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
