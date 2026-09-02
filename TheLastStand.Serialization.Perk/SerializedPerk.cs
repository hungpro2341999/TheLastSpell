using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Perk;

[Serializable]
public class SerializedPerk : ISerializedData
{
	public string Id;

	public bool Unlocked;

	public List<SerializedModule> Modules;

	public bool Bookmarked;
}
