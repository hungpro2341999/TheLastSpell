using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Apocalypse;

[Serializable]
public class SerializedGlobalApocalypse : ISerializedData
{
	public int MaxAvailableApocalypseIndex;

	public List<string> ApocalypseModifiersUnlockSeen = new List<string>();

	public List<SerializedApocalypseModifierStep> SelectedModifierSteps;
}
