using System;
using System.Collections.Generic;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Serialization.SpawnWave;

[Serializable]
public class SerializedSpawnPointInfo : ISerializedData
{
	public int Direction;

	public List<List<SerializableVector2Int>> SpawnPointsPositions;

	public List<SerializableVector2Int> SpawnPointsBasesPositions;

	public SpawnDirectionsDefinition.SpawnDirectionInfo SpawnDirectionInfo;
}
