using System;
using System.Collections.Generic;
using TheLastStand.Definition.Meta;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedMetaShops : ISerializedData
{
	public List<string> MetaUpgradesAlreadySeen = new List<string>();

	public MetaUpgradeDefinition.E_MetaUpgradeFilter CurrentFilter = MetaUpgradeDefinition.E_MetaUpgradeFilter.Acquired | MetaUpgradeDefinition.E_MetaUpgradeFilter.Locked | MetaUpgradeDefinition.E_MetaUpgradeFilter.NotAcquiredYet;
}
