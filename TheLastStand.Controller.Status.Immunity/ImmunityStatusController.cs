using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Status.Immunity;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.Controller.Status.Immunity;

public class ImmunityStatusController : StatusController
{
	private ImmunityStatus ImmunityStatus => base.Status as ImmunityStatus;

	public ImmunityStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo, TheLastStand.Model.Status.Status.E_StatusType immune)
	{
		base.Status = new ImmunityStatus(this, unit, statusCreationInfo, immune);
	}

	protected override bool CanBeMerged(TheLastStand.Model.Status.Status otherStatus)
	{
		if (base.Status.GetType() == otherStatus.GetType())
		{
			return base.Status.StatusType == otherStatus.StatusType;
		}
		return false;
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
			IsFromInjury = base.Status.IsFromInjury,
			IsFromPerk = base.Status.IsFromPerk,
			HideDisplayEffect = base.Status.HideDisplayEffect
		};
		return new ImmunityStatusController(base.Status.Unit, statusCreationInfo, ImmunityStatus.StatusType);
	}

	protected override void MergeStatus(TheLastStand.Model.Status.Status otherStatus)
	{
		if (base.Status.RemainingTurnsCount == -1 || otherStatus.RemainingTurnsCount == -1)
		{
			base.Status.RemainingTurnsCount = -1;
		}
		else
		{
			base.Status.RemainingTurnsCount = Mathf.Max(base.Status.RemainingTurnsCount, otherStatus.RemainingTurnsCount);
		}
	}
}
