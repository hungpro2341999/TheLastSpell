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
	public bool BlockFlying { get; private set; }

	public BuildingDefinition.E_BuildingCategory Category { get; private set; }

	public Vector2 HUDOffset { get; private set; }

	public int OriginX { get; private set; }

	public int OriginY { get; private set; }

	public string ShadowType { get; private set; } = "TilingShadow";

	public string SidewalkType { get; private set; } = "Sidewalk";

	public List<List<Tile.E_UnitAccess>> Tiles { get; private set; }

	public BlueprintModuleDefinition(BuildingDefinition buildingDefinition, XContainer blueprintDefinition)
		: base(buildingDefinition, blueprintDefinition)
	{
	}

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
			Category = ((BuildingDefinition.ConstructionModuleDefinition.NativeMaterialsCost > 0) ? BuildingDefinition.E_BuildingCategory.Defensive : ((BuildingDefinition.ConstructionModuleDefinition.NativeGoldCost > 0) ? BuildingDefinition.E_BuildingCategory.Production : BuildingDefinition.E_BuildingCategory.None));
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
}
