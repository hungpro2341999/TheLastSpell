using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedMagicCicleSettings : ISerializedData
{
	public string Id;

	public int MageSlots;

	public int MageCount;

	public int ClosedSealsCount;

	public int OpenedSealsCount;
}
