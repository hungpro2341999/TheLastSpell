using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Status;

public class PoisonStatus : Status
{
	public float DamagePerTurn { get; set; }

	public override E_StatusType StatusType => E_StatusType.Poison;

	public PoisonStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
		: base(statusController, unit, statusCreationInfo)
	{
		DamagePerTurn = statusCreationInfo.Value;
		base.StatusEffectTime = E_StatusTime.StartMyTurn;
		base.StatusDestructionTime = E_StatusTime.StartMyTurn;
	}

	public override string GetStylizedStatus()
	{
		string text = "<style=Poison>" + Localizer.Get("SkillEffectName_Poison") + "</style>";
		return $"{text} <style=BadNb>({DamagePerTurn})</style> ({AtlasIcons.TimeIcon}{((base.RemainingTurnsCount == -1) ? AtlasIcons.InfiniteIcon : $"<style=KeyWordNb>{base.RemainingTurnsCount}</style>")})";
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
			Value = DamagePerTurn
		};
	}
}
