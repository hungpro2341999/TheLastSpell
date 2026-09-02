using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.TileMap;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Generic;
using TheLastStand.View.LevelEditor;

namespace TheLastStand.Serialization.LevelEditor;

public static class LevelEditorSaverLoader
{
	[Flags]
	public enum E_ImpossibleSaveCause
	{
		None = 0,
		NoMagicCircle = 1,
		TooManyMagicCircles = 2,
		MissingSaveId = 4
	}

	public static string SaveFilePathPrefix => SaveFolderPath + "/" + LevelEditorManager.SaveId + "/" + LevelEditorManager.SaveId;

	public static string SaveFolderPath => "Assets/Resources/TextAssets/Cities/Level Editor";

	public static void Save()
	{
		TPSingleton<LevelEditorManager>.Instance.StartCoroutine(SaveCoroutine());
	}

	private static IEnumerator SaveCoroutine()
	{
		if (!IsSaveAllowed(out var impossibleSaveCause))
		{
			GenericPopUp.Open("Level Editor Save Error", $"Could not save Level. Failure cause(s) : {impossibleSaveCause}");
			yield break;
		}
		LevelEditorManager.OnSaveBegan();
		int tileIndex = TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Length - 1;
		while (tileIndex >= 0)
		{
			if (tileIndex % 30 == 0)
			{
				yield return SharedYields.WaitForEndOfFrame;
			}
			TileController tileController = TPSingleton<TileMapManager>.Instance.TileMap.Tiles[tileIndex].TileController;
			tileController.ComputeDistanceToCity();
			tileController.ComputeDistanceToMagicCircle();
			int num = tileIndex - 1;
			tileIndex = num;
		}
		string text = SaveFolderPath + "/" + LevelEditorManager.SaveId;
		if (Directory.Exists(text))
		{
			TPSingleton<LevelEditorManager>.Instance.Log("Deleting directory " + text + " to overwrite level data files.", CLogLevel.MAJOR);
			Directory.Delete(text, recursive: true);
		}
		else
		{
			TPSingleton<LevelEditorManager>.Instance.Log("Creating directory " + text + ".", CLogLevel.MAJOR);
		}
		Directory.CreateDirectory(text);
		SaveTiles();
		SaveBuildings();
		LevelEditorManager.OnSaveComplete();
		TPSingleton<LevelEditorManager>.Instance.Log("Successfully saved Level data files at path " + text + ".", CLogLevel.MAJOR, forcePrintInUnity: true);
	}

