using TPLib;
using TheLastStand.Database;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.Serialization.Item.ItemRestriction;

namespace TheLastStand.Controller.Item.ItemRestriction;

public class ItemRestrictionFamilyController
{
	public ItemRestrictionFamily ItemFamily { get; }

	public ItemRestrictionFamilyController(ItemRestrictionFamilyDefinition itemRestrictionFamilyDefinition)
	{
		ItemFamily = new ItemRestrictionFamily(itemRestrictionFamilyDefinition, this);
	}

	public ItemRestrictionFamilyController(SerializedItemRestrictionFamily serializedItemRestrictionFamily, ItemRestrictionFamilyDefinition itemRestrictionFamilyDefinition)
	{
		ItemFamily = new ItemRestrictionFamily(serializedItemRestrictionFamily, itemRestrictionFamilyDefinition, this);
	}

	public void ComputeHasUnlockedItems()
	{
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(ItemFamily.ItemFamilyDefinition.ItemsListId, out var value))
		{
			bool hasUnlockedItems = !ItemManager.IsItemsListContentLocked(value, TPSingleton<MetaUpgradesManager>.Instance.GetLockedItemsIds());
			ItemFamily.HasUnlockedItems = hasUnlockedItems;
		}
		else
		{
			ItemFamily.HasUnlockedItems = false;
		}
	}

	public void SetSelected(bool isSelected)
	{
		ItemFamily.IsSelected = isSelected;
	}
}
