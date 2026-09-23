using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa quy tắc sửa chữa và thiết lập xây dựng chung (Construction Definition).
/// </summary>
public class ConstructionDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Properties

	/// <summary>
	/// Tỷ lệ chi phí sửa chữa so với chi phí xây dựng gốc.
	/// </summary>
	public float RepairCostRatio { get; private set; }

	/// <summary>
	/// Từ điển chứa danh sách các nút danh mục sửa chữa công trình.
	/// </summary>
	public Dictionary<string, List<BuildingDefinition.E_BuildingCategory>> RepairCategoryButtons { get; } = new Dictionary<string, List<BuildingDefinition.E_BuildingCategory>>();

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa xây dựng/sửa chữa từ dữ liệu XML.
	/// </summary>
	public ConstructionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho cấu hình sửa chữa và danh mục nút bấm.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("RepairCostRatio");
		if (xElement2 == null)
		{
			CLoggerManager.Log("ConstructionDefinition must have a RepairCostRatio", LogType.Error);
			return;
		}
		if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log("ConstructionDefinition RepairCostRatio must be a valid float", LogType.Error);
			return;
		}
		RepairCostRatio = result * 0.01f;
		foreach (XElement item in xElement.Element("RepairCategoryButtons").Elements("RepairCategoryButton"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			List<BuildingDefinition.E_BuildingCategory> list = new List<BuildingDefinition.E_BuildingCategory>();
			foreach (XElement item2 in item.Elements("FlagId"))
			{
				if (!Enum.TryParse<BuildingDefinition.E_BuildingCategory>(item2.Value, out var result2))
				{
					CLoggerManager.Log("Could not parse " + item2.Value + " as a valid E_BuildingCategory.");
				}
				else
				{
					list.Add(result2);
				}
			}
			RepairCategoryButtons.Add(xAttribute.Value, list);
		}
	}

	#endregion
}

