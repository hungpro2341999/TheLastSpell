using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedNarration : ISerializedData
{
	public List<string> AlreadyUsedReplicasIds;

	public List<string> DialogueGreetingsIdsLeft;

	public int NextDialogueGreetingIndex;
}
