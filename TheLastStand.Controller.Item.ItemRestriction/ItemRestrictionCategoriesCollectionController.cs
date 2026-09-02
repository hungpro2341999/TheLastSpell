using TPLib;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.Serialization.Item.ItemRestriction;

namespace TheLastStand.Controller.Item.ItemRestriction;

public class ItemRestrictionCategoriesCollectionController
{
	public ItemRestrictionCategoriesCollection ItemRestrictionCategoriesCollection { get; }

	public ItemRestrictionCategoriesCollectionController(ItemRestrictionCategoriesCollectionDefinition itemRestrictionCategoriesCollectionDefinition)
	{
		ItemRestrictionCategoriesCollection = new ItemRestrictionCategoriesCollection(itemRestrictionCategoriesCollectionDefinition, this);
	}

	public ItemRestrictionCategoriesCollectionController(SerializedItemRestrictionCategoriesCollection serializedItemRestrictionCategoriesCollection, ItemRestrictionCategoriesCollectionDefinition itemRestrictionFamilyDefinition)
	{
		ItemRestrictionCategoriesCollection = new ItemRestrictionCategoriesCollection(serializedItemRestrictionCategoriesCollection, itemRestrictionFamilyDefinition, this);
	}

	public void SelectAllItemsIfCollectionNotAvailable()
	{
		if (ItemRestrictionCategoriesCollection.IsAvailable)
		{
			return;
		}
		foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in ItemRestrictionCategoriesCollection.ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
		{
			if (ItemRestrictionCategoriesCollection.GetActiveFamiliesNb(itemCategoryDefinition.ItemCategory) == 0)
			{
				TPSingleton<ItemRestrictionManager>.Instance.SelectAllItemFamiliesFromCategory(itemCategoryDefinition.ItemCategory);
			}
		}
	}

	public void SetBoundlessModeActive(bool isActive)
	{
		ItemRestrictionCategoriesCollection.IsBoundlessModeActive = isActive;
	}
}
