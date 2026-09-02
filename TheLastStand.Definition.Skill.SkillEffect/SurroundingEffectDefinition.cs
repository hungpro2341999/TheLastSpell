using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;
using TheLastStand.Model.Extensions;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class SurroundingEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "SurroundingEffect";
	}

	public List<SkillEffectDefinition> SkillEffectDefinitions { get; set; } = new List<SkillEffectDefinition>();

	public override string Id => "SurroundingEffect";

	public SurroundingEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
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
				"Stun" => new StunEffectDefinition(item2, base.TokenVariables), 
				"NegativeStatusImmunityEffect" => new ImmuneToNegativeStatusEffectDefinition(item2, base.TokenVariables), 
				_ => null, 
			};
			SkillEffectDefinitions.Add(item);
		}
		bool num = SkillEffectDefinitions.Any((SkillEffectDefinition skillEffect) => skillEffect is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && affectingUnitSkillEffectDefinition.AffectedUnits.AffectsUnitType(E_SkillUnitAffect.Caster));
		bool flag = SkillEffectDefinitions.Any((SkillEffectDefinition skillEffect) => skillEffect is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && affectingUnitSkillEffectDefinition.AffectedUnits.AffectsUnitType(E_SkillUnitAffect.EnemyUnit));
		bool flag2 = SkillEffectDefinitions.Any((SkillEffectDefinition skillEffect) => skillEffect is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && affectingUnitSkillEffectDefinition.AffectedUnits.AffectsUnitType(E_SkillUnitAffect.PlayableUnit));
		bool flag3 = SkillEffectDefinitions.Any((SkillEffectDefinition skillEffect) => skillEffect is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && affectingUnitSkillEffectDefinition.AffectedUnits.AffectsUnitType(E_SkillUnitAffect.BossUnit));
		AffectedUnits = E_SkillUnitAffect.None;
		if (num)
		{
			AffectedUnits |= E_SkillUnitAffect.Caster;
		}
		if (flag)
		{
			AffectedUnits |= E_SkillUnitAffect.EnemyUnit;
		}
		if (flag2)
		{
			AffectedUnits |= E_SkillUnitAffect.PlayableUnit;
		}
		if (flag3)
		{
			AffectedUnits |= E_SkillUnitAffect.BossUnit;
		}
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

	public bool HasEffect<TEffect>() where TEffect : SkillEffectDefinition
	{
		for (int num = SkillEffectDefinitions.Count - 1; num >= 0; num--)
		{
			if (SkillEffectDefinitions[num] is TEffect)
			{
				return true;
			}
		}
		return false;
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