	public static void SaveBuildings()
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter writer = XmlWriter.Create(stringBuilder))
		{
			SerializeBuildings().WriteTo(writer);
		}
		using StreamWriter streamWriter = new StreamWriter(SaveFilePathPrefix + "_Buildings.xml");
		streamWriter.Write(stringBuilder.ToString());
		streamWriter.Close();
	}

	public static void SaveTiles()
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter writer = XmlWriter.Create(stringBuilder))
		{
			SerializeTiles().WriteTo(writer);
		}
		using StreamWriter streamWriter = new StreamWriter(SaveFilePathPrefix + "_TileMap.xml");
		streamWriter.Write(stringBuilder.ToString());
		streamWriter.Close();
	}

	private static bool IsSaveAllowed(out E_ImpossibleSaveCause impossibleSaveCause)
	{
		impossibleSaveCause = E_ImpossibleSaveCause.None;
		if (string.IsNullOrEmpty(LevelEditorManager.SaveId))
		{
			impossibleSaveCause |= E_ImpossibleSaveCause.MissingSaveId;
		}
		int num = 0;
		foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if (building is MagicCircle)
			{
				num++;
			}
		}
		if (num == 0)
		{
			impossibleSaveCause |= E_ImpossibleSaveCause.NoMagicCircle;
		}
		else if (num > 1)
		{
			impossibleSaveCause |= E_ImpossibleSaveCause.TooManyMagicCircles;
		}
		return impossibleSaveCause == E_ImpossibleSaveCause.None;
	}

	private static XContainer SerializeBuildings()
	{
		XDocument xDocument = new XDocument();
		XElement xElement = new XElement("Buildings");
		xDocument.Add(xElement);
		List<TheLastStand.Model.Building.Building> list = new List<TheLastStand.Model.Building.Building>(TPSingleton<BuildingManager>.Instance.Buildings);
		list.Sort((TheLastStand.Model.Building.Building a, TheLastStand.Model.Building.Building b) => a.BuildingDefinition.Id.CompareTo(b.BuildingDefinition.Id));
		foreach (TheLastStand.Model.Building.Building item in list)
		{
			XElement xElement2 = new XElement("Building");
			xElement2.Add(new XAttribute("Id", item.BuildingDefinition.Id));
			xElement2.Add(new XAttribute("X", item.OriginTile.X));
			xElement2.Add(new XAttribute("Y", item.OriginTile.Y));
			if (TPSingleton<BuildingsSettingsManager>.Instance.BuildingSettingsPanels.TryGetValue(item, out var value))
			{
				if (!value.BuildingSettingsHealth.IsFullHealth)
				{
					xElement2.Add(new XElement("Health", value.BuildingSettingsHealth.CurrentHealth));
				}
				if (value.BuildingSettingsUpgradeLevels.Count > 0 && value.BuildingSettingsUpgradeLevels.Any((BuildingSettingsUpgradeLevel o) => o.CurrentUpgradeLevel > 0))
				{
					XElement xElement3 = new XElement("UpgradesLevels");
					foreach (BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel in value.BuildingSettingsUpgradeLevels)
					{
						if (buildingSettingsUpgradeLevel.CurrentUpgradeLevel > 0)
						{
							xElement3.Add(new XElement(buildingSettingsUpgradeLevel.BuildingUpgradeDefinition.Id, buildingSettingsUpgradeLevel.CurrentUpgradeLevel));
						}
					}
					xElement2.Add(xElement3);
				}
			}
			xElement.Add(xElement2);
		}
		return xDocument;
	}

	private static XContainer SerializeTiles()
	{
		XDocument xDocument = new XDocument();
		XElement xElement = new XElement("TileMap");
		xDocument.Add(xElement);
		xElement.Add(new XElement("Width", TPSingleton<TileMapManager>.Instance.TileMap.Width));
		xElement.Add(new XElement("Height", TPSingleton<TileMapManager>.Instance.TileMap.Height));
		XElement xElement2 = new XElement("Grounds");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int num = TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Length - 1; num >= 0; num--)
		{
			Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.Tiles[num];
			if (!dictionary.ContainsKey(tile.GroundDefinition.Id))
			{
				dictionary.Add(tile.GroundDefinition.Id, string.Empty);
			}
			dictionary[tile.GroundDefinition.Id] += $"{tile.X},{tile.Y},{tile.DistanceToCity},{tile.DistanceToMagicCircle}|";
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			string content = item.Value[..^1];
			xElement2.Add(new XElement(item.Key, content));
		}
		if (xElement2.HasElements)
		{
			xElement.Add(xElement2);
		}
		XElement xElement3 = new XElement("Flags");
		foreach (KeyValuePair<TileFlagDefinition.E_TileFlagTag, List<Tile>> item2 in TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag)
		{
			string text = string.Empty;
			for (int num2 = item2.Value.Count - 1; num2 >= 0; num2--)
			{
				Tile tile2 = item2.Value[num2];
				text += string.Format("{0},{1}{2}", tile2.X, tile2.Y, (num2 > 0) ? "|" : string.Empty);
			}
			if (!string.IsNullOrEmpty(text))
			{
				xElement3.Add(new XElement(item2.Key.ToString(), text));
			}
		}
		if (xElement3.HasElements)
		{
			xElement.Add(xElement3);
		}
		return xDocument;
	}
}
