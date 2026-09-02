using System;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedDeadUnit : ISerializedData
{
	public SerializedPlayableUnit Unit;

	public int DeathTurn;
}
