using System;
using TheLastStand.Definition.Item;

namespace TheLastStand.Serialization.Item;

[Serializable]
public class SerializedItemSlot : ISerializedData
{
	public ItemSlotDefinition.E_ItemSlotId Id;

	public SerializedItem Item;
}
