using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Database.Fog;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.Apocalypse.LightFogSpawner;
using TheLastStand.Definition.Brazier;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.WorldMap;

public class CityDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public static class CityIds
		{
			public const string Swampfurt = "TutorialMap";

			public const string Gildenberg = "Felderland";

			public const string Lakeburg = "LakeBurg";

			public const string Glenwald = "Glenwald";

			public const string Elderlicht = "Elderlicht";

			public const string Glintfein = "Glintfein";

			public const string Runenberg = "GildenbergRedux";

			public const string Amberwald = "GlenwaldRedux";
		}

		public const string CityMetaUnlockName = "Unlock{0}City";

		public const float WorldMapZoomedCityOffsetX = 4.392857f;
	}

	public bool BlackenBackground { get; private set; }

	public string BonePilesEvolutionId { get; private set; }

	public BrazierDefinition BrazierDefinition { get; private set; }

	public Vector4 CameraBoundaries { get; private set; }

	public string Description => Localizer.Get("WorldMap_CityDescription_" + Id);

	public int DifficultySkullsNb { get; private set; }

	public int EnemiesProgressionOffset { get; private set; }

	public string FogDefinitionId { get; private set; }

	public List<SpawnDirectionsDefinition.E_Direction> ForbiddenDirections { get; private set; }

	public bool HasLinkedCity => !string.IsNullOrEmpty(LinkedCityId);

	public bool HasLinkedDLC => !string.IsNullOrEmpty(LinkedDLCId);

	public bool Hidden { get; private set; }

	public bool HideGoddesses { get; private set; }

	public string Id { get; private set; }

	public string InitResourceDefinitionId { get; private set; }

	public bool IsLastMap { get; private set; }

	public bool IsStoryMap { get; private set; }

	public bool IsTutorialMap { get; private set; }

	public string LevelLayoutBuildingsId { get; private set; }

	public string LevelLayoutTileMapId { get; private set; }

	public string LevelArtPrefabId { get; private set; }

	public LightFogSpawnersGenerationDefinition LightFogSpawnersGenerationDefinition { get; private set; }

	public string LinkedCityId { get; private set; }

	public string LinkedDLCId { get; private set; }

	public int MaxGlyphPoints { get; private set; }

	public string Name => Localizer.Get("WorldMap_CityName_" + Id);

	public int PanicRewardItemsOffset { get; private set; }

	public int PanicRewardResourcesOffset { get; private set; }

	public string PreGameCutscene { get; private set; }

	public string PostVictoryCutscene { get; private set; }

	public string RandomBuildingsPerDayDefinitionId { get; private set; }

	public string SectorContainerPrefabId { get; private set; }

	public string ShopEvolutionId { get; private set; }

	public string SpawnDefinitionId { get; private set; }

	public Game.E_DayTurn StartingDayTurn { get; private set; }

	public string StartingSetup { get; private set; }

	public string UnitGenerationDefinitionId { get; private set; }

	public string UnitGenerationGuaranteedRaceId { get; private set; }

	public bool UseCommanderAsMage { get; private set; }

	public int VictoryDaysCount { get; private set; }

	public Vector2 WorldMapPosition { get; private set; }

	public int WorldMapUIOrderIndex { get; private set; }

	public CityDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		CityDefinition cityDefinition = null;
		Hidden = xElement.Element("Hidden") != null;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("TemplateId");
		if (xAttribute2 != null)
		{
			cityDefinition = CityDatabase.CityDefinitions[xAttribute2.Value];
		}
		XElement xElement2 = xElement.Element("Gameplay");
		if (xElement2 == null && cityDefinition == null)
		{
			CLoggerManager.Log("CityDefinition " + Id + " must have a Gameplay element since it has no template.", LogType.Error);
			return;
		}
		XElement xElement3 = xElement.Element("View");
		if (xElement3 == null && cityDefinition == null)
		{
			CLoggerManager.Log("CityDefinition " + Id + " must have a View element since it has no template.", LogType.Error);
			return;
		}
		XElement xElement4 = xElement2?.Element("BrazierDefinitionId");
		if (xElement4 != null)
		{
			if (BuildingDatabase.BraziersDefinition.BrazierDefinitions.TryGetValue(xElement4.Value, out var value))
			{
				BrazierDefinition = value;
			}
			else
			{
				CLoggerManager.Log("Could not find Brazier Definition " + xElement4.Value + " in database.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "CityDefinition");
			}
		}
		else if (cityDefinition != null)
		{
			BrazierDefinition = cityDefinition.BrazierDefinition;
		}
		XElement xElement5 = xElement3.Element("DifficultySkullsNb");
		if (xElement5 != null)
		{
			if (int.TryParse(xElement5.Value, out var result))
			{
				DifficultySkullsNb = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse DifficultySkullNb element into an int in \"" + Id + "\"'s definition.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "CityDefinition");
			}
		}
		XElement xElement6 = xElement3.Element("IsStoryMap");
		if (xElement6 != null)
		{
			if (!bool.TryParse(xElement6.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse CityDefinition IsStoryMap value to a valid bool.");
				IsStoryMap = false;
			}
			else
			{
				IsStoryMap = result2;
			}
		}
		IsTutorialMap = xElement2?.Element("IsTutorialMap") != null;
		IsLastMap = xElement2.Element("IsLastMap") != null;
		XElement xElement7 = xElement3.Element("LinkedCityId");
		if (xElement7 != null)
		{
			LinkedCityId = xElement7.Value;
		}
		XElement xElement8 = xElement3.Element("LinkedDLCId");
		if (xElement8 != null)
		{
			LinkedDLCId = xElement8.Value;
		}
		XElement xElement9 = xElement2?.Element("EnemiesProgressionOffset");
		if (xElement9 != null)
		{
			if (int.TryParse(xElement9.Value, out var result3))
			{
				EnemiesProgressionOffset = result3;
			}
			else
			{
				CLoggerManager.Log("Could not parse EnemiesProgressionOffset element into an int in \"" + Id + "\"'s definition.", TPSingleton<WorldMapCityManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "WorldMapCityManager");
			}
		}
		XElement xElement10 = xElement2?.Element("FogId");
		if (xElement10.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no FogId and no Template to copy it from!", LogType.Error);
				return;
			}
			FogDefinitionId = cityDefinition.FogDefinitionId;
		}
		else
		{
			FogDefinitionId = xElement10.Value;
		}
		XElement xElement11 = xElement2?.Element("InitResourceId");
		if (xElement11.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no InitResourceDefinitionId and no Template to copy it from!", LogType.Error);
				return;
			}
			InitResourceDefinitionId = cityDefinition.InitResourceDefinitionId;
		}
		else
		{
			InitResourceDefinitionId = xElement11.Value;
		}
		XElement xElement12 = xElement3.Element("LevelLayoutIds");
		if (xElement12 != null)
		{
			XElement xElement13 = xElement12.Element("Buildings");
			LevelLayoutBuildingsId = ((!xElement13.IsNullOrEmpty()) ? xElement13.Value : Id);
			XElement xElement14 = xElement12.Element("TileMap");
			LevelLayoutTileMapId = ((!xElement14.IsNullOrEmpty()) ? xElement14.Value : Id);
			XElement xElement15 = xElement12.Element("LevelArtPrefabId");
			LevelArtPrefabId = ((!xElement15.IsNullOrEmpty()) ? xElement15.Value : Id);
		}
		else
		{
			LevelLayoutBuildingsId = Id;
			LevelLayoutTileMapId = Id;
			LevelArtPrefabId = Id;
		}
		XElement xElement16 = xElement2?.Element("LightFogSpawnersGenerationId");
		if (xElement16 != null && FogDatabase.LightFogDefinition.LightFogSpawnersGenerationDefinitions.TryGetValue(xElement16.Value, out var value2))
		{
			LightFogSpawnersGenerationDefinition = value2;
		}
		else
		{
			LightFogSpawnersGenerationDefinition = new LightFogSpawnersGenerationDefinition(null, FogDatabase.LightFogDefinition.TokenVariables);
		}
		XElement xElement17 = xElement2?.Element("MaxGlyphPoints");
		if (xElement17 != null)
		{
			if (int.TryParse(xElement17.Value, out var result4))
			{
				MaxGlyphPoints = result4;
			}
			else
			{
				CLoggerManager.Log("Could not parse MaxGlyphPoints element into an int in \"" + Id + "\"'s definition.", TPSingleton<WorldMapCityManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "WorldMapCityManager");
			}
		}
		else if (cityDefinition != null)
		{
			MaxGlyphPoints = cityDefinition.MaxGlyphPoints;
		}
		XElement xElement18 = xElement2?.Element("PanicRewardOffsets");
		if (xElement18 != null)
		{
			XElement xElement19 = xElement18.Element("Resources");
			if (xElement19 != null)
			{
				if (int.TryParse(xElement19.Value, out var result5))
				{
					PanicRewardResourcesOffset = result5;
				}
				else
				{
					CLoggerManager.Log("Could not parse PanicRewardOffsets Resources element into an int in \"" + Id + "\"'s definition.", TPSingleton<WorldMapCityManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "WorldMapCityManager");
				}
			}
			XElement xElement20 = xElement18.Element("Items");
			if (xElement20 != null)
			{
				if (int.TryParse(xElement20.Value, out var result6))
				{
					PanicRewardItemsOffset = result6;
				}
				else
				{
					CLoggerManager.Log("Could not parse PanicRewardOffsets Items element into an int in \"" + Id + "\"'s definition.", TPSingleton<WorldMapCityManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "WorldMapCityManager");
				}
			}
		}
		else if (cityDefinition != null)
		{
			PanicRewardResourcesOffset = cityDefinition.PanicRewardResourcesOffset;
			PanicRewardItemsOffset = cityDefinition.PanicRewardItemsOffset;
		}
		XElement xElement21 = xElement2?.Element("SectorsId");
		if (xElement21.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no SectorsId and no Template to copy it from!", LogType.Error);
				return;
			}
			SectorContainerPrefabId = cityDefinition.SectorContainerPrefabId;
		}
		else
		{
			SectorContainerPrefabId = xElement21.Value;
		}
		XElement xElement22 = xElement2?.Element("BonePilesEvolutionId");
		if (xElement22 != null)
		{
			BonePilesEvolutionId = xElement22.Value;
		}
		else if (cityDefinition != null)
		{
			BonePilesEvolutionId = cityDefinition.BonePilesEvolutionId;
		}
		else
		{
			CLoggerManager.Log("CityDefinition " + Id + " has no BonePilesEvolutionId and no Template to copy it from!", LogType.Error);
		}
		XElement xElement23 = xElement2?.Element("RandomBuildingsPerDayDefinitionId");
		if (xElement23 != null)
		{
			RandomBuildingsPerDayDefinitionId = xElement23.Value;
		}
		else if (cityDefinition != null)
		{
			RandomBuildingsPerDayDefinitionId = cityDefinition.RandomBuildingsPerDayDefinitionId;
		}
		XElement xElement24 = xElement2?.Element("ShopEvolutionId");
		if (xElement24 != null)
		{
			ShopEvolutionId = xElement24.Value;
		}
		else if (cityDefinition != null)
		{
			ShopEvolutionId = cityDefinition.ShopEvolutionId;
		}
		else
		{
			CLoggerManager.Log("CityDefinition " + Id + " has no ShopEvolutionId and no Template to copy it from!", LogType.Error);
		}
		XElement xElement25 = xElement2?.Element("Spawn");
		XAttribute xAttribute3 = xElement25.Attribute("Id");
		if (xAttribute3.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no SpawnId and no Template to copy it from!", LogType.Error);
				return;
			}
			SpawnDefinitionId = cityDefinition.SpawnDefinitionId;
		}
		else
		{
			SpawnDefinitionId = xAttribute3.Value;
		}
		ForbiddenDirections = new List<SpawnDirectionsDefinition.E_Direction>();
		XElement xElement26 = xElement25.Element("ForbiddenDirections");
		if (xElement26.IsNullOrEmpty())
		{
			if (cityDefinition != null)
			{
				ForbiddenDirections = cityDefinition.ForbiddenDirections;
			}
		}
		else
		{
			foreach (XElement item in xElement26.Elements("ForbiddenDirection"))
			{
				if (!Enum.TryParse<SpawnDirectionsDefinition.E_Direction>(item.Value, out var result7))
				{
					CLoggerManager.Log("Could not parse Forbidden Direction " + item.Value + " of city " + Id + " as a valid Direction!", LogType.Error);
				}
				else if (ForbiddenDirections.Contains(result7))
				{
					CLoggerManager.Log("Forbidden Direction " + item.Value + " of city " + Id + " is set twice!", LogType.Warning);
				}
				else
				{
					ForbiddenDirections.Add(result7);
				}
			}
		}
		XElement xElement27 = xElement2.Element("StartingDayTurn");
		Game.E_DayTurn result8;
		if (xElement27 == null)
		{
			StartingDayTurn = cityDefinition?.StartingDayTurn ?? Game.E_DayTurn.Deployment;
		}
		else if (Enum.TryParse<Game.E_DayTurn>(xElement27.Value, out result8))
		{
			StartingDayTurn = result8;
		}
		else
		{
			CLoggerManager.Log($"Could not parse StartingDayTurn element {xElement27.Value} of city {Id} as a valid DayTurn! Setting it to {Game.E_DayTurn.Deployment}.", LogType.Error);
			StartingDayTurn = Game.E_DayTurn.Deployment;
		}
		XElement xElement28 = xElement3.Element("StartingSetup");
		if (xElement28.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no StartingSetup and no Template to copy it from!", LogType.Error);
				return;
			}
			StartingSetup = cityDefinition.StartingSetup;
		}
		else
		{
			StartingSetup = xElement28.Value;
		}
		XElement xElement29 = xElement2.Element("UnitGenerationId");
		if (xElement29.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no UnitGenerationId and no Template to copy it from!", LogType.Error);
				return;
			}
			UnitGenerationDefinitionId = cityDefinition.UnitGenerationDefinitionId;
		}
		else
		{
			UnitGenerationDefinitionId = xElement29.Value;
		}
		XElement xElement30 = xElement2.Element("UnitGenerationGuaranteedRaceId");
		if (!xElement30.IsNullOrEmpty())
		{
			UnitGenerationGuaranteedRaceId = xElement30.Value;
		}
		XElement xElement31 = xElement2.Element("VictoryDaysCount");
		if (xElement31.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no VictoryDaysCount and no Template to copy it from!", LogType.Error);
				return;
			}
			VictoryDaysCount = cityDefinition.VictoryDaysCount;
		}
		else
		{
			if (!int.TryParse(xElement31.Value, out var result9))
			{
				CLoggerManager.Log("Could not parse VictoryDaysCount from CityDefinition " + Id + " value " + xElement31.Value + " to a valid integer value.", LogType.Error);
				return;
			}
			if (result9 < 1)
			{
				CLoggerManager.Log($"VictoryDaysCount in CityDefinition {Id} is less than 1 ({result9}) which is invalid, setting it to 1.", LogType.Warning);
				result9 = 1;
			}
			VictoryDaysCount = result9;
		}
		XElement xElement32 = xElement3.Element("WorldMapPosition");
		XAttribute xAttribute4 = xElement32.Attribute("X");
		XAttribute xAttribute5 = xElement32.Attribute("Y");
		if (float.TryParse(xAttribute4.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result10) && float.TryParse(xAttribute5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result11))
		{
			WorldMapPosition = new Vector2(result10, result11);
		}
		if (int.TryParse(xElement3.Element("WorldMapUIOrderIndex").Value, out var result12))
		{
			WorldMapUIOrderIndex = result12;
		}
		else
		{
			CLoggerManager.Log("Could not parse WorldMapUIOrderIndex element into an int in \"" + Id + "\"'s definition.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "CityDefinition");
		}
		CameraBoundaries = default(Vector4);
		XElement xElement33 = xElement3.Element("CameraBoundaries");
		if (xElement33.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log("CityDefinition " + Id + " has no CameraBoundaries and no Template to copy it from!", LogType.Error);
				return;
			}
			CameraBoundaries = cityDefinition.CameraBoundaries;
		}
		else
		{
			float result13 = 0f;
			float result14 = 0f;
			float result15 = 0f;
			float result16 = 0f;
			XElement xElement34 = xElement33.Element("Top");
			if (xElement34.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log("CityDefinition " + Id + " has no CameraBoundaries.Top and no Template to copy it from!", LogType.Error);
					return;
				}
				result13 = cityDefinition.CameraBoundaries.x;
			}
			else if (!float.TryParse(xElement34.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result13))
			{
				CLoggerManager.Log("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Top Element !", LogType.Error);
				return;
			}
			XElement xElement35 = xElement33.Element("Bottom");
			if (xElement35.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log("CityDefinition " + Id + " has no CameraBoundaries.Bottom and no Template to copy it from!", LogType.Error);
					return;
				}
				result14 = cityDefinition.CameraBoundaries.y;
			}
			else if (!float.TryParse(xElement35.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result14))
			{
				CLoggerManager.Log("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Bottom Element !", LogType.Error);
				return;
			}
			XElement xElement36 = xElement33.Element("Left");
			if (xElement36.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log("CityDefinition " + Id + " has no CameraBoundaries.Left and no Template to copy it from!", LogType.Error);
					return;
				}
				result15 = cityDefinition.CameraBoundaries.z;
			}
			else if (!float.TryParse(xElement36.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result15))
			{
				CLoggerManager.Log("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Left Element !", LogType.Error);
				return;
			}
			XElement xElement37 = xElement33.Element("Right");
			if (xElement37.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log("CityDefinition " + Id + " has no CameraBoundaries.Right and no Template to copy it from!", LogType.Error);
					return;
				}
				result16 = cityDefinition.CameraBoundaries.w;
			}
			else if (!float.TryParse(xElement37.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result16))
			{
				CLoggerManager.Log("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Right Element !", LogType.Error);
				return;
			}
			CameraBoundaries = new Vector4(result13, result14, result15, result16);
		}
		BlackenBackground = xElement3.Element("BlackenBackground") != null;
		XElement xElement38 = xElement3.Element("AnimatedCutscenes");
		if (!xElement38.IsNullOrEmpty())
		{
			PreGameCutscene = xElement38?.Element("PreGameCutscene")?.Value;
			PostVictoryCutscene = xElement38?.Element("PostVictoryCutscene")?.Value;
		}
		HideGoddesses = xElement3.Element("HideGoddesses") != null;
		UseCommanderAsMage = xElement3.Element("UseCommanderAsMage") != null;
	}
}
