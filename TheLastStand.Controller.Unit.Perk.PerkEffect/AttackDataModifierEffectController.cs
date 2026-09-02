using System;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class AttackDataModifierEffectController : APerkEffectController
{
	public AttackDataModifierEffect AttackDataModifierEffect => base.PerkEffect as AttackDataModifierEffect;

	public AttackDataModifierEffectController(AttackDataModifierEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new AttackDataModifierEffect(aPerkEffectDefinition as AttackDataModifierEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		ModifyAttackDataParameter(data.AttackData);
	}

	private void ModifyAttackDataParameter(AttackSkillActionExecutionTileData attackData)
	{
		switch (AttackDataModifierEffect.AttackDataModifierEffectDefinition.AttackDataParameter)
		{
		case AttackSkillActionExecutionTileData.E_AttackDataParameter.ArmorDamage:
		{
			float armorDamage = attackData.ArmorDamage;
			attackData.ArmorDamage = AttackDataModifierEffect.AttackDataModifierEffectDefinition.ValueExpression.EvalToFloat(base.PerkEffect.APerkModule.Perk);
			attackData.TargetRemainingArmor = Mathf.Max(0f, attackData.TargetRemainingArmor + (armorDamage - attackData.ArmorDamage));
			attackData.TotalDamage = attackData.HealthDamage + attackData.ArmorDamage;
			break;
		}
		case AttackSkillActionExecutionTileData.E_AttackDataParameter.Dodged:
			attackData.Dodged = AttackDataModifierEffect.AttackDataModifierEffectDefinition.ValueExpression.EvalToBool(base.PerkEffect.APerkModule.Perk);
			break;
		case AttackSkillActionExecutionTileData.E_AttackDataParameter.HealthDamage:
		{
			float healthDamage = attackData.HealthDamage;
			attackData.HealthDamage = AttackDataModifierEffect.AttackDataModifierEffectDefinition.ValueExpression.EvalToFloat(base.PerkEffect.APerkModule.Perk);
			attackData.TargetRemainingHealth = Mathf.Max(0f, attackData.TargetRemainingHealth + (healthDamage - attackData.HealthDamage));
			attackData.TotalDamage = attackData.HealthDamage + attackData.ArmorDamage;
			break;
		}
		case AttackSkillActionExecutionTileData.E_AttackDataParameter.IsCrit:
			attackData.IsCrit = AttackDataModifierEffect.AttackDataModifierEffectDefinition.ValueExpression.EvalToBool(base.PerkEffect.APerkModule.Perk);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
