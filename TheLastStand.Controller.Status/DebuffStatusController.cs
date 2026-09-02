using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Status;

public class DebuffStatusController : StatModifierStatusController
{
	public DebuffStatus DebuffStatus => base.Status as DebuffStatus;

	public DebuffStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new DebuffStatus(this, unit, statusCreationInfo);
		ComputeStatusDestructionTime();
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = base.Status.Source,
			TurnsCount = base.Status.RemainingTurnsCount,
			Stat = DebuffStatus.Stat,
			Value = DebuffStatus.ModifierValue,
			IsFromInjury = base.Status.IsFromInjury,
			IsFromPerk = base.Status.IsFromPerk,
			HideDisplayEffect = base.Status.HideDisplayEffect
		};
		return new DebuffStatusController(base.Status.Unit, statusCreationInfo);
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		DebuffDisplay pooledComponent = ObjectPooler.GetPooledComponent("DebuffDisplay", ResourcePooler.LoadOnce<DebuffDisplay>("Prefab/Displayable Effect/UI Effect Displays/DebuffDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(DebuffStatus.Stat, (int)DebuffStatus.ModifierValue);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}
}
