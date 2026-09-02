using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class RestoreStatEffectController : APerkEffectController
{
	public RestoreStatEffect RestoreStatEffect => base.PerkEffect as RestoreStatEffect;

	public RestoreStatEffectController(RestoreStatEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new RestoreStatEffect(aPerkEffectDefinition as RestoreStatEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		TPSingleton<PlayableUnitManager>.Instance.ShouldClearUndoStack = true;
		float num = RestoreStatEffect.RestoreStatEffectDefinition.ValueExpression.EvalToFloat(base.PerkEffect.APerkModule.Perk);
		if (!(num <= 0f))
		{
			PlayableUnit owner = base.PerkEffect.APerkModule.Perk.Owner;
			UnitStatDefinition.E_Stat stat = RestoreStatEffect.RestoreStatEffectDefinition.Stat;
			float num2 = 0f;
			switch (stat)
			{
			case UnitStatDefinition.E_Stat.Health:
				num2 = owner.PlayableUnitController.GainHealth(num, refreshHud: false);
				owner.PlayableUnitController.UpdateInjuryStage();
				break;
			case UnitStatDefinition.E_Stat.Mana:
				num2 = owner.PlayableUnitController.GainMana(num);
				break;
			case UnitStatDefinition.E_Stat.Armor:
			case UnitStatDefinition.E_Stat.ActionPoints:
			case UnitStatDefinition.E_Stat.MovePoints:
				num2 = owner.PlayableUnitStatsController.IncreaseBaseStat(stat, num, includeChildStat: false);
				break;
			default:
				base.PerkEffect.APerkModule.Perk.Owner.LogError($"Perk {base.PerkEffect.APerkModule.Perk.PerkDefinition.Id} tried to restore {num} {RestoreStatEffect.RestoreStatEffectDefinition.Stat}. This is currently not a supported behaviour.");
				break;
			}
			if (num2 > 0f && !RestoreStatEffect.RestoreStatEffectDefinition.HideDisplayEffect)
			{
				DisplayRestoreStatEffect(owner, stat, num2);
			}
		}
	}

	private void DisplayRestoreStatEffect(PlayableUnit playableUnit, UnitStatDefinition.E_Stat stat, float valueRestored)
	{
		if (!(valueRestored <= 0f))
		{
			switch (stat)
			{
			case UnitStatDefinition.E_Stat.Health:
			{
				HealFeedback healFeedback = playableUnit.DamageableView.HealFeedback;
				healFeedback.AddHealInstance(valueRestored, playableUnit.Health);
				playableUnit.DamageableController.AddEffectDisplay(healFeedback);
				playableUnit.UnitController.DisplayEffects();
				break;
			}
			case UnitStatDefinition.E_Stat.Armor:
			case UnitStatDefinition.E_Stat.Mana:
			case UnitStatDefinition.E_Stat.ActionPoints:
			case UnitStatDefinition.E_Stat.MovePoints:
			{
				RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("RestoreStatDisplay", ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay"), EffectManager.EffectDisplaysParent);
				pooledComponent.Init(stat, (int)valueRestored);
				playableUnit.UnitController.AddEffectDisplay(pooledComponent);
				playableUnit.UnitController.DisplayEffects();
				break;
			}
			}
		}
	}
}
