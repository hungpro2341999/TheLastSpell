using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa tiến hóa cấp độ hàng hóa của Cửa hàng theo số ngày chơi (Shop Evolution).
/// Chứa danh sách tuple (Số ngày -> Cấp độ đồ xuất hiện).
/// </summary>
public class ShopEvolutionDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Properties

	/// <summary>
	/// Danh sách cặp giá trị Tuple(DayIndex, ItemLevel) thể hiện sự thay đổi cấp độ hàng hóa theo từng ngày.
	/// </summary>
	public List<Tuple<int, int>> LevelsPerDay { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa tiến hóa cửa hàng từ dữ liệu XML.
	/// </summary>
	public ShopEvolutionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho cấu hình cấp độ đồ shop theo ngày.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		LevelsPerDay = new List<Tuple<int, int>>();
		foreach (XElement item in obj.Elements("Day"))
		{
			XAttribute xAttribute = item.Attribute("Index");
			XAttribute xAttribute2 = item.Attribute("Level");
			LevelsPerDay.Add(new Tuple<int, int>(int.Parse(xAttribute.Value), int.Parse(xAttribute2.Value)));
		}
	}

	#endregion
}

