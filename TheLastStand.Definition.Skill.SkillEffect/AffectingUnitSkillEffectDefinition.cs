using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Model.Extensions;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class AffectingUnitSkillEffectDefinition : SkillEffectDefinition
{
	[Flags]
	public enum E_SkillUnitAffect
	{
		None = 0,
		Caster = 1,
		PlayableUnit = 2,
		EnemyUnit = 4,
		BossUnit = 8,
		Building = 0x10,
		AllPlayable = 3,
		IgnoreCaster = 0x1E,
		All = 0x1F
	}

	public E_SkillUnitAffect AffectedUnits = E_SkillUnitAffect.All;

	public AffectingUnitSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		AffectedUnits.Deserialize(container);
	}
}
