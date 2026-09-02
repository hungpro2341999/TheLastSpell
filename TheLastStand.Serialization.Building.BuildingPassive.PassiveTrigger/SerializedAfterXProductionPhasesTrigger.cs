using System;

namespace TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

[Serializable]
public class SerializedAfterXProductionPhasesTrigger : ISerializedData
{
	public int ProductionPhasesSinceCreation;
}
