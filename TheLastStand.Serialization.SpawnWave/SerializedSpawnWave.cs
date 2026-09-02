using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.SpawnWave;

[Serializable]
public class SerializedSpawnWave : ISerializedData
{
	public string SpawnWaveDefinitionId;

	public bool IsPaused;

	public int CurrentCustomNightHour;

	public List<string> RemainingEliteEnemiesToSpawnIds;

	public List<string> RemainingEnemiesToSpawnIds;

	public int RotationsCount;

	public string SpawnDirectionsDefinitionId;

	public List<SerializedSpawnPointInfo> SpawnPointsInfo;

	public int SpawnsCount;

	public int UnableToSpawnCount;

	public float EnemyWeightModifierXpRatio = 1f;
}
