using System;
using System.Collections.Generic;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedRecruitment : ISerializedData
{
	public List<SerializedPlayableUnit> RecruitableUnits = new List<SerializedPlayableUnit>();

	public float MageGenerationCurrentProbability;

	public int UnitLimitBonus;

	public bool HasMage;
}
