using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedResources : ISerializedData
{
	public int Gold;

	public bool HasSpentGold;

	public bool HasSpentMaterials;

	public int Materials;

	public int MaxWorkers;

	public int Workers;

	public int WorkersBuffer;

	public int ScavengeWorkersThisTurn;
}
