using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyPurgeAffixController : EnemyAffixController
{
	public EnemyPurgeAffix EnemyPurgeAffix => base.EnemyAffix as EnemyPurgeAffix;

	public EnemyPurgeAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyPurgeAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnEndNightTurnEnemy)
		{
			ApplyDispel();
		}
	}

	private void ApplyDispel()
	{
		if ((base.EnemyAffix.EnemyUnit.StatusOwned & EnemyPurgeAffix.EnemyPurgeAffixEffectDefinition.StatusType) != TheLastStand.Model.Status.Status.E_StatusType.None)
		{
			base.EnemyAffix.EnemyUnit.UnitController.RemoveStatus(EnemyPurgeAffix.EnemyPurgeAffixEffectDefinition.StatusType);
			if (EnemyPurgeAffix.EnemyPurgeAffixEffectDefinition.StatusType != TheLastStand.Model.Status.Status.E_StatusType.Charged)
			{
				DispelDisplay pooledComponent = ObjectPooler.GetPooledComponent("DispelDisplay", ResourcePooler.LoadOnce<DispelDisplay>("Prefab/Displayable Effect/UI Effect Displays/DispelDisplay"), EffectManager.EffectDisplaysParent);
				pooledComponent.Init(EnemyPurgeAffix.EnemyPurgeAffixEffectDefinition.StatusType);
				base.EnemyAffix.EnemyUnit.UnitController.AddEffectDisplay(pooledComponent);
				EffectManager.DisplayEffects();
			}
		}
	}
}
