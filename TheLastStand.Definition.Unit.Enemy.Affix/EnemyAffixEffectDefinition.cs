using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public abstract class EnemyAffixEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_EnemyAffixEffect
	{
		Reinforced,
		Aura,
		Mirror,
		Misty,
		Energetic,
		Regenerative,
		Invincible,
		Revenge,
		Purge,
		Barrier,
		HigherPlane,
		HealthChunks
	}

	public enum E_EnemyAffixBoxType
	{
		Base,
		Elite,
		FireLink,
		FireLinkBoss,
		Freude,
		Schaden,
		Boss
	}

	public abstract E_EnemyAffixEffect EnemyAffixEffect { get; }

	protected EnemyAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
