using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class CasterEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "CasterEffect";
	}

	public override string Id => "CasterEffect";

	public override bool DisplayCompendiumEntry => true;

	public override bool ShouldBeDisplayed => false;

	public List<SkillEffectDefinition> SkillEffectDefinitions { get; set; } = new List<SkillEffectDefinition>();

	public CasterEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		foreach (XElement item2 in (container as XElement).Elements())
		{
			SkillEffectDefinition item = item2.Name.LocalName switch
			{
				"Buff" => new BuffEffectDefinition(item2, base.TokenVariables), 
				"Charged" => new ChargedEffectDefinition(item2), 
				"Contagion" => new ContagionEffectDefinition(item2), 
				"Damage" => new DamageSurroundingEffectDefinition(item2), 
				"Debuff" => new DebuffEffectDefinition(item2, base.TokenVariables), 
				"Poison" => new PoisonEffectDefinition(item2, base.TokenVariables), 
				"RemoveStatus" => new RemoveStatusEffectDefinition(item2), 
				"Stun" => new StunEffectDefinition(item2), 
				"RegenStat" => new RegenStatSkillEffectDefinition(item2, base.TokenVariables), 
				"DecreaseStat" => new DecreaseStatSkillEffectDefinition(item2, base.TokenVariables), 
				"NegativeStatusImmunityEffect" => new ImmuneToNegativeStatusEffectDefinition(item2), 
				_ => null, 
			};
			SkillEffectDefinitions.Add(item);
		}
		SkillEffectDefinitions.ForEach(delegate(SkillEffectDefinition ske)
		{
			if (ske is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition)
			{
				affectingUnitSkillEffectDefinition.AffectedUnits = E_SkillUnitAffect.Caster;
			}
		});
		AffectedUnits = E_SkillUnitAffect.Caster;
	}

	public List<TEffect> GetEffects<TEffect>() where TEffect : SkillEffectDefinition
	{
		List<TEffect> list = new List<TEffect>();
		for (int num = SkillEffectDefinitions.Count - 1; num >= 0; num--)
		{
			if (SkillEffectDefinitions[num] is TEffect)
			{
				list.Add(SkillEffectDefinitions[num] as TEffect);
			}
		}
		return list;
	}

	public bool HasEffect(string id)
	{
		for (int num = SkillEffectDefinitions.Count - 1; num >= 0; num--)
		{
			if (SkillEffectDefinitions[num].Id == id)
			{
				return true;
			}
		}
		return false;
	}
}
