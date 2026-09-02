using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Trophy;

[Serializable]
public class SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy : ISerializedData
{
	public struct EnemiesDebuffedDictionary
	{
		public int Turn;

		public List<UnitDebuffDictionary> Values;
	}

	public struct UnitDebuffDictionary
	{
		public int UnitId;

		public List<SerializedValueIntHeroesTrophy.IntPair> Values;
	}

	public List<EnemiesDebuffedDictionary> EnemiesDebuffedDictionaries;
}
