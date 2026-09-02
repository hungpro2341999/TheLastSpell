using System;
using System.Collections.Generic;
using TheLastStand.Serialization.Item;

namespace TheLastStand.Serialization.Building;

[Serializable]
public class SerializedShop : ISerializedData
{
	public List<SerializedItemShopSlot> ItemSlots = new List<SerializedItemShopSlot>();

	public int RerollIndex;
}
