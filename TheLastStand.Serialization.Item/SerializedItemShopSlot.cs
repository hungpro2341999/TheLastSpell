using System;

namespace TheLastStand.Serialization.Item;

[Serializable]
public class SerializedItemShopSlot : ISerializedData
{
	public SerializedItem Item;

	public bool IsSoldOut;
}
