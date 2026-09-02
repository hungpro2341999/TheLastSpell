using TPLib;
using TheLastStand.Controller.Item.ItemRestriction;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Item;
using TheLastStand.Serialization.Item.ItemRestriction;

namespace TheLastStand.Model.Item.ItemRestriction;

public class ItemRestrictionCategoriesCollection : ISerializable, IDeserializable
{
	public string Id => ItemCategoriesCollectionDefinition.Id;

	public bool IsAvailable
	{
		get
		{
			foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
			{
				if (GetItemFamiliesNbWithUnlockedItems(itemCategoryDefinition.ItemFamiliesListId) >= itemCategoryDefinition.MinimumSelectedNb)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsBoundlessModeActive { get; set; }

	public ItemRestrictionCategoriesCollectionController ItemCategoriesCollectionController { get; }

	public ItemRestrictionCategoriesCollectionDefinition ItemCategoriesCollectionDefinition { get; private set; }

	public ItemRestrictionCategoriesCollection(ItemRestrictionCategoriesCollectionDefinition itemCategoriesCollectionDefinition, ItemRestrictionCategoriesCollectionController itemCategoriesCollectionController)
	{
		ItemCategoriesCollectionDefinition = itemCategoriesCollectionDefinition;
		ItemCategoriesCollectionController = itemCategoriesCollectionController;
	}

	public ItemRestrictionCategoriesCollection(SerializedItemRestrictionCategoriesCollection serializedItemRestrictionCategoriesCollection, ItemRestrictionCategoriesCollectionDefinition itemCategoriesCollectionDefinition, ItemRestrictionCategoriesCollectionController itemCategoriesCollectionController)
		: this(itemCategoriesCollectionDefinition, itemCategoriesCollectionController)
	{
		Deserialize(serializedItemRestrictionCategoriesCollection);
	}

	public static int GetItemFamiliesNbWithUnlockedItems(string itemFamiliesListId)
	{
		int num = 0;
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemFamiliesListId, out var value))
		{
			foreach (string key in value.ItemsWithOdd.Keys)
			{
				if (TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemsListId.TryGetValue(key, out var value2) && value2.HasUnlockedItems)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool AreAllCategoriesCorrectlyConfigured()
	{
		foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
		{
			if (!IsCategoryCorrectlyConfigured(itemCategoryDefinition.ItemCategory))
			{
				return false;
			}
		}
		return true;
	}

	public bool AreAllUnlockedFamiliesActiveFromCategory(ItemDefinition.E_Category itemCategory)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory.TryGetValue(itemCategory, out var value))
		{
			int num = 0;
			int num2 = 0;
			foreach (ItemRestrictionFamily item in value)
			{
				if (item.IsActive)
				{
					num2++;
				}
				if (item.HasUnlockedItems)
				{
					num++;
				}
			}
			return num == num2;
		}
		return false;
	}

	public bool AreAllUnlockedFamiliesSelected()
	{
		foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
		{
			if (!TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory.TryGetValue(itemCategoryDefinition.ItemCategory, out var value))
			{
				continue;
			}
			foreach (ItemRestrictionFamily item in value)
			{
				if (item.HasUnlockedItems && !item.IsSelected)
				{
					return false;
				}
			}
		}
		return true;
	}

	public int GetActiveFamiliesNb(ItemDefinition.E_Category itemCategory)
	{
		int num = 0;
		if (TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory.TryGetValue(itemCategory, out var value))
		{
			foreach (ItemRestrictionFamily item in value)
			{
				if (item.IsActive)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool GetCanUnSelectItemFamilyFromCategory(ItemDefinition.E_Category itemCategory)
	{
		return GetActiveFamiliesNb(itemCategory) > GetRequiredSelectedFamiliesNb(itemCategory);
	}

	public int GetRequiredSelectedFamiliesNb(ItemDefinition.E_Category itemCategory, ItemRestrictionCategoryDefinition categoryDefinition = null)
	{
		if (categoryDefinition == null)
		{
			categoryDefinition = ItemCategoriesCollectionDefinition.itemCategoryDefinitions.Find((ItemRestrictionCategoryDefinition aCategoryDefinition) => aCategoryDefinition.ItemCategory == itemCategory);
		}
		if (categoryDefinition == null)
		{
			return -1;
		}
		int num = categoryDefinition.MinimumSelectedNb;
		int unlockedFamiliesNb = GetUnlockedFamiliesNb(itemCategory);
		if (IsBoundlessModeActive)
		{
			num = categoryDefinition.BoundlessMinimumSelectedNb;
		}
		if (unlockedFamiliesNb < num)
		{
			num = unlockedFamiliesNb;
		}
		return num;
	}

	public bool IsBoundlessModeRequired()
	{
		if (!IsBoundlessModeActive)
		{
			return false;
		}
		foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
		{
			int activeFamiliesNb = GetActiveFamiliesNb(itemCategoryDefinition.ItemCategory);
			int unlockedFamiliesNb = GetUnlockedFamiliesNb(itemCategoryDefinition.ItemCategory);
			if (activeFamiliesNb < itemCategoryDefinition.MinimumSelectedNb && activeFamiliesNb < unlockedFamiliesNb)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCategoryCorrectlyConfigured(ItemDefinition.E_Category itemCategory)
	{
		ItemRestrictionCategoryDefinition itemRestrictionCategoryDefinition = ItemCategoriesCollectionDefinition.itemCategoryDefinitions.Find((ItemRestrictionCategoryDefinition aCategoryDefinition) => aCategoryDefinition.ItemCategory == itemCategory);
		if (itemRestrictionCategoryDefinition == null)
		{
			return false;
		}
		int requiredSelectedFamiliesNb = GetRequiredSelectedFamiliesNb(itemCategory, itemRestrictionCategoryDefinition);
		return GetActiveFamiliesNb(itemCategory) >= requiredSelectedFamiliesNb;
	}

	public override string ToString()
	{
		string text = "Id: " + ItemCategoriesCollectionDefinition.Id + "\n" + $"IsBoundlessModeActive: {IsBoundlessModeActive}\n" + $"IsBoundlessModeRequired: {IsBoundlessModeRequired()}\n" + $"AreAllCategoriesCorrectlyConfigured: {AreAllCategoriesCorrectlyConfigured()}\n";
		foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
		{
			text += $"IsCategoryCorrectlyConfigured[{itemCategoryDefinition.ItemCategory}]: {IsCategoryCorrectlyConfigured(itemCategoryDefinition.ItemCategory)}\n";
		}
		return text;
	}

	public int GetUnlockedFamiliesNb(ItemDefinition.E_Category itemCategory)
	{
		int num = 0;
		if (TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamiliesByItemCategory.TryGetValue(itemCategory, out var value))
		{
			foreach (ItemRestrictionFamily item in value)
			{
				if (item.HasUnlockedItems)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedItemRestrictionCategoriesCollection serializedItemRestrictionCategoriesCollection)
		{
			IsBoundlessModeActive = serializedItemRestrictionCategoriesCollection.IsBoundlessActive;
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedItemRestrictionCategoriesCollection
		{
			Id = ItemCategoriesCollectionDefinition.Id,
			IsBoundlessActive = IsBoundlessModeActive
		};
	}
}
