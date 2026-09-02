using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database;
using TheLastStand.Definition;
using TheLastStand.Definition.TileMap;
using TheLastStand.Manager;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Model.TileMap;

public class TileMap
{
	public List<Tile> CityTiles { get; set; } = new List<Tile>();

	public int Height { get; set; }

	public int Width { get; set; }

	public TileMapView TileMapView { get; }

	public Tile[] Tiles { get; set; }

	public Dictionary<TileFlagDefinition.E_TileFlagTag, List<Tile>> TilesWithFlag { get; } = new Dictionary<TileFlagDefinition.E_TileFlagTag, List<Tile>>(TileFlagDefinition.SharedTileFlagTagComparer);

	public TileMap(XContainer container, TileMapView tileMapView)
	{
		Deserialize(container);
		TileMapView = tileMapView;
	}

	public Tile GetTile(int x, int y)
	{
		if (!IsTileValid(x, y))
		{
			return null;
		}
		return Tiles[x * Height + y];
	}

	public Tile GetTile(Vector2Int position)
	{
		if (!IsTileValid(position.x, position.y))
		{
			return null;
		}
		return Tiles[position.x * Height + position.y];
	}

	public bool IsTileValid(int x, int y)
	{
		if (IsHorizontalPosValid(x))
		{
			return IsVerticalPosValid(y);
		}
		return false;
	}

	public bool IsHorizontalPosValid(int x)
	{
		if (x >= 0)
		{
			return x < Width;
		}
		return false;
	}

	public bool IsVerticalPosValid(int y)
	{
		if (y >= 0)
		{
			return y < Height;
		}
		return false;
	}

	public void Deserialize(XContainer container = null)
	{
		CityTiles.Clear();
		if (container != null)
		{
			XElement xElement = (container as XDocument).Element("TileMap");
			if (!int.TryParse(xElement.Element("Width").Value, out var result))
			{
				TPSingleton<TileMapManager>.Instance.LogError("Could not parse Width value " + xElement.Element("Width").Value + " to a valid int.");
				return;
			}
			if (!int.TryParse(xElement.Element("Height").Value, out var result2))
			{
				TPSingleton<TileMapManager>.Instance.LogError("Could not parse Height value " + xElement.Element("Height").Value + " to a valid int.");
				return;
			}
			Width = result;
			Height = result2;
			Tiles = new Tile[Width * Height];
			foreach (XElement item in xElement.Element("Grounds").Elements())
			{
				GroundDefinition groundDefinition = TileDatabase.GroundDefinitions[item.Name.LocalName];
				string[] array = item.Value.Split('|');
				for (int num = array.Length - 1; num >= 0; num--)
				{
					string[] array2 = array[num].Split(',');
					if (!int.TryParse(array2[0], out var result3))
					{
						TPSingleton<TileMapManager>.Instance.LogError("Could not parse Ground " + item.Name.LocalName + " X coordinate value \"" + array2[0] + "\" to a valid int value.");
						continue;
					}
					if (!int.TryParse(array2[1], out var result4))
					{
						TPSingleton<TileMapManager>.Instance.LogError("Could not parse Ground " + item.Name.LocalName + " Y coordinate value \"" + array2[1] + "\" to a valid int value.");
						continue;
					}
					Tile tile = new TileController(result3, result4, groundDefinition).Tile;
					if (array2.Length >= 3)
					{
						if (!int.TryParse(array2[2], out var result5))
						{
							TPSingleton<TileMapManager>.Instance.LogError("Could not parse Ground " + item.Name.LocalName + " Haven Distance coordinate value \"" + array2[2] + "\" to a valid int value.");
							continue;
						}
						tile.DistanceToCity = result5;
					}
					else
					{
						TPSingleton<TileMapManager>.Instance.LogWarning("Invalid serialized tile format: Haven distance parameter is missing.");
					}
					if (array2.Length >= 4)
					{
						if (!int.TryParse(array2[3], out var result6))
						{
							TPSingleton<TileMapManager>.Instance.LogError("Could not parse Ground " + item.Name.LocalName + " MagicCircle Distance coordinate value \"" + array2[3] + "\" to a valid int value.");
							continue;
						}
						tile.DistanceToMagicCircle = result6;
					}
					else
					{
						TPSingleton<TileMapManager>.Instance.LogWarning("Invalid serialized tile format: MagicCircle distance parameter is missing.");
					}
					Tiles[result3 * Height + result4] = tile;
					if (tile.IsCityTile)
					{
						CityTiles.Add(tile);
					}
				}
			}
			XElement xElement2 = xElement.Element("Flags");
			if (xElement2 == null)
			{
				return;
			}
			{
				foreach (XElement item2 in xElement2.Elements())
				{
					TileFlagDefinition.E_TileFlagTag e_TileFlagTag = (TileFlagDefinition.E_TileFlagTag)Enum.Parse(typeof(TileFlagDefinition.E_TileFlagTag), item2.Name.LocalName);
					TilesWithFlag.Add(e_TileFlagTag, new List<Tile>());
					string[] array3 = item2.Value.Split('|');
					for (int num2 = array3.Length - 1; num2 >= 0; num2--)
					{
						string[] array4 = array3[num2].Split(',');
						int result8;
						if (!int.TryParse(array4[0], out var result7))
						{
							TPSingleton<TileMapManager>.Instance.LogError($"Could not parse Flag {e_TileFlagTag} X coordinate value \"{array4[0]}\" to a valid int value.");
						}
						else if (!int.TryParse(array4[1], out result8))
						{
							TPSingleton<TileMapManager>.Instance.LogError($"Could not parse Flag {e_TileFlagTag} Y coordinate value \"{array4[1]}\" to a valid int value.");
						}
						else
						{
							TilesWithFlag[e_TileFlagTag].Add(GetTile(result7, result8));
						}
					}
				}
				return;
			}
		}
		Width = TileMapManager.DefaultLevelWidth;
		Height = TileMapManager.DefaultLevelHeight;
		Tiles = new Tile[Width * Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Tile tile2 = new TileController(i, j, TileDatabase.GroundDefinitions["Dirt"]).Tile;
				Tiles[i * Height + j] = tile2;
				if (tile2.IsCityTile)
				{
					CityTiles.Add(tile2);
				}
			}
		}
	}
}
