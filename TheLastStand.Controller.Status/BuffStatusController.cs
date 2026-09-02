using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Status;

public class BuffStatusController : StatModifierStatusController
{
	public BuffStatus BuffStatus => base.Status as BuffStatus;

	public BuffStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new BuffStatus(this, unit, statusCreationInfo);
		ComputeStatusDestructionTime();
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = base.Status.Source,
			TurnsCount = base.Status.RemainingTurnsCount,
			Stat = BuffStatus.Stat,
			Value = BuffStatus.ModifierValue,
			IsFromInjury = base.Status.IsFromInjury,
			IsFromPerk = base.Status.IsFromPerk,
			HideDisplayEffect = base.Status.HideDisplayEffect
		};
		return new BuffStatusController(base.Status.Unit, statusCreationInfo);
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		BuffDisplay pooledComponent = ObjectPooler.GetPooledComponent("BuffDisplay", ResourcePooler.LoadOnce<BuffDisplay>("Prefab/Displayable Effect/UI Effect Displays/BuffDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(BuffStatus.Stat, (int)BuffStatus.ModifierValue);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}
}
