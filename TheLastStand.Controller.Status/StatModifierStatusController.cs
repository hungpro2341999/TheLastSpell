using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Status;

namespace TheLastStand.Controller.Status;

public abstract class StatModifierStatusController : StatusController
{
	protected override bool CanBeMerged(TheLastStand.Model.Status.Status otherStatus)
	{
		if (base.CanBeMerged(otherStatus))
		{
			return ((StatModifierStatus)base.Status).Stat == ((StatModifierStatus)otherStatus).Stat;
		}
		return false;
	}

	protected void ComputeStatusDestructionTime()
	{
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits;
		base.Status.StatusDestructionTime = (flag ? TheLastStand.Model.Status.Status.E_StatusTime.StartPlayerTurn : TheLastStand.Model.Status.Status.E_StatusTime.StartEnemyTurn);
	}

	protected override void MergeStatus(TheLastStand.Model.Status.Status otherStatus)
	{
		((StatModifierStatus)base.Status).ModifierValue += ((StatModifierStatus)otherStatus).ModifierValue;
	}
}
