using System;

namespace TheLastStand.Serialization.Item.ItemRestriction;

[Serializable]
public class SerializedItemRestrictionFamily : ISerializedData
{
	public string Id;

	public bool IsSelected;
}
