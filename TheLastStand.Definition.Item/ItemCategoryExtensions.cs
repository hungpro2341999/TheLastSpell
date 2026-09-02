using TPLib.Localization;

namespace TheLastStand.Definition.Item;

public static class ItemCategoryExtensions
{
	public static string GetLocalizedName(this ItemDefinition.E_Category category)
	{
		return Localizer.Get(category.GetLocalizationKey());
	}

	public static string GetLocalizationKey(this ItemDefinition.E_Category category)
	{
		if (ItemDefinition.E_Category.Usable.HasFlag(category))
		{
			category = ItemDefinition.E_Category.Usable;
		}
		return string.Format("{0}{1}", "CategoryName_", category);
	}
}
