using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Item.ItemRestriction;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.Serialization.Item.ItemRestriction;
using UnityEngine;

namespace TheLastStand.Manager.Item;

public sealed class ItemRestrictionManager : Manager<ItemRestrictionManager>, ISerializable, IDeserializable
{
	public static class Constants
	{
		public const string WeaponsCategoriesCollectionId = "WeaponsRestrictions";
	}

	private bool initialized;

	public List<ItemRestrictionFamily> ItemRestrictionFamilies { get; private set; }

	public Dictionary<string, ItemRestrictionFamily> ItemRestrictionFamiliesByItemsListId { get; private set; }

	public Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamily>> ItemRestrictionFamiliesByItemCategory { get; private set; }

	public ItemRestrictionCategoriesCollection WeaponsRestrictionsCategories { get; private set; }

	public static void RefreshItemFamiliesLockedItemsFromCategory(ItemDefinition.E_Category itemCategory, bool isUnlocked)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null || !TPSingleton<ItemRestrictionManager>.Instance.initialized)
		{
			return;
		}
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.ItemFamilyDefinition.ItemCategory.HasFlag(itemCategory) && itemRestrictionFamily.HasUnlockedItems != isUnlocked)
			{
				itemRestrictionFamily.ItemFamilyController.ComputeHasUnlockedItems();
			}
		}
	}

	public HashSet<string> GetLockedItemsIds()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			if (!itemRestrictionFamily.IsActive)
			{
				hashSet.UnionWith(itemRestrictionFamily.ItemsIds);
			}
		}
		return hashSet;
	}

	public bool TryChangeItemFamilySelected(bool isSelected, string itemFamilyId)
	{
		if (ItemRestrictionFamiliesByItemsListId.TryGetValue(itemFamilyId, out var value))
		{
			value.ItemFamilyController.SetSelected(isSelected);
			return true;
		}
		return false;
	}

	public void SelectAllItemFamiliesFromCategory(ItemDefinition.E_Category itemCategory)
	{
		if (!ItemRestrictionFamiliesByItemCategory.TryGetValue(itemCategory, out var value))
		{
			return;
		}
		foreach (ItemRestrictionFamily item in value)
		{
			item.ItemFamilyController.SetSelected(isSelected: true);
		}
	}

	public string[] GetActiveWeaponFamiliesForAnalytics()
	{
		List<string> list = new List<string>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.IsActive && (ItemDefinition.E_Category.Weapon & itemRestrictionFamily.ItemFamilyDefinition.ItemCategory) != ItemDefinition.E_Category.None)
			{
				string item = itemRestrictionFamily.ItemFamilyDefinition.ItemCategory.ToString() + "_" + itemRestrictionFamily.ItemFamilyDefinition.ShortId;
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public string[] GetExcludedWeaponFamiliesForAnalytics()
	{
		List<string> list = new List<string>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.HasUnlockedItems && !itemRestrictionFamily.IsSelected && (ItemDefinition.E_Category.Weapon & itemRestrictionFamily.ItemFamilyDefinition.ItemCategory) != ItemDefinition.E_Category.None)
			{
				string item = itemRestrictionFamily.ItemFamilyDefinition.ItemCategory.ToString() + "_" + itemRestrictionFamily.ItemFamilyDefinition.ShortId;
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		ItemRestrictionFamiliesByItemsListId = new Dictionary<string, ItemRestrictionFamily>();
		ItemRestrictionFamiliesByItemCategory = new Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamily>>();
		Dictionary<string, SerializedItemRestrictionFamily> dictionary = new Dictionary<string, SerializedItemRestrictionFamily>();
		SerializedItemRestrictions serializedItemRestrictions = null;
		if (container is SerializedItemRestrictions serializedItemRestrictions2)
		{
			serializedItemRestrictions = serializedItemRestrictions2;
			if (serializedItemRestrictions2.ItemFamilies != null && serializedItemRestrictions2.ItemFamilies.Count > 0)
			{
				foreach (SerializedItemRestrictionFamily itemFamily in serializedItemRestrictions2.ItemFamilies)
				{
					dictionary.Add(itemFamily.Id, itemFamily);
				}
			}
		}
		ItemRestrictionFamilies = new List<ItemRestrictionFamily>();
		foreach (ItemRestrictionFamilyDefinition itemRestrictionFamilyDefinition in ItemDatabase.ItemRestrictionFamiliesDefinitions.Values)
		{
			ItemRestrictionFamily itemRestrictionFamily = null;
			itemRestrictionFamily = ((!dictionary.TryGetValue(itemRestrictionFamilyDefinition.ItemsListId, out var value)) ? new ItemRestrictionFamilyController(itemRestrictionFamilyDefinition).ItemFamily : new ItemRestrictionFamilyController(value, itemRestrictionFamilyDefinition).ItemFamily);
			if (ItemRestrictionFamilies.Any((ItemRestrictionFamily itemFamily) => itemFamily.ItemFamilyDefinition.ItemsListId == itemRestrictionFamilyDefinition.ItemsListId))
			{
				LogError("Tried to add an ItemRestrictionFamily with an ItemListsIds already present !", CLogLevel.DETAILED);
				continue;
			}
			itemRestrictionFamily.ItemFamilyController.ComputeHasUnlockedItems();
			ItemRestrictionFamilies.Add(itemRestrictionFamily);
			if (!ItemRestrictionFamiliesByItemsListId.ContainsKey(itemRestrictionFamilyDefinition.ItemsListId))
			{
				ItemRestrictionFamiliesByItemsListId.Add(itemRestrictionFamilyDefinition.ItemsListId, itemRestrictionFamily);
			}
			if (!ItemRestrictionFamiliesByItemCategory.ContainsKey(itemRestrictionFamilyDefinition.ItemCategory))
			{
				ItemRestrictionFamiliesByItemCategory.Add(itemRestrictionFamilyDefinition.ItemCategory, new List<ItemRestrictionFamily> { itemRestrictionFamily });
			}
			else
			{
				ItemRestrictionFamiliesByItemCategory[itemRestrictionFamilyDefinition.ItemCategory].Add(itemRestrictionFamily);
			}
		}
		foreach (ItemRestrictionCategoriesCollectionDefinition value2 in ItemDatabase.ItemRestrictionCategoriesCollectionDefinitions.Values)
		{
			string id = value2.Id;
			if (id != null && id == "WeaponsRestrictions")
			{
				if (serializedItemRestrictions != null && serializedItemRestrictions.WeaponsCategoriesCollection != null)
				{
					WeaponsRestrictionsCategories = new ItemRestrictionCategoriesCollectionController(serializedItemRestrictions.WeaponsCategoriesCollection, value2).ItemRestrictionCategoriesCollection;
				}
				else
				{
					WeaponsRestrictionsCategories = new ItemRestrictionCategoriesCollectionController(value2).ItemRestrictionCategoriesCollection;
				}
				WeaponsRestrictionsCategories.ItemCategoriesCollectionController.SelectAllItemsIfCollectionNotAvailable();
			}
		}
		initialized = true;
	}

	public ISerializedData Serialize()
	{
		return new SerializedItemRestrictions
		{
			ItemFamilies = ItemRestrictionFamilies.Select((ItemRestrictionFamily family) => family.Serialize() as SerializedItemRestrictionFamily).ToList(),
			WeaponsCategoriesCollection = (WeaponsRestrictionsCategories.Serialize() as SerializedItemRestrictionCategoriesCollection)
		};
	}

	[DevConsoleCommand("ItemRestrictionDisplayFamily")]
	public static void DebugItemRestrictionDisplayFamily([StringConverter(typeof(ItemRestrictionFamily.StringToItemRestrictionFamilyShortIdConverter))] string itemRestrictionFamilyShortId)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		ItemRestrictionFamily itemRestrictionFamily = TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies.Find((ItemRestrictionFamily anItemFamily) => anItemFamily.ItemFamilyDefinition.ShortId == itemRestrictionFamilyShortId);
		if (itemRestrictionFamily == null)
		{
			TPDebug.LogError("Couldn't find an ItemRestrictionFamily with short id: " + itemRestrictionFamilyShortId);
		}
		else
		{
			TPSingleton<DebugManager>.Instance.LogDevConsole(itemRestrictionFamily);
		}
	}

	[DevConsoleCommand("ItemRestrictionSetFamilySelected")]
	public static void DebugItemRestrictionSetFamilySelected([StringConverter(typeof(ItemRestrictionFamily.StringToItemRestrictionFamilyShortIdConverter))] string itemRestrictionFamilyShortId, bool selected = true)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		ItemRestrictionFamily itemRestrictionFamily = TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies.Find((ItemRestrictionFamily anItemFamily) => anItemFamily.ItemFamilyDefinition.ShortId == itemRestrictionFamilyShortId);
		if (itemRestrictionFamily == null)
		{
			TPDebug.LogError("Couldn't find an ItemRestrictionFamily with short id: " + itemRestrictionFamilyShortId);
		}
		else
		{
			itemRestrictionFamily.ItemFamilyController.SetSelected(selected);
		}
	}

	[DevConsoleCommand("ItemRestrictionSetAllFamiliesSelected")]
	public static void DebugItemRestrictionSetAllFamiliesSelected(bool selected)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			itemRestrictionFamily.ItemFamilyController.SetSelected(selected);
		}
	}

	[DevConsoleCommand("ItemRestrictionGetAllActiveFamilies")]
	public static void DebugItemRestrictionGetAllActiveFamilies(bool areActive = true, bool getOnlyUnlockedFamilies = false)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
			return;
		}
		Dictionary<ItemDefinition.E_Category, List<string>> dictionary = new Dictionary<ItemDefinition.E_Category, List<string>>();
		foreach (ItemRestrictionFamily itemRestrictionFamily in TPSingleton<ItemRestrictionManager>.Instance.ItemRestrictionFamilies)
		{
			if (itemRestrictionFamily.IsActive == areActive && (!getOnlyUnlockedFamilies || itemRestrictionFamily.HasUnlockedItems))
			{
				if (dictionary.ContainsKey(itemRestrictionFamily.ItemFamilyDefinition.ItemCategory))
				{
					dictionary[itemRestrictionFamily.ItemFamilyDefinition.ItemCategory].Add(itemRestrictionFamily.ItemFamilyDefinition.ShortId);
					continue;
				}
				dictionary.Add(itemRestrictionFamily.ItemFamilyDefinition.ItemCategory, new List<string> { itemRestrictionFamily.ItemFamilyDefinition.ShortId });
			}
		}
		TPSingleton<DebugManager>.Instance.LogDevConsole("<b>" + (areActive ? "Active" : "Inactive") + "</b> item families :");
		foreach (ItemDefinition.E_Category key in dictionary.Keys)
		{
			TPSingleton<DebugManager>.Instance.LogDevConsole(string.Format("{0} : {1}", key, string.Join(", ", dictionary[key])));
		}
	}

	[DevConsoleCommand("ItemRestrictionWeaponsDetails")]
	public static void DebugItemRestrictionWeaponsDetails()
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
		}
		else
		{
			Debug.Log(TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories);
		}
	}

	[DevConsoleCommand("ItemRestrictionWeaponsSetBoundless")]
	public static void DebugItemRestrictionWeaponsSetBoundless(bool isBoundless)
	{
		if (TPSingleton<ItemRestrictionManager>.Instance == null)
		{
			TPDebug.LogError("No ItemRestrictionManager instantiated !");
		}
		else
		{
			TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.ItemCategoriesCollectionController.SetBoundlessModeActive(isBoundless);
		}
	}
}
