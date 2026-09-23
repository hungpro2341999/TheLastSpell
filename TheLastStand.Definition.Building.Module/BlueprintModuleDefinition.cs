using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class BlueprintModuleDefinition : BuildingModuleDefinition, ITileObjectDefinition
{
	#region Properties
	/// <summary>
	/// Cho biết công trình có cản trở đơn vị bay (flying units) hay không.
	/// </summary>
	public bool BlockFlying { get; private set; }

	/// <summary>
	/// Phân loại công trình (Phòng thủ, Sản xuất, hoặc Không xác định).
	/// </summary>
	public BuildingDefinition.E_BuildingCategory Category { get; private set; }

	/// <summary>
	/// Độ lệch vị trí hiển thị HUD công trình.
	/// </summary>
	public Vector2 HUDOffset { get; private set; }

	/// <summary>
	/// Tọa độ gốc X của công trình trên lưới chiếm ô.
	/// </summary>
	public int OriginX { get; private set; }

	/// <summary>
	/// Tọa độ gốc Y của công trình trên lưới chiếm ô.
	/// </summary>
	public int OriginY { get; private set; }

	/// <summary>
	/// Loại bóng của công trình.
	/// </summary>
	public string ShadowType { get; private set; } = "TilingShadow";

	/// <summary>
	/// Loại đường đi/vỉa hè xung quanh công trình.
	/// </summary>
	public string SidewalkType { get; private set; } = "Sidewalk";

	/// <summary>
	/// Ma trận các ô tile biểu diễn quyền truy cập của đơn vị (UnitAccess).
	/// </summary>
	public List<List<Tile.E_UnitAccess>> Tiles { get; private set; }
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo định nghĩa module bản vẽ chiếm ô của công trình.
	/// </summary>
	public BlueprintModuleDefinition(BuildingDefinition buildingDefinition, XContainer blueprintDefinition)
		: base(buildingDefinition, blueprintDefinition)
	{
	}
	#endregion

	#region Deserialization
	/// <summary>
	/// Đọc dữ liệu ma trận ô chiếm giữ, danh mục công trình, loại bóng và tọa độ offset từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("Tiles");
		Tiles = new List<List<Tile.E_UnitAccess>>();
		string[] array = xElement2.Value.Split('\n');
		for (int num = array.Length - 1; num >= 0; num--)
		{
			array[num] = array[num].RemoveWhitespace();
			if (array[num] != string.Empty)
			{
				Tiles.Add(new List<Tile.E_UnitAccess>(array[num].Length));
				for (int i = 0; i < array[num].Length; i++)
				{
					char tileChar = array[num][i];
					Tiles[Tiles.Count - 1].Add(Tile.CharToUnitAccess(tileChar));
				}
			}
		}
		OriginX = ((xElement2.Attribute("OriginX") != null) ? int.Parse(xElement2.Attribute("OriginX").Value) : 0);
		OriginY = ((xElement2.Attribute("OriginY") != null) ? int.Parse(xElement2.Attribute("OriginY").Value) : 0);
		string text = xElement.Element("Category")?.Value;
		if (text != null)
		{
			if (Enum.TryParse<BuildingDefinition.E_BuildingCategory>(text, out var result))
			{
				Category = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse the Category element in " + BuildingDefinition.Id + " into an enum : " + text + ".", LogType.Error, CLogLevel.MAJOR);
			}
		}
		else
		{
		}
		XElement xElement3 = xElement.Element("ShadowType");
		if (xElement3 != null)
		{
			if (xElement3.IsEmpty)
			{
				Debug.LogError("BuildingDefinition " + BuildingDefinition.Id + " must have a valid ShadowType!");
				return;
			}
			ShadowType = xElement3.Value;
		}
		XElement xElement4 = xElement.Element("SidewalkType");
		if (!xElement4.IsNullOrEmpty())
		{
			SidewalkType = xElement4.Value;
		}
		XElement xElement5 = xElement.Element("BuildingHUDOffset");
		if (xElement5 != null)
		{
			XAttribute xAttribute = xElement5.Attribute("X");
			XAttribute xAttribute2 = xElement5.Attribute("Y");
			HUDOffset = new Vector2(int.Parse(xAttribute.Value), int.Parse(xAttribute2.Value));
		}
		else
		{
			HUDOffset = Vector2.zero;
		}
		BlockFlying = xElement.Element("BlockFlying") != null;
	}
	#endregion
}
