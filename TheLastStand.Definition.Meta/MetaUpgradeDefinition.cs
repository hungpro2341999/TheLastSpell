using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Bản thiết kế của một nâng cấp Meta (Meta Upgrade Blueprint).
/// <para>Quản lý toàn bộ thông tin về một nâng cấp trong Oraculum: giá mua, phân loại, các điều kiện mở khóa/kích hoạt, danh sách hiệu ứng và các tooltip hiển thị tương ứng.</para>
/// </summary>
public class MetaUpgradeDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Phân loại danh mục của nâng cấp Meta (dạng bitwise flag để có thể kết hợp nhiều danh mục).
	/// </summary>
	[Flags]
	public enum E_MetaUpgradeCategory
	{
		None = 0,
		Misc = 1,
		City = 2,
		Building = 4,
		Weapon = 8,
		Equipment = 0x10,
		Glyph = 0x20,
		Hero = 0x40,
		All = 0x7F
	}

	/// <summary>
	/// Bộ lọc trạng thái nâng cấp hiển thị trên giao diện người dùng (UI Filter).
	/// </summary>
	[Flags]
	public enum E_MetaUpgradeFilter
	{
		None = 0,
		/// <summary>Đã mua / đã sở hữu</summary>
		Acquired = 1,
		/// <summary>Đang bị khóa</summary>
		Locked = 2,
		/// <summary>Đã mở khóa nhưng chưa mua</summary>
		NotAcquiredYet = 4,
		/// <summary>Nâng cấp mới xuất hiện</summary>
		New = 8
	}

	/// <summary>
	/// Nhóm điều kiện logic (Conditions Group) phục vụ việc mở khóa hoặc kích hoạt nâng cấp.
	/// </summary>
	public class ConditionsGroup
	{
		/// <summary>Chỉ số thứ tự của nhóm điều kiện.</summary>
		public int GroupIndex;

		/// <summary>Nếu true, nhóm điều kiện này chỉ cần kiểm tra thỏa mãn 1 lần duy nhất.</summary>
		public bool CheckOnce;

		/// <summary>Danh sách các điều kiện cụ thể trong nhóm.</summary>
		public List<MetaConditionDefinition> Conditions = new List<MetaConditionDefinition>();
	}

	/// <summary>
	/// Danh mục tổng hợp của nâng cấp (Hero, Building, Weapon, City, Glyph...).
	/// </summary>
	public E_MetaUpgradeCategory Category { get; private set; }

	/// <summary>
	/// Cho biết nâng cấp này có nằm trong cửa hàng Damned Souls hay không (True nếu có giá mua > 0).
	/// </summary>
	public bool DamnedSoulsShop => Price != 0;

	/// <summary>
	/// Thứ tự deserialize của nâng cấp này.
	/// </summary>
	public int DeserializationIndex { get; private set; }

	/// <summary>
	/// Định danh của DLC nếu nâng cấp này thuộc một bản mở rộng nội dung trả phí.
	/// </summary>
	public string DLCId { get; private set; }

	/// <summary>
	/// Bắt buộc mở khóa nâng cấp này theo kịch bản tiến trình game.
	/// </summary>
	public bool MandatoryUnlock { get; private set; }

	/// <summary>
	/// Nâng cấp có bị ẩn trên giao diện hay không.
	/// </summary>
	public bool Hidden { get; private set; }

	/// <summary>
	/// Tên sprite/icon hiển thị của nâng cấp. Mặc định lấy theo Id nếu không cấu hình riêng.
	/// </summary>
	public string IconName { get; private set; } = string.Empty;

	/// <summary>
	/// Mã định danh duy nhất của nâng cấp Meta (ví dụ: "Weapon_Longbow", "City_StartingGold"...).
	/// </summary>
	public string Id { get; private set; }

	/// <summary>
	/// Kiểm tra nâng cấp có yêu cầu DLC cụ thể hay không.
	/// </summary>
	public bool IsLinkedToDLC => !string.IsNullOrEmpty(DLCId);

	/// <summary>
	/// Giá mua nâng cấp (bằng đơn vị Damned Souls).
	/// </summary>
	public uint Price { get; private set; }

	/// <summary>
	/// Danh sách các nhóm điều kiện kích hoạt hiệu ứng nâng cấp sau khi mua.
	/// </summary>
	public List<ConditionsGroup> ActivationConditionsDefinitions { get; } = new List<ConditionsGroup>();

	/// <summary>
	/// Danh sách các nhóm điều kiện để mở khóa hiển thị nâng cấp trong Oraculum.
	/// </summary>
	public List<ConditionsGroup> UnlockConditionsDefinitions { get; } = new List<ConditionsGroup>();

	/// <summary>
	/// Danh sách các hiệu ứng (MetaEffect) mà nâng cấp này đem lại cho người chơi.
	/// </summary>
	public List<MetaEffectDefinition> UpgradeEffectDefinitions { get; } = new List<MetaEffectDefinition>();

	/// <summary>
	/// Danh sách Id hành động công trình (Building Action) cần hiển thị tooltip khi xem nâng cấp.
	/// </summary>
	public List<string> BuildingActionsToShow { get; private set; } = new List<string>();

	/// <summary>
	/// Danh sách Id công trình (Building) cần hiển thị tooltip khi xem nâng cấp.
	/// </summary>
	public List<string> BuildingsToShow { get; private set; } = new List<string>();

	/// <summary>
	/// Danh sách Id nhánh nâng cấp công trình cần hiển thị tooltip khi xem nâng cấp.
	/// </summary>
	public List<string> BuildingUpgradesToShow { get; private set; } = new List<string>();

	/// <summary>
	/// Danh sách Id Glyph (Khắc ấn) cần hiển thị tooltip khi xem nâng cấp.
	/// </summary>
	public List<string> GlyphsToShow { get; private set; } = new List<string>();

	/// <summary>
	/// Danh sách Id vật phẩm (Item/Weapon) cần hiển thị tooltip khi xem nâng cấp.
	/// </summary>
	public List<string> ItemsToShow { get; private set; } = new List<string>();

	/// <summary>
	/// Khởi tạo định nghĩa nâng cấp Meta với chỉ số giải tuần tự hóa.
	/// </summary>
	public MetaUpgradeDefinition(XContainer container, int deserializationIndex)
		: base(container)
	{
		DeserializationIndex = deserializationIndex;
	}

	/// <summary>
	/// Giải tuần tự hóa toàn bộ dữ liệu cấu hình nâng cấp từ XML: thuộc tính cơ bản, điều kiện, hiệu ứng và các tooltip.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Id = xElement.Attribute("Id").Value;
		IconName = xElement.Element("IconName")?.Value ?? Id;
		Hidden = xElement.Element("Hidden") != null;
		Price = uint.Parse(xElement.Attribute("Price")?.Value ?? Price.ToString());
		MandatoryUnlock = xElement.Element("MandatoryUnlock") != null;
		
		XAttribute xAttribute = xElement.Attribute("DLCId");
		if (xAttribute != null)
		{
			DLCId = xAttribute.Value;
		}

		// Đọc các nhóm điều kiện mở khóa (UnlockConditions)
		XElement xElement2 = xElement.Element("UnlockConditions");
		if (xElement2 != null)
		{
			DeserializeConditions(xElement2, UnlockConditionsDefinitions);
		}

		// Đọc các nhóm điều kiện kích hoạt (ActivationConditions)
		XElement xElement3 = xElement.Element("ActivationConditions");
		if (xElement3 != null)
		{
			DeserializeConditions(xElement3, ActivationConditionsDefinitions);
		}

		// Đọc và phân loại danh sách các hiệu ứng nâng cấp (UpgradeEffects)
		XElement xElement4 = xElement.Element("UpgradeEffects");
		if (xElement4 != null)
		{
			foreach (XElement item in xElement4.Elements())
			{
				switch (item.Name.LocalName)
				{
				case "AdditionalInitMages":
					UpgradeEffectDefinitions.Add(new AdditionalInitMagesMetaEffectDefinition(item));
					break;
				case "AdditionalRerollReward":
					UpgradeEffectDefinitions.Add(new AdditionalRerollRewardMetaEffectDefinition(item));
					break;
				case "BuildingModifier":
					UpgradeEffectDefinitions.Add(new BuildingModifierMetaEffectDefinition(item));
					break;
				case "CreateItemModifier":
					UpgradeEffectDefinitions.Add(new CreateItemModifierMetaEffectDefinition(item));
					break;
				case "FogModifier":
					UpgradeEffectDefinitions.Add(new FogModifierMetaEffectDefinition(item));
					break;
				case "InitResourcesBonus":
					UpgradeEffectDefinitions.Add(new InitResourcesBonusMetaEffectDefinition(item));
					break;
				case "ItemLevelProbabilityModifier":
					UpgradeEffectDefinitions.Add(new ItemLevelProbabilityMetaEffectDefinition(item));
					break;
				case "ItemRaritiesModifier":
					UpgradeEffectDefinitions.Add(new ItemRaritiesMetaEffectDefinition(item));
					break;
				case "LockItems":
					UpgradeEffectDefinitions.Add(new LockItemsMetaEffectDefinition(item));
					break;
				case "NewEnemy":
					UpgradeEffectDefinitions.Add(new NewEnemyMetaEffectDefinition(item));
					break;
				case "TraitsParameters":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new TraitsParametersMetaEffectDefinition(item));
					break;
				case "PlayableUnitAttributeModifier":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnitAttributeModifierMetaEffectDefinition(item));
					break;
				case "UnlockAffixes":
					UpgradeEffectDefinitions.Add(new UnlockAffixesMetaEffectDefinition(item));
					break;
				case "UnlockBuildingAction":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingActionMetaEffectDefinition unlockBuildingActionMetaEffectDefinition = new UnlockBuildingActionMetaEffectDefinition(item);
					BuildingActionsToShow.Add(unlockBuildingActionMetaEffectDefinition.BuildingActionId);
					UpgradeEffectDefinitions.Add(unlockBuildingActionMetaEffectDefinition);
					break;
				}
				case "UnlockBuilding":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingMetaEffectDefinition unlockBuildingMetaEffectDefinition = new UnlockBuildingMetaEffectDefinition(item);
					BuildingsToShow.Add(unlockBuildingMetaEffectDefinition.BuildingId);
					UpgradeEffectDefinitions.Add(unlockBuildingMetaEffectDefinition);
					break;
				}
				case "UnlockBuildingUpgrade":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingUpgradeMetaEffectDefinition unlockBuildingUpgradeMetaEffectDefinition = new UnlockBuildingUpgradeMetaEffectDefinition(item);
					BuildingUpgradesToShow.Add(unlockBuildingUpgradeMetaEffectDefinition.UpgradeId);
					UpgradeEffectDefinitions.Add(unlockBuildingUpgradeMetaEffectDefinition);
					break;
				}
				case "UnlockEquipmentGeneration":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockEquipmentGenerationMetaEffectDefinition(item));
					break;
				case "UnlockItems":
				{
					UnlockItemsMetaEffectDefinition unlockItemsMetaEffectDefinition = new UnlockItemsMetaEffectDefinition(item);
					foreach (string item2 in unlockItemsMetaEffectDefinition.ItemsToUnlock)
					{
						if (ItemDatabase.ItemDefinitions.TryGetValue(item2, out var value))
						{
							if (value.IsWeapon)
							{
								Category |= E_MetaUpgradeCategory.Weapon;
							}
							else
							{
								Category |= E_MetaUpgradeCategory.Equipment;
							}
						}
					}
					ItemsToShow.AddRange(unlockItemsMetaEffectDefinition.ItemsToUnlock);
					UpgradeEffectDefinitions.Add(unlockItemsMetaEffectDefinition);
					break;
				}
				case "UnlockCities":
					Category |= E_MetaUpgradeCategory.City;
					UpgradeEffectDefinitions.Add(new UnlockCitiesMetaEffectDefinition(item));
					break;
				case "UnlockGlyphs":
				{
					Category |= E_MetaUpgradeCategory.Glyph;
					UnlockGlyphsMetaEffectDefinition unlockGlyphsMetaEffectDefinition = new UnlockGlyphsMetaEffectDefinition(item);
					GlyphsToShow.AddRange(unlockGlyphsMetaEffectDefinition.GlyphIds);
					UpgradeEffectDefinitions.Add(unlockGlyphsMetaEffectDefinition);
					break;
				}
				case "UnlockPerkCollectionSlots":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockPerkCollectionSlotsMetaEffectDefinition(item));
					break;
				case "UnlockRaces":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockRacesMetaEffectDefinition(item));
					break;
				case "UnlockRerollReward":
					UpgradeEffectDefinitions.Add(new UnlockRerollRewardMetaEffectDefinition(item));
					break;
				case "UnlockShopReroll":
					UpgradeEffectDefinitions.Add(new UnlockShopRerollMetaEffectDefinition(item));
					break;
				case "UnlockSink":
					UpgradeEffectDefinitions.Add(new UnlockSinkMetaEffectDefinition(item));
					break;
				case "UnlockTraits":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockTraitsMetaEffectDefinition(item));
					break;
				case "UnlockWaves":
					UpgradeEffectDefinitions.Add(new UnlockWavesMetaEffectDefinition(item));
					break;
				case "UpgradeCity":
					UpgradeEffectDefinitions.Add(new UpgradeCityMetaEffectDefinition(item));
					break;
				case "WavesParameters":
					UpgradeEffectDefinitions.Add(new WavesParametersMetaEffectDefinition(item));
					break;
				default:
					CLoggerManager.Log("MetaUpgrade effect " + item.Name.LocalName + " is not handled to be parsed as a valid definition!", LogType.Error);
					break;
				}
			}

			// Xử lý các tooltip được ép hiển thị (ForceDisplayTooltips)
			XElement xElement5 = xElement.Element("ForceDisplayTooltips");
			if (xElement5 != null)
			{
				foreach (XElement item3 in xElement5.Elements())
				{
					XAttribute xAttribute2 = item3.Attribute("Id");
					switch (item3.Name.LocalName)
					{
					case "ItemTooltip":
						ItemsToShow.Add(xAttribute2.Value);
						break;
					case "GlyphTooltip":
						GlyphsToShow.Add(xAttribute2.Value);
						break;
					case "BuildingTooltip":
						BuildingsToShow.Add(xAttribute2.Value);
						break;
					case "BuildingActionTooltip":
						BuildingActionsToShow.Add(xAttribute2.Value);
						break;
					case "BuildingUpgradeTooltip":
						BuildingUpgradesToShow.Add(xAttribute2.Value);
						break;
					}
				}
			}

			// Xử lý các tooltip bị ép ẩn (ForceHideTooltips)
			XElement xElement6 = xElement.Element("ForceHideTooltips");
			if (xElement6 != null)
			{
				foreach (XElement item4 in xElement6.Elements())
				{
					XAttribute xAttribute3 = item4.Attribute("Id");
					switch (item4.Name.LocalName)
					{
					case "ItemTooltip":
						ItemsToShow.Remove(xAttribute3.Value);
						break;
					case "GlyphTooltip":
						GlyphsToShow.Remove(xAttribute3.Value);
						break;
					case "BuildingTooltip":
						BuildingsToShow.Remove(xAttribute3.Value);
						break;
					case "BuildingActionTooltip":
						BuildingActionsToShow.Remove(xAttribute3.Value);
						break;
					case "BuildingUpgradeTooltip":
						BuildingUpgradesToShow.Remove(xAttribute3.Value);
						break;
					}
				}
			}

			// Đọc cấu hình ghi đè danh mục nếu có (<Categories>)
			XElement xElement7 = xElement.Element("Categories");
			if (xElement7 != null)
			{
				XAttribute xAttribute4 = xElement7.Attribute("OverrideAutomaticCategories");
				bool result = default(bool);
				if (xAttribute4 != null && bool.TryParse(xAttribute4.Value, out result) && result)
				{
					Category = E_MetaUpgradeCategory.None;
				}
				foreach (XElement item5 in xElement7.Elements("Category"))
				{
					XAttribute xAttribute5 = item5.Attribute("Value");
					if (Enum.TryParse<E_MetaUpgradeCategory>(xAttribute5.Value, out var result2))
					{
						Category |= result2;
					}
					else
					{
						CLoggerManager.Log("Could not parse Category attribute into a meta upgrade category in meta upgrade " + Id + " : " + xAttribute5.Value);
					}
				}
			}

			// Nếu không có danh mục cụ thể, mặc định là Misc
			if (Category == E_MetaUpgradeCategory.None)
			{
				Category = E_MetaUpgradeCategory.Misc;
			}
			if (Category == E_MetaUpgradeCategory.Misc)
			{
				CLoggerManager.Log("Meta upgrade doesn't have a category except Misc. This shouldn't happen ! (" + Id + ").");
			}
		}
		else
		{
			CLoggerManager.Log("MetaUpgrade " + Id + " doesn't have UpgradeEffects element!", LogType.Error);
		}
	}

	public override string ToString()
	{
		string log = "<b>#--- " + Id + (Hidden ? "(Hidden)" : "") + " ---#</b>\n";
		log += "Unlock Conditions Groups :\n";
		for (int i = 0; i < UnlockConditionsDefinitions.Count; i++)
		{
			log += $"Group {i + 1}:\n";
			UnlockConditionsDefinitions[i].Conditions.ForEach(delegate(MetaConditionDefinition o)
			{
				log += $"- {o}\n";
			});
		}
		log += "Activation Conditions Groups :\n";
		for (int num = 0; num < ActivationConditionsDefinitions.Count; num++)
		{
			log += $"Group {num + 1}:\n";
			ActivationConditionsDefinitions[num].Conditions.ForEach(delegate(MetaConditionDefinition o)
			{
				log += $"- {o}\n";
			});
		}
		log += "Effects :\n";
		UpgradeEffectDefinitions.ForEach(delegate(MetaEffectDefinition o)
		{
			log += $"- {o}\n";
		});
		return log;
	}

	/// <summary>
	/// Phân tích danh sách nhóm điều kiện từ thẻ XML &lt;ConditionsGroup&gt;.
	/// </summary>
	private void DeserializeConditions(XElement conditionsElement, List<ConditionsGroup> conditionsDefinitions)
	{
		int num = 0;
		int num2 = 0;
		foreach (XElement item in conditionsElement.Elements("ConditionsGroup"))
		{
			bool flag = item.Element("Hidden") != null;
			if (!flag)
			{
				num2++;
			}
			bool checkOnce = item.Element("CheckOnce") != null;
			ConditionsGroup conditionsGroup = new ConditionsGroup
			{
				CheckOnce = checkOnce,
				GroupIndex = num
			};
			foreach (XElement item2 in item.Elements())
			{
				if (!(item2.Name.LocalName == "Hidden") && !(item2.Name.LocalName == "CheckOnce"))
				{
					try
					{
						conditionsGroup.Conditions.Add(new MetaConditionDefinition(item2, flag, num));
					}
					catch (Exception arg)
					{
						CLoggerManager.Log($"Caught and skipped invalid or obsolete condition definition in MetaUpgrade {Id}:\n{arg}", LogType.Error);
					}
				}
			}
			conditionsDefinitions.Add(conditionsGroup);
			num++;
		}
		if (num2 >= 2)
		{
			CLoggerManager.Log("More than one unlock/activation conditions groups are shown in MetaUpgrade " + Id + ". At most ONE must be visible.", LogType.Error);
		}
	}
}
