using System;

namespace TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

[Serializable]
public class SerializedAfterXNightTurnsTrigger : ISerializedData
{
	public int NightTurnsSinceCreation;
}
