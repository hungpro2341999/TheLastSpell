using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Trophy;

[Serializable]
public class SerializedValueIntHeroesTrophy : ASerializedTrophy
{
	public struct IntPair
	{
		public int UnitId;

		public int Value;
	}

	public List<IntPair> ValuePerUnitId;
}
