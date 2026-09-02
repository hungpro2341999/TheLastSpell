using System;
using System.Collections.Generic;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveEffect;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Serialization.Building;

[Serializable]
public class SerializedBuildingPassive : ISerializedData
{
	public string Id;

	public List<SerializedAfterXProductionPhasesTrigger> AfterXProductionPhasesTriggers;

	public List<SerializedAfterXNightEndTrigger> AfterXNightEndTriggers;

	public List<SerializedAfterXNightTurnsTrigger> AfterXNightTurnsTriggers;

	public List<SerializedGenerateLightFog> GenerateLightFogEffects;
}
