using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Status.Immunity;

public class ImmunityStatus : Status
{
	public static class Constants
	{
		public const string ImmunitySuffix = "Immunity";
	}

	private E_StatusType statusType;

	public override string Name => Localizer.Get("StatusName_Immunity");

	public override E_StatusType StatusType => statusType;

	public override string StatusTypeString => "NegativeStatusImmunityEffect";

	public ImmunityStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo, E_StatusType immuneStatus)
		: base(statusController, unit, statusCreationInfo)
	{
		statusType = immuneStatus;
		base.StatusDestructionTime = E_StatusTime.StartMyTurn;
	}

	public override string GetStylizedStatus()
	{
		return $"<style={StatusType}>{Localizer.Get(Name)}</style>" + ((base.RemainingTurnsCount == -1) ? (" (" + AtlasIcons.TimeIcon + AtlasIcons.InfiniteIcon + ")") : $" ({AtlasIcons.TimeIcon}<style=KeyWordNb>{base.RemainingTurnsCount}</style>)");
	}
}
