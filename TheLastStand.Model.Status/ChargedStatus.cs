using TheLastStand.Controller.Status;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Status;

public class ChargedStatus : Status
{
	public override E_StatusType StatusType => E_StatusType.Charged;

	public ChargedStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
		: base(statusController, unit, statusCreationInfo)
	{
		base.StatusEffectTime = E_StatusTime.Permanently;
		base.StatusDestructionTime = E_StatusTime.EndMyTurn;
	}

	public override string GetStylizedStatus()
	{
		return $"<style={StatusType}>{Name}</style>" + ((base.RemainingTurnsCount == -1) ? (" (" + AtlasIcons.TimeIcon + " " + AtlasIcons.InfiniteIcon + ")") : $" ({AtlasIcons.TimeIcon} <style=KeyWordNb>{base.RemainingTurnsCount}</style>)");
	}
}
