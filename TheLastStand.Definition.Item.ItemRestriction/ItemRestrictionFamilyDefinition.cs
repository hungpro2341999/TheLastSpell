namespace TheLastStand.Definition.Item.ItemRestriction;

public class ItemRestrictionFamilyDefinition
{
	public static class Constants
	{
		public const string ItemFamilyIdPrefix = "ItemList_";
	}

	public ItemDefinition.E_Category ItemCategory { get; }

	public string ItemsListId { get; }

	public string ShortId { get; private set; }

	public string LocalKey => "ItemFamily_" + ItemsListId;

	public ItemRestrictionFamilyDefinition(string itemsListId, ItemDefinition.E_Category itemCategory)
	{
		ItemsListId = itemsListId;
		ItemCategory = itemCategory;
		ShortId = itemsListId.Replace("ItemList_", string.Empty);
	}
}
