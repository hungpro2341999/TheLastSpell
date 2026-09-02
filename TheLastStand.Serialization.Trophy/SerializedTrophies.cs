using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Trophy;

[Serializable]
public class SerializedTrophies : ISerializedData
{
	public int RawDamnedSoulsEarnedThisNight;

	public List<SerializedValueIntTrophy> ValueIntTrophies;

	public List<SerializedValueIntHeroesTrophy> ValueIntHeroesTrophies;

	public SerializedEnemiesDamagedTrophy EnemiesDamagedTrophy;

	public SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy EnemiesDebuffedSeveralTimesSingleTurnTrophy;
}
