using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Skill.SkillAction;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyRegenerativeAffixController : EnemyAffixController
{
	public EnemyRegenerativeAffix EnemyRegenerativeAffix => base.EnemyAffix as EnemyRegenerativeAffix;

	public EnemyRegenerativeAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyRegenerativeAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnMovementEnd)
		{
			ApplyRegeneration();
		}
	}

	private void ApplyRegeneration()
	{
		float num = base.EnemyAffix.EnemyUnit.EnemyUnitController.GainHealth(EnemyRegenerativeAffix.RegenerationValue, refreshHud: false);
		if (num > 0f)
		{
			HealFeedback healFeedback = base.EnemyAffix.EnemyUnit.DamageableView.HealFeedback;
			healFeedback.AddHealInstance(num, base.EnemyAffix.EnemyUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).FinalClamped);
			base.EnemyAffix.EnemyUnit.UnitController.AddEffectDisplay(healFeedback);
			base.EnemyAffix.EnemyUnit.UnitController.DisplayEffects();
		}
	}
}
