using TheLastStand.Controller.Status;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Status;

public abstract class StatModifierStatus : Status
{
	public float ModifierValue { get; set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public StatModifierStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
		: base(statusController, unit, statusCreationInfo)
	{
		Stat = statusCreationInfo.Stat;
		ModifierValue = statusCreationInfo.Value;
		base.StatusEffectTime = E_StatusTime.Permanently;
		base.StatusDestructionTime = E_StatusTime.StartMyTurn;
	}

	public override ISerializedData Serialize()
	{
		return new SerializedUnitStatus
		{
			RemainingTurns = base.RemainingTurnsCount,
			Type = StatusType,
			Value = ModifierValue,
			Stat = Stat,
			FromInjury = base.IsFromInjury
		};
	}
}
