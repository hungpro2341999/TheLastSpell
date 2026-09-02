using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Status;

public class StunStatus : Status
{
	public override E_StatusType StatusType => E_StatusType.Stun;

	public StunStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
		: base(statusController, unit, statusCreationInfo)
	{
		base.StatusEffectTime = E_StatusTime.Permanently;
		base.StatusDestructionTime = E_StatusTime.EndMyTurn;
		if (base.Source is BattleModule battleModule && battleModule.BuildingParent.IsTrap)
		{
			base.RemainingTurnsCount = 2;
		}
	}

	public override string GetStylizedStatus()
	{
		string text = "<style=Stun>" + Localizer.Get("SkillEffectName_Stun") + "</style>";
		return text + " (" + AtlasIcons.TimeIcon + " " + ((base.RemainingTurnsCount == -1) ? AtlasIcons.InfiniteIcon : $"<style=KeyWordNb>{base.RemainingTurnsCount}</style>") + ")";
	}
}
