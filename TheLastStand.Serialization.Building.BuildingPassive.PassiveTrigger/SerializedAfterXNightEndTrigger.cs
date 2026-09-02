using System;

namespace TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

[Serializable]
public class SerializedAfterXNightEndTrigger : ISerializedData
{
	public int NightEndSinceCreation;
}
