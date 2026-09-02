using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Item.ItemRestriction;

public class ItemRestrictionCategoriesCollectionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<ItemRestrictionCategoryDefinition> itemCategoryDefinitions { get; private set; }

	public string Id { get; set; }

	public ItemRestrictionCategoriesCollectionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		itemCategoryDefinitions = new List<ItemRestrictionCategoryDefinition>();
		foreach (XElement item2 in obj.Elements("ItemRestrictionCategoryDefinition"))
		{
			ItemRestrictionCategoryDefinition item = new ItemRestrictionCategoryDefinition(item2);
			itemCategoryDefinitions.Add(item);
		}
	}
}
