using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction;

namespace TheLastStand.Controller.Status;

public abstract class StatusController
{
	public TheLastStand.Model.Status.Status Status { get; protected set; }

	public virtual IDisplayableEffect ApplyStatus()
	{
		return null;
	}

	public abstract StatusController Clone();

	public virtual bool CreateEffectDisplay(IDamageableController damageableController)
	{
		return false;
	}

	public virtual void SetUnit(TheLastStand.Model.Unit.Unit unit)
	{
		Status.Unit = unit;
	}

	public bool TryMergeStatus(TheLastStand.Model.Status.Status otherStatus)
	{
		if (!CanBeMerged(otherStatus))
		{
			return false;
		}
		MergeStatus(otherStatus);
		return true;
	}

	protected virtual bool CanBeMerged(TheLastStand.Model.Status.Status otherStatus)
	{
		if (Status.GetType() == otherStatus.GetType() && Status.RemainingTurnsCount == otherStatus.RemainingTurnsCount)
		{
			return Status.IsFromInjury == otherStatus.IsFromInjury;
		}
		return false;
	}

	protected abstract void MergeStatus(TheLastStand.Model.Status.Status otherStatus);
}
