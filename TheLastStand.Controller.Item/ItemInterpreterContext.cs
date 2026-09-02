using TPLib;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Item;

namespace TheLastStand.Controller.Item;

public class ItemInterpreterContext : FormulaInterpreterContext
{
	private TheLastStand.Model.Item.Item item;

	private int All => -1;

	private float BasePrice => item.ItemDefinition.BasePriceByLevel[item.Level];

	private int Rarity => (int)item.Rarity;

	private float Constant1 => ItemDatabase.ItemPriceEquationConstant1;

	private float Constant2 => ItemDatabase.ItemPriceEquationConstant2;

	private float PowerConstant => ItemDatabase.ItemPriceEquationPowerConstant;

	private int MeleeWeaponQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.MeleeWeapon);

	private int RangedWeaponQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.RangeWeapon);

	private int MagicWeaponQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.MagicWeapon);

	private int WeaponQuantity => MeleeWeaponQuantity + RangedWeaponQuantity + MagicWeaponQuantity;

	private int ShieldQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Shield);

	private int BodyArmorQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.BodyArmor);

	private int HelmQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Helm);

	private int BootsQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Boots);

	private int TrinketQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Trinket);

	private int UtilityQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Utility);

	private int PotionQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Potion);

	private int ScrollQuantity => TPSingleton<ShopManager>.Instance.GetAvailableItemsCount(ItemDefinition.E_Category.Scroll);

	private int UsableQuantity => PotionQuantity + ScrollQuantity;

	private int ArmorQuantity => BodyArmorQuantity + HelmQuantity + BootsQuantity;

	private int EquipmentQuantity => ShieldQuantity + ArmorQuantity + TrinketQuantity;

	private int OffHandQuantity => UtilityQuantity + ShieldQuantity;

	private int AllQuantity => WeaponQuantity + EquipmentQuantity + UsableQuantity + OffHandQuantity;

	public ItemInterpreterContext()
	{
	}

	public ItemInterpreterContext(TheLastStand.Model.Item.Item item)
	{
		this.item = item;
	}
}
