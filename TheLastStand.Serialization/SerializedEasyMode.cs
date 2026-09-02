using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedEasyMode : ISerializedData
{
	public bool Enabled;

	public bool HasSeenWarningPopup;

	public List<bool> ModifiersEnabled;
}
