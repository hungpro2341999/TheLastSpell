using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Item.ItemRestriction;

[Serializable]
public class SerializedItemRestrictions : ISerializedData
{
	public List<SerializedItemRestrictionFamily> ItemFamilies = new List<SerializedItemRestrictionFamily>();

	public SerializedItemRestrictionCategoriesCollection WeaponsCategoriesCollection;
}
