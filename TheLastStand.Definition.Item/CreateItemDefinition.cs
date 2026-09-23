using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// "Đơn đặt hàng" (Factory Config) cho hệ thống sinh vật phẩm.
/// Mô tả CẤU HÌNH để sinh item: sinh bao nhiêu, từ danh sách nào, rarity nào, level tối thiểu bao nhiêu.
/// 
/// Được đọc từ XML và sử dụng bởi ItemManager.GenerateItems():
/// <code>
/// &lt;CreateItem Id="NightReward" MinLevel="2"&gt;
///   &lt;ItemsList Id="AllWeapons"/&gt;
///   &lt;ItemRaritiesList Id="StandardRarities"/&gt;
///   &lt;Count&gt;3&lt;/Count&gt;
/// &lt;/CreateItem&gt;
/// </code>
/// 
/// Lưu ý:
/// - Count có thể là biểu thức (expression), không chỉ là số cố định.
/// - Count = -1 (All) → sinh TẤT CẢ item trong danh sách.
/// - ItemMinLevel = -1 → không giới hạn level tối thiểu.
/// </summary>
public class CreateItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>Hằng số đặc biệt: Count = -1 nghĩa là sinh TẤT CẢ item trong danh sách.</summary>
	public static int All => -1;

	/// <summary>
	/// Số lượng item cần sinh (dạng expression tree, có thể là "2+1", "BaseCount*Modifier"...).
	/// Được evaluate bằng Parser.Parse() → EvalToInt() khi cần giá trị cụ thể.
	/// </summary>
	public Node Count { get; private set; }

	/// <summary>Kiểm tra đơn đặt hàng này có ID không (dùng để match với MetaUpgrade modifier).</summary>
	public bool HasID => !string.IsNullOrEmpty(Id);

	/// <summary>ID đơn đặt hàng (optional). Dùng để MetaUpgrade có thể thay đổi Count.</summary>
	public string Id { get; private set; }

	/// <summary>
	/// ID danh sách modifier level theo building (optional).
	/// Cho phép building ảnh hưởng đến level item được sinh.
	/// </summary>
	public string LevelModifierListId { get; private set; }

	/// <summary>
	/// Level tối thiểu cho item được sinh. -1 = không giới hạn.
	/// Nếu level yêu cầu thấp hơn ItemMinLevel → tự động nâng lên.
	/// </summary>
	public int ItemMinLevel { get; private set; }

	/// <summary>
	/// Danh sách item nguồn để random chọn.
	/// Tham chiếu đến ItemsListDefinition chứa ItemId + weight.
	/// </summary>
	public ItemsListDefinition ItemsListDefinition { get; private set; }

	/// <summary>
	/// Bảng xác suất rarity (Common/Magic/Rare/Epic).
	/// Quyết định tỉ lệ sinh item ở mỗi mức độ hiếm.
	/// </summary>
	public ProbabilityTreeEntriesDefinition ItemRaritiesListDefinition { get; private set; }

	public CreateItemDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc cấu hình sinh item từ XML element.
	/// Thứ tự đọc: Id → MinLevel → ItemsList → BuildingLevelModifiersList → ItemRaritiesList → Count.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		// Đọc ID (optional - dùng cho MetaUpgrade matching)
		Id = xElement.Attribute("Id")?.Value;
		// Đọc MinLevel (optional, default = -1 = không giới hạn)
		XAttribute xAttribute = xElement.Attribute("MinLevel");
		ItemMinLevel = -1;
		if (xAttribute != null)
		{
			if (!int.TryParse(xAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				TPDebug.LogError("CreateItemDefinition " + Id + "'s MinLevel " + HasAnInvalidInt(xAttribute.Value));
				return;
			}
			if (result >= 0)
			{
				ItemMinLevel = result;
			}
		}
		// Đọc ItemsList - danh sách item nguồn (bắt buộc)
		XAttribute xAttribute2 = xElement.Element("ItemsList").Attribute("Id");
		if (!ItemDatabase.ItemsListDefinitions.TryGetValue(xAttribute2.Value, out var value))
		{
			CLoggerManager.Log(xAttribute2.Value + " items list not found!", LogType.Error);
			return;
		}
		ItemsListDefinition = value;
		// Đọc BuildingLevelModifiersList (optional)
		XElement xElement2 = xElement.Element("BuildingLevelModifiersList");
		if (xElement2 != null)
		{
			LevelModifierListId = xElement2.Attribute("Id")?.Value;
		}
		// Đọc ItemRaritiesList - bảng xác suất rarity (bắt buộc)
		XElement xElement3 = xElement.Element("ItemRaritiesList");
		if (xElement3 == null)
		{
			CLoggerManager.Log("CreateItem levels missing!", LogType.Assert);
			return;
		}
		XAttribute xAttribute3 = xElement3.Attribute("Id");
		if (xAttribute3.IsNullOrEmpty() || !ItemDatabase.ItemRaritiesListDefinitions.ContainsKey(xAttribute3.Value))
		{
			CLoggerManager.Log("CreateItem ItemRarities Id is not valid or does not exist in ItemRaritiesListDefinitions", LogType.Error);
			return;
		}
		if (!ItemDatabase.ItemRaritiesListDefinitions.TryGetValue(xAttribute3.Value, out var value2))
		{
			CLoggerManager.Log(xAttribute3.Value + " items rarities list not found!", LogType.Error);
			return;
		}
		ItemRaritiesListDefinition = value2;
		// Đọc Count - số lượng item cần sinh (default = 1)
		XElement xElement4 = xElement.Element("Count");
		Count = (xElement4.IsNullOrEmpty() ? Parser.Parse("1") : Parser.Parse(xElement4.Value));
	}
}
