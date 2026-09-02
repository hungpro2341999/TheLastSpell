using System.Collections.Generic;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Item.ItemRestriction;
using TheLastStand.Database;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Item;
using TheLastStand.Serialization.Item.ItemRestriction;

namespace TheLastStand.Model.Item.ItemRestriction;

public class ItemRestrictionFamily : ISerializable, IDeserializable
{
	public static class Constants
	{
		public const string FamilyIconPathPrefix = "View/Sprites/UI/Meta/DarkShop/Icon_DarkShop_Unlock";
	}

	public class StringToItemRestrictionFamilyIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(ItemDatabase.ItemRestrictionFamiliesDefinitions.Keys);
	}

	public class StringToItemRestrictionFamilyShortIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				foreach (ItemRestrictionFamilyDefinition value in ItemDatabase.ItemRestrictionFamiliesDefinitions.Values)
				{
					list.Add(value.ShortId);
				}
				return list;
			}
		}
	}

	public string Id => ItemFamilyDefinition.ItemsListId;

	public bool IsActive
	{
		get
		{
			if (HasUnlockedItems)
			{
				return IsSelected;
			}
			return false;
		}
	}

	public bool HasUnlockedItems { get; set; }

	public bool IsSelected { get; set; } = true;

	public ItemRestrictionFamilyController ItemFamilyController { get; }

	public ItemRestrictionFamilyDefinition ItemFamilyDefinition { get; private set; }

	public HashSet<string> ItemsIds { get; set; } = new HashSet<string>();

	public string LocalizedName => Localizer.Get(ItemFamilyDefinition.LocalKey);

	public ItemRestrictionFamily(ItemRestrictionFamilyDefinition itemFamilyDefinition, ItemRestrictionFamilyController itemFamilyController)
	{
		ItemFamilyDefinition = itemFamilyDefinition;
		ItemFamilyController = itemFamilyController;
		InitItemsIds();
	}

	public ItemRestrictionFamily(SerializedItemRestrictionFamily serializedItemRestrictionFamily, ItemRestrictionFamilyDefinition itemFamilyDefinition, ItemRestrictionFamilyController itemFamilyController)
		: this(itemFamilyDefinition, itemFamilyController)
	{
		Deserialize(serializedItemRestrictionFamily);
	}

	public override string ToString()
	{
		return ItemFamilyDefinition.ItemsListId + "\n" + $"Category: {ItemFamilyDefinition.ItemCategory}\n" + $"IsActive: {IsActive}\n" + $"HasUnlockedItems: {HasUnlockedItems}\n" + $"IsSelected: {IsSelected}\n";
	}

	private void InitItemsIds()
	{
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(ItemFamilyDefinition.ItemsListId, out var value))
		{
			ItemsIds.Clear();
			ItemsIds = ItemManager.GetAllItemsInList(value);
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedItemRestrictionFamily serializedItemRestrictionFamily)
		{
			IsSelected = serializedItemRestrictionFamily.IsSelected;
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedItemRestrictionFamily
		{
			Id = ItemFamilyDefinition.ItemsListId,
			IsSelected = IsSelected
		};
	}
}
