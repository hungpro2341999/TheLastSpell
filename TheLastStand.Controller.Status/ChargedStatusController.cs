using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Status;

public class ChargedStatusController : StatusController
{
	public ChargedStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new ChargedStatus(this, unit, statusCreationInfo);
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		StyledKeyDisplay pooledComponent = ObjectPooler.GetPooledComponent("StyledKeyDisplay", ResourcePooler.LoadOnce<StyledKeyDisplay>("Prefab/Displayable Effect/UI Effect Displays/StyledKeyDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(base.Status.StatusType);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}

	protected override bool CanBeMerged(TheLastStand.Model.Status.Status otherStatus)
	{
		return false;
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = base.Status.Source,
			TurnsCount = base.Status.RemainingTurnsCount,
			IsFromInjury = base.Status.IsFromInjury,
			IsFromPerk = base.Status.IsFromPerk,
			HideDisplayEffect = base.Status.HideDisplayEffect
		};
		return new ChargedStatusController(base.Status.Unit, statusCreationInfo);
	}

	protected override void MergeStatus(TheLastStand.Model.Status.Status otherStatus)
	{
	}
}
