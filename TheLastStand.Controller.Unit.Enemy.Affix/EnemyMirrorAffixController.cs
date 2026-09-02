using TPLib;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyMirrorAffixController : EnemyAffixController
{
	public EnemyMirrorAffix EnemyMirrorAffix => base.EnemyAffix as EnemyMirrorAffix;

	public EnemyMirrorAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyMirrorAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnHitTaken)
		{
			ISkillCaster attacker = data as ISkillCaster;
			ReturnDamage(attacker);
		}
	}

	private void ReturnDamage(ISkillCaster attacker)
	{
		if (!(attacker is PlayableUnit playableUnit))
		{
			return;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, 100);
		float finalClamped = playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Dodge).FinalClamped;
		AttackSkillActionExecutionTileData attackSkillActionExecutionTileData = null;
		if ((float)randomRange >= finalClamped)
		{
			float num = EnemyMirrorAffix.ComputeDamage();
			num = playableUnit.PlayableUnitController.ReduceIncomingDamage((int)num, base.EnemyAffix.EnemyUnit, AttackSkillActionDefinition.E_AttackType.None, checkBlock: true, out var blockedDamage, updateLifetimeStats: true);
			num = Mathf.Max(num, 0f);
			float armor = playableUnit.Armor;
			float num2 = Mathf.Min(armor, num);
			if (num > 0f)
			{
				playableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesReceivedByEnemy(base.EnemyAffix.EnemyUnit.Id, num);
			}
			if (num2 > 0f)
			{
				playableUnit.DamageableController.LoseArmor(num2, base.EnemyAffix.EnemyUnit, refreshHud: false);
			}
			num = Mathf.Max(0f, num - armor);
			if (num > 0f)
			{
				playableUnit.DamageableController.LoseHealth(num, base.EnemyAffix.EnemyUnit, refreshHud: false, "EnemyMirrorAffix");
				playableUnit.UnitController.UpdateInjuryStage();
			}
			attackSkillActionExecutionTileData = new AttackSkillActionExecutionTileData
			{
				Damageable = playableUnit,
				TargetTile = attacker.OriginTile,
				ArmorDamage = num2,
				TargetRemainingArmor = playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Armor).FinalClamped,
				TargetArmorTotal = playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ArmorTotal).FinalClamped,
				TargetRemainingHealth = playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).FinalClamped,
				TargetHealthTotal = playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthTotal).FinalClamped,
				HealthDamage = num,
				TotalDamage = num + num2,
				BlockedDamage = blockedDamage
			};
		}
		else
		{
			attackSkillActionExecutionTileData = new AttackSkillActionExecutionTileData
			{
				Damageable = playableUnit,
				TargetTile = attacker.OriginTile,
				Dodged = true
			};
		}
		if (attackSkillActionExecutionTileData != null)
		{
			AttackFeedback attackFeedback = playableUnit.DamageableView.AttackFeedback;
			attackFeedback.AddDamageInstance(attackSkillActionExecutionTileData);
			playableUnit.DamageableController.AddEffectDisplay(attackFeedback);
			EffectManager.Register(playableUnit.DamageableController);
		}
	}
}
