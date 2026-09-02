using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Trophy;

[Serializable]
public class SerializedEnemiesDamagedTrophy : ISerializedData
{
	public struct IntListPair
	{
		public int UnitId;

		public List<int> Values;
	}

	public List<IntListPair> ValuesPerUnitId;
}
