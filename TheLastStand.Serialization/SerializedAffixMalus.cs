using System;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedAffixMalus : ISerializedData
{
	public AffixMalusDefinition.E_MalusLevel MalusLevel;

	public UnitStatDefinition.E_Stat Stat;
}
