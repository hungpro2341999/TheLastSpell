using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedNarrations : ISerializedData
{
	public SerializedNarration DarkNarration;

	public SerializedNarration LightNarration;

	public bool NarrationDoneThisDay;
}
