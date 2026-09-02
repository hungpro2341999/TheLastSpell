using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Status;

public class InvulnerableStatus : Status
{
	public override E_StatusType StatusType => E_StatusType.Invulnerable;

	public InvulnerableStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
		: base(statusController, unit, statusCreationInfo)
	{
		base.StatusEffectTime = E_StatusTime.Permanently;
		base.StatusDestructionTime = E_StatusTime.EndMyTurn;
	}

	public override string GetStylizedStatus()
	{
		string text = "<style=Invulnerable>" + Localizer.Get("SkillEffectName_Invulnerable") + "</style>";
		return text + " (" + AtlasIcons.TimeIcon + " " + ((base.RemainingTurnsCount == -1) ? AtlasIcons.InfiniteIcon : $"<style=KeyWordNb>{base.RemainingTurnsCount}</style>") + ")";
	}
}
