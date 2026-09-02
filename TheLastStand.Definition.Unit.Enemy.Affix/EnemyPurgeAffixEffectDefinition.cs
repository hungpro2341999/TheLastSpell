using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyPurgeAffixEffectDefinition : EnemyStatusAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Purge;

	public EnemyPurgeAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
