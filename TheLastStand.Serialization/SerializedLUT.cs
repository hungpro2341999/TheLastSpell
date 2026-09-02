using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedLUT : ISerializedData
{
	public int DawnStartRemainingEnemies;

	public float TargetBlendAmount;
}
