using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item.ItemRestriction;

public class ItemRestrictionCategoryDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int BoundlessMinimumSelectedNb { get; private set; }

	public ItemDefinition.E_Category ItemCategory { get; private set; }

	public string ItemFamiliesListId { get; private set; }

	public int MinimumSelectedNb { get; private set; }

	public ItemRestrictionCategoryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("ItemFamiliesListId");
		if (xElement2.IsNullOrEmpty())
		{
			CLoggerManager.Log("An ItemRestrictionCategoryDefinition doesn't have a list of items Id !", LogType.Error);
			return;
		}
		ItemFamiliesListId = xElement2.Value;
		XElement xElement3 = xElement.Element("BoundlessMinimumSelectedNb");
		if (xElement3 != null)
		{
			if (!int.TryParse(xElement3.Value, out var result))
			{
				CLoggerManager.Log("The ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + " BoundlessMinimumSelectedNb " + HasAnInvalidInt(xElement3.Value), LogType.Error);
				return;
			}
			BoundlessMinimumSelectedNb = result;
		}
		XAttribute xAttribute = xElement.Attribute("ItemCategory");
		if (xAttribute != null)
		{
			if (!Enum.TryParse<ItemDefinition.E_Category>(xAttribute.Value, out var result2))
			{
				CLoggerManager.Log("ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + ", ItemCategory " + HasAnInvalid("E_Category", xAttribute.Value), LogType.Error);
				return;
			}
			ItemCategory = result2;
			if (ItemDatabase.ItemsListDefinitions.TryGetValue(ItemFamiliesListId, out var value))
			{
				foreach (KeyValuePair<string, int> item in value.ItemsWithOdd)
				{
					if (ItemDatabase.ItemsListDefinitions.TryGetValue(item.Key, out var value2))
					{
						ItemRestrictionFamilyDefinition itemRestrictionFamilyDefinition = new ItemRestrictionFamilyDefinition(value2.Id, ItemCategory);
						ItemDatabase.ItemRestrictionFamiliesDefinitions.Add(itemRestrictionFamilyDefinition.ItemsListId, itemRestrictionFamilyDefinition);
					}
				}
			}
			XElement xElement4 = xElement.Element("MinimumSelectedNb");
			if (xElement4 != null)
			{
				if (!int.TryParse(xElement4.Value, out var result3))
				{
					CLoggerManager.Log("The ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + " MinimumSelectedNb " + HasAnInvalidInt(xElement4.Value), LogType.Error);
				}
				else
				{
					MinimumSelectedNb = result3;
				}
			}
		}
		else
		{
			CLoggerManager.Log("ItemRestrictionCategoryDefinition with list " + ItemFamiliesListId + " must have a Category", LogType.Error);
		}
	}
}
