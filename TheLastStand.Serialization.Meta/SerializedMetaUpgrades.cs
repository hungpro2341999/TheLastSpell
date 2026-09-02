using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedMetaUpgrades : ISerializedData
{
	public List<SerializedMetaUpgrade> ActivatedUpgrades = new List<SerializedMetaUpgrade>();

	public List<SerializedMetaUpgrade> FullfilledUpgrades = new List<SerializedMetaUpgrade>();

	public List<SerializedMetaUpgrade> LockedUpgrades = new List<SerializedMetaUpgrade>();

	public List<SerializedMetaUpgrade> UnlockedUpgrades = new List<SerializedMetaUpgrade>();
}
