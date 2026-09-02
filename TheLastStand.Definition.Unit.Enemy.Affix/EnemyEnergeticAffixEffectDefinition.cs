using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyEnergeticAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Energetic;

	public EnemyEnergeticAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
