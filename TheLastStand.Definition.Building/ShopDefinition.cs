using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa cho Cửa hàng (Shop Definition).
/// Quản lý hệ số bán đồ (SellingMultiplier), danh sách giá đổi mới hàng (RerollPrices) và cấu hình tiến hóa cửa hàng (ShopEvolutionDefinitions).
/// </summary>
public class ShopDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Properties

	/// <summary>
	/// Hệ số nhân khi bán đồ vào cửa hàng.
	/// </summary>
	public float SellingMultiplier { get; private set; }

	/// <summary>
	/// Danh sách chi phí Vàng cần thiết cho mỗi lần làm mới danh sách mặt hàng (Reroll).
	/// </summary>
	public List<int> RerollPrices { get; private set; }

	/// <summary>
	/// Từ điển chứa định nghĩa tiến hóa cấp độ cửa hàng theo ngày.
	/// </summary>
	public Dictionary<string, ShopEvolutionDefinition> ShopEvolutionDefinitions { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa cửa hàng từ dữ liệu XML.
	/// </summary>
	public ShopDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho cấu hình cửa hàng.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("SellingMultiplier");
		if (xElement2 == null)
		{
			Debug.LogError("ShopDefinion must have a SellingMultiplier");
			return;
		}
		if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Debug.LogError("ShopDefinition SellingMultiplier must have a valid float value");
			return;
		}
		SellingMultiplier = result;
		RerollPrices = new List<int>();
		XElement xElement3 = xElement.Element("Rerolls");
		int i = 0;
		int item = 0;
		foreach (XElement item2 in xElement3.Elements("Price"))
		{
			XAttribute xAttribute = item2.Attribute("Id");
			if (!int.TryParse(xAttribute.Value, out var result2))
			{
				CLoggerManager.Log("Can't parse id attribute in price element into an int : " + xAttribute.Value, LogType.Error, CLogLevel.MAJOR);
				continue;
			}
			if (!int.TryParse(item2.Value, out var result3))
			{
				CLoggerManager.Log("Can't parse price element into an int : " + item2.Value, LogType.Error, CLogLevel.MAJOR);
				continue;
			}
			if (result2 <= i)
			{
				CLoggerManager.Log($"index ({result2}) is inferior or equal to the current index ({i}), skipping this one.", LogType.Error, CLogLevel.MAJOR);
				continue;
			}
			for (; i < result2 - 1; i++)
			{
				RerollPrices.Add(item);
			}
			RerollPrices.Add(result3);
			i++;
			item = result3;
		}
		if (i == 0)
		{
			CLoggerManager.Log("There is no valid shop reroll prices with strictly positive index, that is unexpected !", LogType.Error, CLogLevel.MAJOR);
			RerollPrices.Add(0);
		}
		XElement xElement4 = xElement.Element("ShopEvolutions");
		ShopEvolutionDefinitions = new Dictionary<string, ShopEvolutionDefinition>();
		foreach (XElement item3 in xElement4.Elements("ShopEvolution"))
		{
			XAttribute xAttribute2 = item3.Attribute("Id");
			ShopEvolutionDefinitions.Add(xAttribute2.Value, new ShopEvolutionDefinition(item3));
		}
	}

	#endregion
}

