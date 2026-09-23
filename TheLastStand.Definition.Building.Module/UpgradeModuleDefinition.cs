using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class UpgradeModuleDefinition : BuildingModuleDefinition
{
	#region Properties
	/// <summary>
	/// Danh sách các định nghĩa nâng cấp (BuildingUpgradeDefinition) có sẵn cho công trình.
	/// </summary>
	public List<BuildingUpgradeDefinition> BuildingUpgradeDefinitions { get; private set; }

	/// <summary>
	/// ID của công trình tiền thân mà công trình hiện tại là bản nâng cấp từ đó.
	/// </summary>
	public string UpgradeOf { get; set; }
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo định nghĩa module nâng cấp của công trình.
	/// </summary>
	public UpgradeModuleDefinition(BuildingDefinition buildingDefinition, XContainer upgradeDefinition)
		: base(buildingDefinition, upgradeDefinition)
	{
	}
	#endregion

	#region Upgrade Hierarchy
	/// <summary>
	/// Truy ngược cây nâng cấp để lấy danh sách tất cả các ID công trình tiền thân cấp thấp hơn.
	/// </summary>
	public List<string> GetPreviousUpgrades()
	{
		List<string> list = new List<string>();
		BuildingDefinition buildingDefinition = BuildingDefinition;
		while (buildingDefinition.UpgradeModuleDefinition.UpgradeOf != null)
		{
			list.Add(buildingDefinition.UpgradeModuleDefinition.UpgradeOf);
			buildingDefinition = BuildingDatabase.BuildingDefinitions[buildingDefinition.UpgradeModuleDefinition.UpgradeOf];
		}
		return list;
	}
	#endregion

	#region Deserialization
	/// <summary>
	/// Đọc thông tin công trình gốc (UpgradeOf) và danh sách các BuildingUpgradeDefinition từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("UpgradeOf");
		if (xElement2 != null)
		{
			if (xElement2.IsNullOrEmpty())
			{
				Debug.LogError("Building " + BuildingDefinition.Id + " has an invalid UpgradeOf !");
				return;
			}
			UpgradeOf = xElement2.Value;
		}
		XElement xElement3 = xElement.Element("BuildingUpgradeDefinitions");
		if (xElement3 == null)
		{
			return;
		}
		BuildingUpgradeDefinitions = new List<BuildingUpgradeDefinition>();
		foreach (XElement item in xElement3.Elements("BuildingUpgradeDefinition"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				Debug.LogError("BuildingDefinition " + BuildingDefinition.Id + " BuildingUpgradeDefinition must have an attribute Id");
			}
			if (BuildingDatabase.BuildingUpgradeDefinitions.TryGetValue(xAttribute.Value, out var value))
			{
				BuildingUpgradeDefinitions.Add(value);
				continue;
			}
			Debug.LogError("BuildingUpgradeDefinition " + xAttribute.Value + " not found");
			break;
		}
	}
	#endregion
}
