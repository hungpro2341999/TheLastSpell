using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class BrazierModuleDefinition : BuildingModuleDefinition
{
	#region Constants & Properties
	private static class Constants
	{
		public const string PointsTotalElement = "PointsTotal";
	}

	/// <summary>
	/// Tổng số điểm hỏa đài/lửa thiêng (Brazier Points) của công trình.
	/// </summary>
	public int BrazierPointsTotal { get; private set; }
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo định nghĩa module hỏa đài.
	/// </summary>
	public BrazierModuleDefinition(BuildingDefinition buildingDefinition, XContainer constructionDefinition)
		: base(buildingDefinition, constructionDefinition)
	{
	}
	#endregion

	#region Deserialization
	/// <summary>
	/// Đọc tổng số điểm hỏa đài từ XML element "PointsTotal".
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (container is XElement xElement)
		{
			XElement xElement2 = xElement.Element("PointsTotal");
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("PointsTotal couldn't be parsed into an int : " + xElement2.Value + ".,", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "BrazierModuleDefinition");
			}
			BrazierPointsTotal = result;
		}
	}
	#endregion
}
