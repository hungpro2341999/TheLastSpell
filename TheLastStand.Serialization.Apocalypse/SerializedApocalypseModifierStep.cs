using System;

namespace TheLastStand.Serialization.Apocalypse;

[Serializable]
public class SerializedApocalypseModifierStep : ISerializedData
{
	public string ModifierId;

	public int StepIndex;
}
