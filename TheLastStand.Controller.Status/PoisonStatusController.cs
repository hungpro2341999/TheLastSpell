using TPLib;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.Controller.Status;

public class PoisonStatusController : StatusController
{
	public PoisonStatus PoisonStatus => base.Status as PoisonStatus;

	public PoisonStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new PoisonStatus(this, unit, statusCreationInfo);
	}

	public override IDisplayableEffect ApplyStatus()
	{
		if (base.Status.Unit.UnitController.IsImmuneTo(base.Status))
		{
			return null;
		}
		AttackSkillActionExecutionTileData attackSkillActionExecutionTileData = new AttackSkillActionExecutionTileData
		{
			Damageable = base.Status.Unit,
			IsPoison = true,
			HealthDamage = Mathf.Min(base.Status.Unit.Health, PoisonStatus.DamagePerTurn),
			TotalDamage = PoisonStatus.DamagePerTurn,
			TargetTile = base.Status.Unit.OriginTile,
			TargetHealthTotal = base.Status.Unit.HealthTotal,
			TargetArmorTotal = base.Status.Unit.ArmorTotal,
			TargetRemainingArmor = base.Status.Unit.Armor,
			TargetRemainingHealth = Mathf.Max(0f, base.Status.Unit.Health - PoisonStatus.DamagePerTurn)
		};
		PerkDataContainer perkDataContainer = new PerkDataContainer
		{
			AttackData = attackSkillActionExecutionTileData,
			Caster = base.Status.Source,
			TargetDamageable = base.Status.Unit,
			TargetTile = base.Status.Unit.OriginTile
		};
		base.Status.Unit.Events.GetValueOrDefault(E_EffectTime.OnAttackDataComputed)?.Invoke(perkDataContainer);
		base.Status.Unit.DamageableController.OnAttackDataComputed(perkDataContainer);
		base.Status.Unit.UnitController.LoseHealth(attackSkillActionExecutionTileData.HealthDamage, PoisonStatus.Source, refreshHud: false, "PoisonStatus");
		TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnPoisonProc, perkDataContainer);
		if (base.Status.Unit.IsDeadOrDeathRattling)
		{
			TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnPoisonKill, perkDataContainer);
		}
		base.Status.Unit.DamageableView.AttackFeedback.AddDamageInstance(attackSkillActionExecutionTileData);
		if (base.Status.Unit is EnemyUnit enemyUnit && base.Status.Unit.IsDead)
		{
			TPSingleton<MetaConditionManager>.Instance.IncreaseEnemyKillsFromPoison(enemyUnit.EnemyUnitTemplateDefinition.Id);
		}
		return base.Status.Unit.DamageableView.AttackFeedback;
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		StyledKeyDisplay pooledComponent = ObjectPooler.GetPooledComponent("StyledKeyDisplay", ResourcePooler.LoadOnce<StyledKeyDisplay>("Prefab/Displayable Effect/UI Effect Displays/StyledKeyDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(base.Status.StatusType);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = base.Status.Source,
			TurnsCount = base.Status.RemainingTurnsCount,
			Value = PoisonStatus.DamagePerTurn,
			IsFromInjury = base.Status.IsFromInjury,
			IsFromPerk = base.Status.IsFromPerk,
			HideDisplayEffect = base.Status.HideDisplayEffect
		};
		return new PoisonStatusController(base.Status.Unit, statusCreationInfo);
	}

	public override void SetUnit(TheLastStand.Model.Unit.Unit unit)
	{
		base.SetUnit(unit);
		base.Status.Unit.UnitView.RefreshHealth();
	}

	protected override void MergeStatus(TheLastStand.Model.Status.Status otherStatus)
	{
		PoisonStatus.DamagePerTurn += ((PoisonStatus)otherStatus).DamagePerTurn;
	}
}
