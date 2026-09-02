using System.Collections.Generic;
using System.Text;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyHealthChunksAffixController : EnemyAffixController
{
	public EnemyHealthChunksAffix EnemyHealthChunksAffix => base.EnemyAffix as EnemyHealthChunksAffix;

	public EnemyHealthChunksAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyHealthChunksAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public void InitSteps()
	{
		EnemyHealthChunksAffix.Steps = new List<int>();
		int num = Mathf.CeilToInt(base.EnemyAffix.EnemyUnit.HealthTotal * EnemyHealthChunksAffix.StepPercentage);
		int num2 = Mathf.RoundToInt(base.EnemyAffix.EnemyUnit.HealthTotal);
		int num3 = num2 / num;
		int num4 = num2;
		StringBuilder stringBuilder = new StringBuilder(base.EnemyAffix.EnemyUnit.UniqueIdentifier + " Steps: ");
		for (int i = 0; i < num3; i++)
		{
			num4 -= num;
			if (num4 <= 0)
			{
				break;
			}
			EnemyHealthChunksAffix.Steps.Add(num4);
			stringBuilder.Append($"{num4}");
			stringBuilder.Append(", ");
		}
		TPSingleton<EnemyUnitManager>.Instance.Log($"{stringBuilder}", CLogLevel.DETAILED);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnAttackDataComputed)
		{
			PerkDataContainer perkDataContainer = data as PerkDataContainer;
			CheckNextStep(perkDataContainer);
		}
	}

	private void CheckNextStep(PerkDataContainer perkDataContainer)
	{
		if (EnemyHealthChunksAffix.Steps == null)
		{
			InitSteps();
		}
		EnemyUnit enemyUnit = EnemyHealthChunksAffix.EnemyUnit;
		int num = 0;
		for (int i = 0; i < EnemyHealthChunksAffix.Steps.Count; i++)
		{
			num = EnemyHealthChunksAffix.Steps[i];
			if (enemyUnit.Health > (float)num)
			{
				break;
			}
		}
		if (!((float)num <= 0f) && enemyUnit.Health > (float)num && enemyUnit.Health - perkDataContainer.AttackData.HealthDamage <= (float)num)
		{
			TriggerNextStep(enemyUnit, num, perkDataContainer);
		}
	}

	private void TriggerNextStep(TheLastStand.Model.Unit.Unit targetUnit, int nextStepValue, PerkDataContainer perkDataContainer)
	{
		TPSingleton<EnemyUnitManager>.Instance.Log($"Trigger {base.EnemyAffix.EnemyUnit.UniqueIdentifier} HealthChunks step at {nextStepValue} health.", CLogLevel.DETAILED);
		float num = targetUnit.Health - (float)nextStepValue;
		float num2 = perkDataContainer.AttackData.HealthDamage - num;
		perkDataContainer.AttackData.HealthDamage = num;
		perkDataContainer.AttackData.TotalDamage -= num2;
		perkDataContainer.AttackData.TargetRemainingHealth = Mathf.Max(0f, targetUnit.Health - num);
		foreach (TheLastStand.Model.Status.Status.E_StatusType removeStatus in EnemyHealthChunksAffix.EnemyHealthChunksAffixEffectDefinition.RemoveStatuses)
		{
			targetUnit.UnitController.RemoveStatus(removeStatus);
			if (removeStatus != TheLastStand.Model.Status.Status.E_StatusType.Charged)
			{
				DispelDisplay pooledComponent = ObjectPooler.GetPooledComponent("DispelDisplay", ResourcePooler.LoadOnce<DispelDisplay>("Prefab/Displayable Effect/UI Effect Displays/DispelDisplay"), EffectManager.EffectDisplaysParent);
				pooledComponent.Init(removeStatus);
				targetUnit.UnitController.AddEffectDisplay(pooledComponent);
				EffectManager.DisplayEffects();
			}
		}
		foreach (KeyValuePair<TheLastStand.Model.Status.Status.E_StatusType, int> item in EnemyHealthChunksAffix.EnemyHealthChunksAffixEffectDefinition.ApplyStatusesWithDuration)
		{
			if (item.Key != TheLastStand.Model.Status.Status.E_StatusType.Invulnerable)
			{
				ApplyStatus(targetUnit, item.Key, item.Value);
			}
		}
		if (EnemyHealthChunksAffix.EnemyHealthChunksAffixEffectDefinition.ApplyStatusesWithDuration.ContainsKey(TheLastStand.Model.Status.Status.E_StatusType.Invulnerable))
		{
			ApplyStatus(targetUnit, TheLastStand.Model.Status.Status.E_StatusType.Invulnerable, EnemyHealthChunksAffix.EnemyHealthChunksAffixEffectDefinition.ApplyStatusesWithDuration[TheLastStand.Model.Status.Status.E_StatusType.Invulnerable]);
		}
	}

	private void ApplyStatus(TheLastStand.Model.Unit.Unit targetUnit, TheLastStand.Model.Status.Status.E_StatusType statusType, int statusDuration)
	{
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = base.EnemyAffix.EnemyUnit,
			TurnsCount = statusDuration
		};
		SkillManager.AddStatus(targetUnit, statusType, statusCreationInfo);
	}
}
