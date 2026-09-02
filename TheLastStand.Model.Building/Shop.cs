using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building;
using TheLastStand.Controller.Item;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Item;
using TheLastStand.Serialization.Building;
using TheLastStand.Serialization.Item;
using TheLastStand.View.Shop;
using UnityEngine;

namespace TheLastStand.Model.Building;

public class Shop : ISerializable, IDeserializable
{
	public enum E_SortType
	{
		None,
		Rarity,
		Level,
		Price
	}

	public static class Constants
	{
		public static readonly List<ItemDefinition.E_Category> ShopSortOrder = new List<ItemDefinition.E_Category>
		{
			ItemDefinition.E_Category.MeleeWeapon,
			ItemDefinition.E_Category.RangeWeapon,
			ItemDefinition.E_Category.MagicWeapon,
			ItemDefinition.E_Category.Shield,
			ItemDefinition.E_Category.BodyArmor,
			ItemDefinition.E_Category.Helm,
			ItemDefinition.E_Category.Boots,
			ItemDefinition.E_Category.Trinket,
			ItemDefinition.E_Category.Utility,
			ItemDefinition.E_Category.Usable,
			ItemDefinition.E_Category.Potion,
			ItemDefinition.E_Category.Scroll
		};
	}

	public bool IsOpened { get; set; }

	public float SellingMultiplier => BuildingDatabase.ShopDefinition.SellingMultiplier + SellRatioLevel + (float)TPSingleton<GlyphManager>.Instance.BonusSellingRatio;

	public float SellRatioLevel { get; set; }

	public ShopController ShopController { get; }

	public List<ShopSlot> ShopSlots { get; } = new List<ShopSlot>();

	public List<ShopInventorySlot> ShopInventorySlots { get; } = new List<ShopInventorySlot>();

	public int ShopRerollPrice => BuildingDatabase.ShopDefinition.RerollPrices[Mathf.Min(BuildingDatabase.ShopDefinition.RerollPrices.Count - 1, ShopRerollIndex)];

	public int ShopRerollIndex { get; set; }

	public ShopView ShopView { get; }

	public int UnitToCompareIndex { get; set; } = -1;

	public Shop(ShopController shopController, ShopView shopView)
	{
		ShopController = shopController;
		ShopView = shopView;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedShop serializedShop = container as SerializedShop;
		foreach (SerializedItemShopSlot itemSlot in serializedShop.ItemSlots)
		{
			if (itemSlot.Item != null)
			{
				TheLastStand.Model.Item.Item item;
				try
				{
					item = new ItemController(itemSlot.Item, null).Item;
				}
				catch (Database<TheLastStand.Database.ItemDatabase>.MissingAssetException arg)
				{
					TPSingleton<BuildingManager>.Instance.LogError($"Skipped loading of item {itemSlot.Item.Id} while deserializing shop controller\n{arg}", CLogLevel.DETAILED);
					continue;
				}
				ShopController.AddItem(item, itemSlot.Item.HasBeenSoldBefore, itemSlot.IsSoldOut);
			}
		}
		ShopRerollIndex = serializedShop.RerollIndex;
	}

	public ISerializedData Serialize()
	{
		SerializedShop serializedShop = new SerializedShop();
		for (int i = 0; i < ShopSlots.Count; i++)
		{
			serializedShop.ItemSlots.Add(ShopSlots[i].Serialize() as SerializedItemShopSlot);
		}
		serializedShop.RerollIndex = ShopRerollIndex;
		return serializedShop;
	}
}
