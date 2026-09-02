using System;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Status;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedUnitStatus : ISerializedData
{
	public StatusSourceInfo StatusSourceInfo;

	public Status.E_StatusType Type;

	public int RemainingTurns;

	public float Value;

	public UnitStatDefinition.E_Stat Stat = UnitStatDefinition.E_Stat.Undefined;

	public bool FromInjury;

	public bool FromPerk;
}
