using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Status;

public class ContagionStatus : Status
{
	public int ContagionsCount { get; private set; } = 1;

	public override E_StatusType StatusType => E_StatusType.Contagion;

	public ContagionStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
		: base(statusController, unit, statusCreationInfo)
	{
		base.StatusEffectTime = E_StatusTime.Permanently;
		base.StatusDestructionTime = E_StatusTime.EndMyTurn;
		ContagionsCount = (int)statusCreationInfo.Value;
	}

	public override string GetStylizedStatus()
	{
		string text = "<style=Contagion>" + Localizer.Get("SkillEffectName_Contagion") + "</style>";
		return (base.RemainingTurnsCount == -1) ? (text + " (" + AtlasIcons.TimeIcon + " " + AtlasIcons.InfiniteIcon + ")") : $"{text} ({AtlasIcons.TimeIcon} <style=KeyWordNb>{base.RemainingTurnsCount}</style>)";
	}

	public override ISerializedData Serialize()
	{
		return new SerializedUnitStatus
		{
			StatusSourceInfo = ((base.Source is IEntity entity) ? new StatusSourceInfo(entity.RandomId, base.Source.GetDamageableType()) : null),
			RemainingTurns = base.RemainingTurnsCount,
			Type = StatusType,
			FromInjury = base.IsFromInjury,
			FromPerk = base.IsFromPerk,
			Value = ContagionsCount
		};
	}
}
