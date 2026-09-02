using System;

namespace TheLastStand.Serialization.Item.ItemRestriction;

[Serializable]
public class SerializedItemRestrictionCategoriesCollection : ISerializedData
{
	public string Id;

	public bool IsBoundlessActive;
}
