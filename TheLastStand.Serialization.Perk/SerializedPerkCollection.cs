using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Perk;

[Serializable]
public class SerializedPerkCollection : ISerializedData
{
	public string Id;

	public List<SerializedPerk> Perks;
}
