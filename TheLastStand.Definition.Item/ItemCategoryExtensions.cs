using TPLib.Localization;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Extension methods cho E_Category — chuyển category enum thành tên hiển thị (localized).
/// 
/// Đặc biệt: Potion và Scroll đều trả về key "CategoryName_Usable"
/// (gộp chung thành 1 tên "Vật phẩm tiêu hao" trong UI).
/// 
/// Ví dụ:
/// - E_Category.MeleeWeapon.GetLocalizedName() → "Vũ khí cận chiến"
/// - E_Category.Potion.GetLocalizedName() → "Vật phẩm tiêu hao" (dùng key Usable)
/// - E_Category.Scroll.GetLocalizedName() → "Vật phẩm tiêu hao" (dùng key Usable)
/// </summary>
public static class ItemCategoryExtensions
{
	/// <summary>
	/// Lấy tên localized của category.
	/// </summary>
	/// <param name="category">Category cần lấy tên.</param>
	/// <returns>Tên đã localize (ví dụ: "Vũ khí cận chiến").</returns>
	public static string GetLocalizedName(this ItemDefinition.E_Category category)
	{
		return Localizer.Get(category.GetLocalizationKey());
	}

	/// <summary>
	/// Lấy localization key cho category.
	/// Nếu category là Potion hoặc Scroll → gộp chung thành "CategoryName_Usable".
	/// </summary>
	/// <param name="category">Category cần lấy key.</param>
	/// <returns>Localization key. Ví dụ: "CategoryName_MeleeWeapon".</returns>
	public static string GetLocalizationKey(this ItemDefinition.E_Category category)
	{
		// Potion và Scroll đều dùng chung key "CategoryName_Usable"
		if (ItemDefinition.E_Category.Usable.HasFlag(category))
		{
			category = ItemDefinition.E_Category.Usable;
		}
		return string.Format("{0}{1}", "CategoryName_", category);
	}
}
