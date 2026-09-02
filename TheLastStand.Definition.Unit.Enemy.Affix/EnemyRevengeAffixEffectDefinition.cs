using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillEffect;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyRevengeAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Revenge;

	public StatusEffectDefinition StatusEffectDefinition { get; private set; }

	public EnemyRevengeAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = (container as XElement).Elements().First();
		StatusEffectDefinition = xElement.Name.LocalName switch
		{
			"Debuff" => new DebuffEffectDefinition(xElement, base.TokenVariables), 
			"Stun" => new StunEffectDefinition(xElement, base.TokenVariables), 
			"Poison" => new PoisonEffectDefinition(xElement, base.TokenVariables), 
			_ => null, 
		};
	}
}
