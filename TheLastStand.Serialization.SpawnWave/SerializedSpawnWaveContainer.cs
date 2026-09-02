using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.SpawnWave;

[Serializable]
public class SerializedSpawnWaveContainer : ISerializedData
{
	[Serializable]
	public class RerolledSpawnWave : ISerializedData
	{
		public List<string> EnemiesId = new List<string>();

		public string SpawnDirectionDefinitionId;

		public int RotationsCount;

		public string SpawnWaveDefinitionId;
	}

	public bool DisplayDangerIndicators;

	public RerolledSpawnWave LastRerolledSpawnWave;

	public SerializedSpawnWave CurrentSpawnWave;

	public string LastPlayedSpawnWaveId;
}
