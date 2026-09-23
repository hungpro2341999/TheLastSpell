using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa quy tắc tạo công trình ngẫu nhiên trên bản đồ (Random Buildings Generation).
/// Quản lý danh sách thông tin công trình (`BuildingInfo`), vị trí đặt tile flag và số lượng sinh.
/// </summary>
public class RandomBuildingsGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Constants

	public static class Constants
	{
		public const string AllowingRandomBuildingsIdsList = "AllowingRandomBuildings";
	}

	#endregion

	#region Structs

	/// <summary>
	/// Struct lưu thông tin về công trình được sinh ngẫu nhiên.
	/// </summary>
	public struct BuildingInfo
	{
		/// <summary>
		/// ID công trình.
		/// </summary>
		public string Id;

		/// <summary>
		/// Cờ TileFlag quy định vị trí cho phép đặt công trình.
		/// </summary>
		public TileFlagDefinition.E_TileFlagTag TileFlag;

		/// <summary>
		/// Số lượng công trình cần sinh.
		/// </summary>
		public int Count;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Mã ID định danh của quy tắc sinh công trình.
	/// </summary>
	public string Id { get; private set; }

	/// <summary>
	/// Danh sách các công trình cần sinh ngẫu nhiên.
	/// </summary>
	public List<BuildingInfo> BuildingsInfo { get; } = new List<BuildingInfo>();

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa tạo công trình ngẫu nhiên từ dữ liệu XML.
	/// </summary>
	public RandomBuildingsGenerationDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho danh sách công trình sinh ngẫu nhiên.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		foreach (XElement item2 in obj.Elements("Building"))
		{
			XAttribute xAttribute2 = item2.Attribute("Id");
			XAttribute xAttribute3 = item2.Attribute("TileFlag");
			if (!Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(xAttribute3.Value, out var result))
			{
				CLoggerManager.Log("Could not parse TileFlag attribute value " + xAttribute3.Value + " of Building " + xAttribute2.Value + " in RandomBuildingsGenerationDefinition " + Id + "!", LogType.Error);
				break;
			}
			XAttribute xAttribute4 = item2.Attribute("Count");
			if (!int.TryParse(xAttribute4.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse Count attribute value " + xAttribute4.Value + " of Building " + xAttribute2.Value + " in RandomBuildingsGenerationDefinition " + Id + "!", LogType.Error);
				break;
			}
			BuildingInfo item = new BuildingInfo
			{
				Id = xAttribute2.Value,
				TileFlag = result,
				Count = result2
			};
			BuildingsInfo.Add(item);
		}
	}

	#endregion
}

