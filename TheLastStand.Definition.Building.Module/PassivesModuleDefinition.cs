using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class PassivesModuleDefinition : BuildingModuleDefinition
{
	#region Fields & Properties
	private List<BuildingPassiveDefinition> buildingPassiveDefinitions;

	/// <summary>
	/// Danh sách các định nghĩa nội tại/bị động (BuildingPassiveDefinition) của công trình
	/// (có tính đến chỉnh sửa từ GlyphManager nếu có).
	/// </summary>
	public List<BuildingPassiveDefinition> BuildingPassiveDefinitions
	{
		get
		{
			if (!TPSingleton<GlyphManager>.Exist())
			{
				return buildingPassiveDefinitions;
			}
			return TPSingleton<GlyphManager>.Instance.GetModifiedBuildingPassives(BuildingDefinition.Id, buildingPassiveDefinitions);
		}
	}

	/// <summary>
	/// Cho biết công trình có hiệu ứng bị động kích hoạt khi tử trận (OnDeath) hay không.
	/// </summary>
	public bool HasOnDeathEffect { get; private set; }
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo định nghĩa module bị động của công trình.
	/// </summary>
	public PassivesModuleDefinition(BuildingDefinition buildingDefinition, XContainer passivesDefinition)
		: base(buildingDefinition, passivesDefinition)
	{
	}
	#endregion

	#region Deserialization
	/// <summary>
	/// Đọc danh sách các ID nội tại từ XML và tra cứu từ BuildingDatabase.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		buildingPassiveDefinitions = new List<BuildingPassiveDefinition>();
		foreach (XElement item in xElement.Elements("BuildingPassiveDefinition"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (!BuildingDatabase.BuildingPassiveDefinitions.TryGetValue(xAttribute.Value, out var value))
			{
				CLoggerManager.Log("Could not find building passive with the id (" + xAttribute.Value + ").", LogType.Error, CLogLevel.MAJOR);
			}
			else
			{
				buildingPassiveDefinitions.Add(value);
			}
		}
		HasOnDeathEffect = buildingPassiveDefinitions.Any((BuildingPassiveDefinition x) => x.HasOnDeathEffect);
	}
	#endregion
}
