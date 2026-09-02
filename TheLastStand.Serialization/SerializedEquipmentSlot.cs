using System;
using System.Collections.Generic;
using TheLastStand.Definition.Item;
using TheLastStand.Serialization.Item;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedEquipmentSlot : ISerializedData
{
	public ItemSlotDefinition.E_ItemSlotId Id;

	public List<SerializedItemSlot> ItemSlots = new List<SerializedItemSlot>();
}
