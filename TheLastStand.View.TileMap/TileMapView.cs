using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Fog;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.BonePile;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Fog;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Dev.View;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Building;
using TheLastStand.View.Building.Construction;
using TheLastStand.View.Camera;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.View.TileMap;

public class TileMapView : TPSingleton<TileMapView>
{
	public enum E_AreaOfEffectTileDisplayType
	{
		AreaOfEffect,
		Maneuver,
		Surrounding
	}

	public static class Constants
	{
		public static class Assets
		{
			public const string BuildingConstructionAnimationPath = "View/Sprites/ConstructionAnimation";

			public const string BuildingDestructionAnimationPath = "View/Sprites/DestructionAnimation";

			public const string BuildingLUTConstructionAnimationPath = "View/Sprites/LUTConstructionAnimation";

			public const string BuildingGhostPathPrefix = "View/Tiles/Buildings/Ghost";

			public const string BuildingDamagedTilePathPrefix = "View/Tiles/Buildings/Damaged Diffuse";

			public const string BuildingDamagedTilePathSuffix = "_DamagedDiffuse";

			public const string BuildingDamagedTileMaskPathPrefix = "View/Tiles/Buildings/Damaged Mask";

			public const string BuildingDamagedTileMaskPathSuffix = "_DamagedMask";

			public const string BuildingOutlinePathPrefix = "View/Tiles/Buildings/Outline";

			public const string BuildingSelectionFeedbackTilePath = "View/Tiles/Feedbacks/BuildingSelectionFeedback";

			public const string BuildingShadowsPathPrefix = "View/Tiles/Buildings/Diffuse/_Shadows";

			public const string BuildingTilePathPrefix = "View/Tiles/Buildings/Diffuse";

			public const string BuildingTileMaskPathPrefix = "View/Tiles/Buildings/Mask";

			public const string FogAreaTilePath = "View/Tiles/Feedbacks/MistRange/MistRange";

			public const string FogTilePath = "View/Tiles/World/Fog";

			public const string LightFogTilePath = "View/Tiles/World/LightFog";

			public const string LightFogDispelledTilePath = "View/Tiles/World/LightFog_Dispelled";

			public const string Ghost = "Ghost";

			public const string GridTilePath = "View/Tiles/Feedbacks/Grid Cell";

			public const string GroundTilePathPrefix = "View/Tiles/World";

			public const string GroundTileShapePath = "View/Tiles/World/TileShape";

			public const string LevelArtsPathFormat = "Prefab/Level Art/{0}/{0}_Level Art";

			public const string ReachableTilePath = "View/Tiles/Feedbacks/Movement/MoveRange";

			public const string ForbiddenBuildingTilePath = "View/Tiles/Feedbacks/Forbidden Building";

			public const string OccupationVolumeTilePath = "View/Tiles/Feedbacks/Occupation Volume";

			public const string OccupationVolumeGhostTilePath = "View/Tiles/Feedbacks/Occupation Volume Ghost";

			public const string OutlineSuffix = "_Outline";

			public const string PanicOnEnemyTilePath = "View/Tiles/Feedbacks/PanicOnEnemy";

			public const string PlaceholderDiffusePath = "View/Tiles/Buildings/Diffuse/Placeholder/Placeholder";

			public const string PoisonDeathFeedbackPath = "View/Tiles/Feedbacks/PoisonDeath";

			public const string SidewalkPathPrefix = "View/Tiles/Buildings/Diffuse/_Sidewalks";

			public const string SidewalkShadowPathPrefix = "View/Tiles/Buildings/Diffuse/_Sidewalks/_Shadows";

			public const string SkillAoeTilePath = "View/Tiles/Feedbacks/Skill/SkillAoe Back";

			public const string SkillInaccurateRangeTilePath = "View/Tiles/Feedbacks/Skill/InaccurateRange";

			public const string SkillManeuverTilePath = "View/Tiles/Feedbacks/Skill/SkillManeuver";

			public const string SkillRangeTilePath = "View/Tiles/Feedbacks/Skill/SkillRange";

			public const string SkillSurroundingTilePath = "View/Tiles/Feedbacks/Skill/SkillSurrounding";

			public const string DialsTileTopLinePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_top";

			public const string DialsTileBotLinePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_bot";

			public const string DialsTileLeftRightLinePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_LeftRight";

			public const string SkillFlipNorthSouthFeedbackOnTilePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_FlipSkill_NorthSouth_On";

			public const string SkillFlipEastWestFeedbackOnTilePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_FlipSkill_EastWest_On";

			public const string SkillRotationFeedbackOnTilePath = "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_RotationSkill_On";

			public const string LimitFogTilePath = "View/Tiles/Feedbacks/MistLimits/MistLimits";

			public const string WorldLimitTilePath = "View/Tiles/Feedbacks/WorldLimits/WorldLimits";
		}

		public static class PoolNames
		{
			public const string BuildingConstructionAnimation = "Building Construction Animation";

			public const string BuildingDestructionAnimation = "Building Destruction Animation";
		}

		public const string LevelArtNoneId = "None";
	}

	public class StringToBonePileIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(BonePileDatabase.BonePileGeneratorsDefinition.Buildings.Keys);
	}

	[SerializeField]
	private Grid grid;

	[SerializeField]
	private Transform levelArtContainer;

	[SerializeField]
	private int mapSizeReference = 51;

	[SerializeField]
	private float backgroundSizeRatio = 1.03f;

	[SerializeField]
	private Tilemap worldLimitTilemap;

	[SerializeField]
	private Transform levelBackground;

	[SerializeField]
	private SpriteRenderer levelBackgroundRenderer;

	[SerializeField]
	private Tilemap groundCityTilemap;

	[SerializeField]
	private Tilemap groundCraterTilemap;

	[SerializeField]
	private Tilemap groundBackgroundTilemap;

	[SerializeField]
	private Tilemap gridTilemap;

	[SerializeField]
	private Tilemap tilesFlagTilemapTemplate;

	[SerializeField]
	private Tilemap buildingSelectionFeedbackTilemap;

	[SerializeField]
	private Tilemap buildingTilemap;

	[SerializeField]
	private Tilemap buildingDamagedTilemap;

	[SerializeField]
	private Tilemap buildingDamagedMaskTilemap;

	[SerializeField]
	private Tilemap buildingFrontTilemap;

	[SerializeField]
	private Tilemap buildingMasksTilemap;

	[SerializeField]
	private Tilemap buildingFrontMasksTilemap;

	[SerializeField]
	private ConstructionAnimationView constructionAnimationViewPrefab;

	[SerializeField]
	private Tilemap occupationVolumeBuildingTilemap;

	[SerializeField]
	private Tilemap ghostBuildingsTilemap;

	[SerializeField]
	private Tilemap ghostBuildingsFrontTilemap;

	[SerializeField]
	private DestructionAnimationView destructionAnimationViewPrefab;

	[SerializeField]
	private Vector2 destructionAnimationRandomDelay = new Vector2(0f, 0.2f);

	[SerializeField]
	[Range(0f, 1f)]
	private float ghostTweenMaxAlpha = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float ghostTweenMinAlpha = 0.5f;

	[SerializeField]
	private float ghostTweenDuration = 1f;

	[SerializeField]
	private Ease ghostTweenEaseCurve = Ease.InOutSine;

	[SerializeField]
	private Tilemap buildingSelectionOutlinesTilemap;

	[SerializeField]
	private Tilemap buildingHoverOutlinesTilemap;

	[SerializeField]
	[Range(0f, 1f)]
	private float hoverOutlineTweenMaxAlpha = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float hoverOutlineTweenMinAlpha = 1f;

	[SerializeField]
	private float hoverOutlineTweenDuration = 1f;

	[SerializeField]
	private Ease hoverOutlineTweenEaseCurve = Ease.InOutSine;

	[SerializeField]
	private Tilemap buildingShadows;

	[SerializeField]
	private Tilemap sideWalksTilemap;

	[SerializeField]
	private Tilemap sideWalkShadowsTilemap;

	[SerializeField]
	private DataColor buildingValidColor;

	[SerializeField]
	private DataColor buildingInvalidColor;

	[SerializeField]
	private Tilemap reachableTilesTilemap;

	[SerializeField]
	private DataColor reachableTilesColor;

	[SerializeField]
	private Tilemap movePathTilemap;

	[SerializeField]
	private Tilemap dialsTileMap;

	[SerializeField]
	private Tilemap skillRotationFeedbackTileMap;

	[SerializeField]
	private DataColor inRangeDialsColor;

	[SerializeField]
	private DataColor inRangeInvalidOrientationDialsColor;

	[SerializeField]
	private DataColor outOfRangeDialsColor;

	[SerializeField]
	private DataColor outOfRangeInvalidOrientationDialsColor;

	[SerializeField]
	private Tilemap skillRangeTilemap;

	[SerializeField]
	private DataColor skillRangeTilesColor;

	[SerializeField]
	private DataColor skillRangeTilesColorInvalidOrientation;

	[SerializeField]
	private DataColor skillHiddenRangeTilesColor;

	[SerializeField]
	private DataColor skillHiddenRangeTilesColorInvalidOrientation;

	[SerializeField]
	private Tilemap skillInaccurateRangeTilemap;

	[SerializeField]
	private Tilemap areaOfEffectTilemap;

	[SerializeField]
	private Tilemap enemiesHoverAreaOfEffectTilemap;

	[SerializeField]
	private DataColor skillAoeValidColor;

	[SerializeField]
	private DataColor skillAoeInvalidColor;

	[SerializeField]
	private DataColor skillManeuverValidColor;

	[SerializeField]
	private DataColor skillManeuverInvalidColor;

	[SerializeField]
	private DataColor skillSurroundingValidColor;

	[SerializeField]
	private DataColor skillSurroundingInvalidColor;

	[SerializeField]
	private Transform rangedSkillsDodgeMultiplierContainer;

	[SerializeField]
	private Tilemap rangedSkillsDodgeMultiplierTemplate;

	[SerializeField]
	private GameObject hitPrefab;

	[SerializeField]
	private Transform boneZoneTilemapsContainer;

	[SerializeField]
	private Tilemap boneZoneTilemapTemplate;

	[SerializeField]
	private Tilemap havenAreaTilemap;

	[SerializeField]
	private Tilemap perkRangeTilemap;

	[SerializeField]
	private Tilemap perkHoverRangeSeparatorTemplate;

	[SerializeField]
	private Transform perkHoverRangeSeparatorContainer;

	[SerializeField]
	private Tilemap fogTilemap;

	[SerializeField]
	private Tilemap fogAreaTilemap;

	[SerializeField]
	private Tilemap lightFogOnTilemap;

	[SerializeField]
	private Tilemap lightFogOffTilemap;

	[SerializeField]
	private Tilemap fogMinMaxTilemap;

	[SerializeField]
	private TileBase fogMinMaxTileBase;

	[SerializeField]
	private DataColor fogOutlineColor;

	[SerializeField]
	private Tilemap fogLimitTilemap;

	[SerializeField]
	private Tilemap unitFeedbackTilemap;

	[SerializeField]
	private Tilemap enemiesReachableTilemap;

	[SerializeField]
	private Tilemap enemiesHoverTilemap;

	[SerializeField]
	private TileBase enemiesHoverTileBase;

	[SerializeField]
	private EnemyUnitDeadBodyView deadBodyPrefab;

	[SerializeField]
	private BuildingCorpseView deadBuildingPrefab;

	private Dictionary<Tilemap, Tween> alphaTweens = new Dictionary<Tilemap, Tween>();

	private HashSet<TheLastStand.Model.TileMap.Tile> enemiesReachableTiles = new HashSet<TheLastStand.Model.TileMap.Tile>();

	private Tween ghostFadeTween;

	private Tween hoverOutlineTween;

	private Tilemap[] rangedSkillsDodgeMultiplierTilemaps;

	private List<Tilemap> boneZoneTilemaps = new List<Tilemap>();

	private List<Tilemap> perkHoverRangeSeparatorTilemaps = new List<Tilemap>();

	private Dictionary<TileFlagDefinition.E_TileFlagTag, Tilemap> tilemapsByFlag;

	private Queue<int> activatedTileMaps = new Queue<int>();

	private HashSet<Vector3Int> fogMinMaxTiles;

	private bool levelArtLoaded;

	public static Tilemap AreaOfEffectTilemap => TPSingleton<TileMapView>.Instance.areaOfEffectTilemap;

	public static Tilemap EnemiesHoverAreaOfEffectTilemap => TPSingleton<TileMapView>.Instance.enemiesHoverAreaOfEffectTilemap;

	public static Tilemap BuildingTilemap => TPSingleton<TileMapView>.Instance.buildingTilemap;

	public static Tilemap BuildingFrontTilemap => TPSingleton<TileMapView>.Instance.buildingFrontTilemap;

	public static Tilemap BuildingFrontMasksTilemap => TPSingleton<TileMapView>.Instance.buildingFrontMasksTilemap;

	public static Tilemap BuildingSelectionOutlineTilemap => TPSingleton<TileMapView>.Instance.buildingSelectionOutlinesTilemap;

	public static Tilemap BuildingMasksTilemap => TPSingleton<TileMapView>.Instance.buildingMasksTilemap;

	public static Tilemap BuildingSelectionFeedbackTilemap => TPSingleton<TileMapView>.Instance.buildingSelectionFeedbackTilemap;

	public static Tilemap BuildingShadowsTilemap => TPSingleton<TileMapView>.Instance.buildingShadows;

	public static EnemyUnitDeadBodyView DeadBodyPrefab => TPSingleton<TileMapView>.Instance.deadBodyPrefab;

	public static BuildingCorpseView DeadBuildingPrefab => TPSingleton<TileMapView>.Instance.deadBuildingPrefab;

	public static Tilemap EnemiesReachableTilemap => TPSingleton<TileMapView>.Instance.enemiesReachableTilemap;

	public static Tilemap EnemiesHoverTilemap => TPSingleton<TileMapView>.Instance.enemiesHoverTilemap;

	public static TileBase EnemiesHoverTileBase => TPSingleton<TileMapView>.Instance.enemiesHoverTileBase;

	public static HashSet<TheLastStand.Model.TileMap.Tile> EnemiesReachableTiles => TPSingleton<TileMapView>.Instance.enemiesReachableTiles;

	public static Tilemap FogAreaTilemap => TPSingleton<TileMapView>.Instance.fogAreaTilemap;

	public static Tilemap FogTilemap => TPSingleton<TileMapView>.Instance.fogTilemap;

	public static Tilemap FogLimitTilemap => TPSingleton<TileMapView>.Instance.fogLimitTilemap;

	public static Grid Grid => TPSingleton<TileMapView>.Instance.grid;

	public static Tilemap LightFogOnTilemap => TPSingleton<TileMapView>.Instance.lightFogOnTilemap;

	public static Tilemap LightFogOffTilemap => TPSingleton<TileMapView>.Instance.lightFogOffTilemap;

	public static Tilemap FogMinMaxTilemap => TPSingleton<TileMapView>.Instance.fogMinMaxTilemap;

	public static Tilemap GhostBuildingsTilemap => TPSingleton<TileMapView>.Instance.ghostBuildingsTilemap;

	public static Tilemap GhostBuildingsFrontTilemap => TPSingleton<TileMapView>.Instance.ghostBuildingsFrontTilemap;

	public static Tilemap GridTilemap => TPSingleton<TileMapView>.Instance.gridTilemap;

	public static Tilemap GroundBackgroundTilemap => TPSingleton<TileMapView>.Instance.groundBackgroundTilemap;

	public static Tilemap GroundCityTilemap => TPSingleton<TileMapView>.Instance.groundCityTilemap;

	public static Tilemap GroundCraterTilemap => TPSingleton<TileMapView>.Instance.groundCraterTilemap;

	public static Tilemap MovePathTilemap => TPSingleton<TileMapView>.Instance.movePathTilemap;

	public static Tilemap OccupationVolumeBuildingTilemap => TPSingleton<TileMapView>.Instance.occupationVolumeBuildingTilemap;

	public static Tilemap ReachableTilesTilemap => TPSingleton<TileMapView>.Instance.reachableTilesTilemap;

	public static Tilemap SideWalksTilemap => TPSingleton<TileMapView>.Instance.sideWalksTilemap;

	public static Tilemap SideWalkShadowsTilemap => TPSingleton<TileMapView>.Instance.sideWalkShadowsTilemap;

	public static Tilemap SkillRangeTilemap => TPSingleton<TileMapView>.Instance.skillRangeTilemap;

	public static Tilemap SkillRotationFeedbackTileMap => TPSingleton<TileMapView>.Instance.skillRotationFeedbackTileMap;

	public static DataColor SkillRangeTilesColor => TPSingleton<TileMapView>.Instance.skillRangeTilesColor;

	public static DataColor SkillRangeTilesColorInvalidOrientation => TPSingleton<TileMapView>.Instance.skillRangeTilesColorInvalidOrientation;

	public static DataColor SkillHiddenRangeTilesColor => TPSingleton<TileMapView>.Instance.skillHiddenRangeTilesColor;

	public static DataColor SkillHiddenRangeTilesColorInvalidOrientation => TPSingleton<TileMapView>.Instance.skillHiddenRangeTilesColorInvalidOrientation;

	public static Tilemap UnitFeedbackTilemap => TPSingleton<TileMapView>.Instance.unitFeedbackTilemap;

	public static Tilemap WorldLimitsTilemap => TPSingleton<TileMapView>.Instance.worldLimitTilemap;

	public static Dictionary<TileFlagDefinition.E_TileFlagTag, Tilemap> TilemapsByFlag
	{
		get
		{
			if (TPSingleton<TileMapView>.Instance.tilemapsByFlag == null)
			{
				TPSingleton<TileMapView>.Instance.tilemapsByFlag = new Dictionary<TileFlagDefinition.E_TileFlagTag, Tilemap>();
				TileFlagDefinition[] tileFlagDefinitions = TileMapManager.TileFlagDefinitions;
				foreach (TileFlagDefinition tileFlagDefinition in tileFlagDefinitions)
				{
					Tilemap tilemap = UnityEngine.Object.Instantiate(TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate, TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate.transform.parent);
					tilemap.transform.name = tilemap.transform.name.Replace("(Clone)", $" ({tileFlagDefinition.TileFlagTag})");
					TPSingleton<TileMapView>.Instance.tilemapsByFlag.Add(tileFlagDefinition.TileFlagTag, tilemap);
				}
			}
			TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate.gameObject.SetActive(value: false);
			return TPSingleton<TileMapView>.Instance.tilemapsByFlag;
		}
		private set
		{
			TPSingleton<TileMapView>.Instance.tilemapsByFlag = value;
		}
	}

	public Vector2 DestructionAnimationRandomDelay => destructionAnimationRandomDelay;

	public static void ClearTiles(Tilemap tileMap)
	{
		tileMap.ClearAllTiles();
	}

	public static void DisplayLevel(bool displayInCoroutine = false)
	{
		TPSingleton<TileMapView>.Instance.LoadTileAssets();
		GroundBackgroundTilemap.ClearAllTiles();
		GroundCityTilemap.ClearAllTiles();
		GroundCraterTilemap.ClearAllTiles();
		GridTilemap.ClearAllTiles();
		BuildingTilemap.ClearAllTiles();
		TPSingleton<TileMapView>.Instance.buildingDamagedTilemap.ClearAllTiles();
		TPSingleton<TileMapView>.Instance.buildingDamagedMaskTilemap.ClearAllTiles();
		BuildingFrontTilemap.ClearAllTiles();
		BuildingMasksTilemap.ClearAllTiles();
		BuildingFrontMasksTilemap.ClearAllTiles();
		TPSingleton<TileMapView>.Instance.tilesFlagTilemapTemplate.ClearAllTiles();
		TransformExtensions.DestroyChildren(TPSingleton<TileMapView>.Instance.levelArtContainer);
		if (displayInCoroutine)
		{
			TPSingleton<TileMapView>.Instance.StartCoroutine(TPSingleton<TileMapView>.Instance.DisplayLevelCoroutine());
			return;
		}
		bool flag = ApplicationManager.Application.State.GetName() == "LevelEditor";
		CityDefinition cityDefinition = (flag ? null : TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition);
		string text = cityDefinition?.Id ?? LevelEditorManager.CityToLoadId;
		TPSingleton<TileMapView>.Instance.levelArtLoaded = false;
		string text2 = cityDefinition?.LevelArtPrefabId ?? LevelEditorManager.CityToLoadId;
		if (text2 != "None")
		{
			GameObject gameObject = ResourcePooler.LoadOnce<GameObject>(string.Format("Prefab/Level Art/{0}/{0}_Level Art", text2));
			if (gameObject != null)
			{
				UnityEngine.Object.Instantiate(gameObject, TPSingleton<TileMapView>.Instance.levelArtContainer);
				TPSingleton<TileMapView>.Instance.levelArtLoaded = true;
			}
			else
			{
				TPSingleton<TileMapManager>.Instance.LogWarning("No Level Art prefab has been found for city Id " + text2 + ". This could be due to loading template city inside LevelEditor, but if it's not the case, then there's something going wrong.", CLogLevel.MAJOR);
			}
		}
		else
		{
			TPSingleton<TileMapManager>.Instance.Log("Level art Id set to None, then no level art is being loaded.", CLogLevel.MAJOR);
		}
		for (int i = -1; i <= TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			for (int j = -1; j <= TPSingleton<TileMapManager>.Instance.TileMap.Height; j++)
			{
				TPSingleton<TileMapView>.Instance.SetWorldLimitTile(new Vector3Int(i, j, 0));
				if (i != -1 && j != -1 && i != TPSingleton<TileMapManager>.Instance.TileMap.Width && j != TPSingleton<TileMapManager>.Instance.TileMap.Height)
				{
					TheLastStand.Model.TileMap.Tile tile = TileMapManager.GetTile(i, j);
					SetTile(GridTilemap, tile, "View/Tiles/Feedbacks/Grid Cell");
					TileBase tileBase = null;
					if (TPSingleton<TileMapView>.Instance.levelArtLoaded)
					{
						tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/TileShape");
					}
					else
					{
						tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id + "_" + text) ?? ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id);
						TileBase tileBase2 = ResourcePooler.LoadOnce<TileBase>("View/Tiles/World/Ground_" + text) ?? ResourcePooler.LoadOnce<TileBase>("View/Tiles/World/Ground");
						SetTile(GroundBackgroundTilemap, tile, tileBase2);
						GroundCityTilemap.color = Color.white;
						GroundCraterTilemap.color = Color.white;
					}
					switch (tile.GroundDefinition.GroundCategory)
					{
					case GroundDefinition.E_GroundCategory.City:
						GroundCityTilemap.SetTile(new Vector3Int(i, j, 0), tileBase);
						break;
					case GroundDefinition.E_GroundCategory.NoBuilding:
						GroundCraterTilemap.SetTile(new Vector3Int(i, j, 0), tileBase);
						break;
					}
					if (tile.Building != null && tile.Building.OriginTile == tile)
					{
						TPSingleton<TileMapView>.Instance.DisplayBuilding(tile.Building, tile);
					}
				}
			}
		}
		bool blackenBackground = cityDefinition?.BlackenBackground ?? false;
		TPSingleton<TileMapView>.Instance.UpdateBackground(blackenBackground);
		if (!flag)
		{
			TPSingleton<TileMapView>.Instance.SetGroundTilemapsAlpha(0f);
		}
	}

	public static Vector3 GetCellCenterWorldPosition(TheLastStand.Model.TileMap.Tile tile)
	{
		return GetCellCenterWorldPosition(tile.Position);
	}

	public static Vector3 GetCellCenterWorldPosition(Vector2Int tilePosition)
	{
		return GridTilemap.GetCellCenterWorld(new Vector3Int(tilePosition.x, tilePosition.y, 0));
	}

	public static Vector3 GetLocalInterpolatedPosition(Vector3 localTilePosition)
	{
		return GridTilemap.CellToLocalInterpolated(localTilePosition);
	}

	public static string GetSkillFlipIconPathFromOrientation(TileObjectSelectionManager.E_Orientation orientation)
	{
		if (orientation == TileObjectSelectionManager.E_Orientation.NORTH || orientation == TileObjectSelectionManager.E_Orientation.SOUTH)
		{
			return "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_FlipSkill_NorthSouth_On";
		}
		return "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_FlipSkill_EastWest_On";
	}

	public static Vector2 GetTileCenter(TheLastStand.Model.TileMap.Tile tile)
	{
		return GetCellCenterWorldPosition(tile) + new Vector3(0f, TPSingleton<TileMapView>.Instance.grid.cellSize.y * 0.5f, 0f);
	}

	public static Vector3 GetWorldPosition(TheLastStand.Model.TileMap.Tile tile)
	{
		return GetWorldPosition(tile.Position);
	}

	public static Vector3 GetWorldPosition(Vector2Int tilePosition)
	{
		return GridTilemap.CellToWorld(new Vector3Int(tilePosition.x, tilePosition.y, 0));
	}

	public static Vector3 GetCameraCenterTilePosition()
	{
		Vector3 position = ACameraView.MainCam.transform.position;
		Vector3Int vector3Int = GridTilemap.WorldToCell(new Vector3Int((int)position.x, (int)position.y, 0));
		return GetCellCenterWorldPosition(new Vector2Int(vector3Int.x, vector3Int.y));
	}

	public static void SetFogOutlinesTileBases()
	{
		TileBase tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/Fog");
		TileBase[] tileArray = new TileBase[4] { tileBase, tileBase, tileBase, tileBase };
		for (int i = -1; i <= TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			Vector3Int[] positionArray = new Vector3Int[4]
			{
				new Vector3Int(i, -1, 0),
				new Vector3Int(-1, i, 0),
				new Vector3Int(i, TPSingleton<TileMapManager>.Instance.TileMap.Width, 0),
				new Vector3Int(TPSingleton<TileMapManager>.Instance.TileMap.Width, i, 0)
			};
			FogTilemap.SetTiles(positionArray, tileArray);
		}
		for (int j = -2; j <= TPSingleton<TileMapManager>.Instance.TileMap.Width + 1; j++)
		{
			Vector3Int[] array = new Vector3Int[4]
			{
				new Vector3Int(j, -2, 0),
				new Vector3Int(-2, j, 0),
				new Vector3Int(j, TPSingleton<TileMapManager>.Instance.TileMap.Width + 1, 0),
				new Vector3Int(TPSingleton<TileMapManager>.Instance.TileMap.Width + 1, j, 0)
			};
			FogTilemap.SetTiles(array, tileArray);
			for (int k = 0; k < array.Length; k++)
			{
				FogTilemap.SetColor(array[k], TPSingleton<TileMapView>.Instance.fogOutlineColor._Color);
			}
		}
	}

	public static void SetTile(Tilemap tileMap, TheLastStand.Model.TileMap.Tile tile, string tileBasePath, string backupTileBasePath = null)
	{
		TileBase tileBase = ((tileBasePath != null) ? ResourcePooler<TileBase>.LoadOnce(tileBasePath) : null);
		if (tileBase == null)
		{
			tileBase = ((backupTileBasePath != null) ? ResourcePooler<TileBase>.LoadOnce(backupTileBasePath) : null);
		}
		SetTile(tileMap, tile, tileBase);
	}

	public static void SetTile(Tilemap tileMap, TheLastStand.Model.TileMap.Tile tile, TileBase tileBase = null)
	{
		tileMap.SetTile((Vector3Int)tile.Position, tileBase);
	}

	public static void SetTiles(Tilemap tileMap, List<TheLastStand.Model.TileMap.Tile> tiles, string tileBasePath)
	{
		TileBase tileBase = ((tileBasePath != null) ? ResourcePooler<TileBase>.LoadOnce(tileBasePath) : null);
		SetTiles(tileMap, tiles, tileBase);
	}

	public static void SetTiles(Tilemap tileMap, List<TheLastStand.Model.TileMap.Tile> tiles, TileBase tileBase = null)
	{
		Vector3Int[] array = new Vector3Int[tiles.Count];
		TileBase[] array2 = new TileBase[tiles.Count];
		for (int i = 0; i < tiles.Count; i++)
		{
			array[i] = (Vector3Int)tiles[i].Position;
			array2[i] = tileBase;
		}
		tileMap.SetTiles(array, array2);
	}

	public static void SetTiles(Tilemap tileMap, HashSet<Vector3Int> positions, TileBase tileBase = null)
	{
		int count = positions.Count;
		Vector3Int[] positionArray = positions.ToArray();
		TileBase[] array = new TileBase[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = tileBase;
		}
		tileMap.SetTiles(positionArray, array);
	}

	public static void SetTileColor(Tilemap tileMap, TheLastStand.Model.TileMap.Tile tile, Color color)
	{
		tileMap.SetColor((Vector3Int)tile.Position, color);
	}

	public static void SpawnConstructionAnimation(Vector3 worldPosition, Sprite[] sprites, int sortingOrder, int animationFrameRate, int shockwaveFrame, Sprite[] spritesLUT = null, TheLastStand.Model.Building.Building building = null)
	{
		ConstructionAnimationView pooledComponent = ObjectPooler.GetPooledComponent("Building Construction Animation", TPSingleton<TileMapView>.Instance.constructionAnimationViewPrefab);
		if (building != null)
		{
			pooledComponent.ChangeBuilding(building);
		}
		pooledComponent.transform.position = worldPosition;
		pooledComponent.Init(sortingOrder, sprites, animationFrameRate, shockwaveFrame, spritesLUT);
		pooledComponent.PlayConstructionAnimation();
	}

	public static void SpawnDestructionAnimation(TheLastStand.Model.Building.Building building, TheLastStand.Model.TileMap.Tile tile, float delay)
	{
		Vector3 worldPosition = BuildingTilemap.CellToWorld((Vector3Int)tile.Position);
		Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(tile);
		Sprite[] array = ResourcePooler.LoadAllOnce<Sprite>(string.Format("{0}/{1}/{2}{3}", "View/Sprites/DestructionAnimation", building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y));
		if (array != null && array.Length != 0)
		{
			DestructionAnimationView pooledComponent = ObjectPooler.GetPooledComponent("Building Destruction Animation", TPSingleton<TileMapView>.Instance.destructionAnimationViewPrefab);
			pooledComponent.ChangeBuilding(building);
			pooledComponent.Init(worldPosition, array, delay);
			pooledComponent.PlayDestructionAnimation();
		}
	}

	public void AddEnemyReachableTiles(List<TheLastStand.Model.TileMap.Tile> tiles)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			TheLastStand.Model.TileMap.Tile item = tiles[i];
			enemiesReachableTiles.Add(item);
		}
	}

	public void ChangeBuildingGhostTileMapsColor(bool isValid)
	{
		Color color = (isValid ? buildingValidColor._Color : buildingInvalidColor._Color);
		color.a = GhostBuildingsTilemap.color.a;
		GhostBuildingsTilemap.color = color;
		GhostBuildingsFrontTilemap.color = color;
	}

	public void ClearAllEnemiesReachableTiles()
	{
		List<Vector3Int> list = new List<Vector3Int>(enemiesReachableTiles.Count);
		List<TileBase> list2 = new List<TileBase>(enemiesReachableTiles.Count);
		foreach (TheLastStand.Model.TileMap.Tile enemiesReachableTile in enemiesReachableTiles)
		{
			list.Add((Vector3Int)enemiesReachableTile.Position);
			list2.Add(null);
		}
		EnemiesReachableTilemap.SetTiles(list.ToArray(), list2.ToArray());
		enemiesReachableTiles.Clear();
	}

	public void ClearBuilding(TheLastStand.Model.TileMap.Tile tile)
	{
		SetTile(BuildingTilemap, tile);
		SetTile(buildingDamagedTilemap, tile);
		SetTile(buildingDamagedMaskTilemap, tile);
		SetTile(BuildingFrontTilemap, tile);
		SetTile(BuildingMasksTilemap, tile);
		SetTile(BuildingFrontMasksTilemap, tile);
		SetTile(BuildingShadowsTilemap, tile);
		SetTile(SideWalksTilemap, tile);
		SetTile(SideWalkShadowsTilemap, tile);
	}

	public void ClearBuildingGhost(TheLastStand.Model.TileMap.Tile tile, BuildingDefinition buildingDefinition)
	{
		List<TheLastStand.Model.TileMap.Tile> occupiedTiles = tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int num = occupiedTiles.Count - 1; num >= 0; num--)
		{
			SetTile(GhostBuildingsTilemap, occupiedTiles[num]);
			SetTile(GhostBuildingsFrontTilemap, occupiedTiles[num]);
			if (buildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent && TileMapController.CanPlaceBuilding(buildingDefinition, tile, ignoreUnit: true))
			{
				int buildingDeadZoneRange = BuildingManager.GetBuildingDeadZoneRange(buildingDefinition.Id);
				for (int i = -buildingDeadZoneRange; i <= buildingDeadZoneRange; i++)
				{
					for (int j = -buildingDeadZoneRange; j <= buildingDeadZoneRange; j++)
					{
						TheLastStand.Model.TileMap.Tile tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(occupiedTiles[num].Position.x + i, occupiedTiles[num].Position.y + j);
						if (tile2 != null && !occupiedTiles.Contains(tile2) && TPSingleton<ConstructionManager>.Instance.Construction.BuildingAvailableSpaceTiles.Contains(tile2))
						{
							SetTile(OccupationVolumeBuildingTilemap, tile2, "View/Tiles/Feedbacks/Occupation Volume");
						}
					}
				}
			}
		}
		ClearBuildingGhostRangeAndZoneTiles();
	}

	public void ClearDialsTiles(TheLastStand.Model.TileMap.Tile sourceTile)
	{
		if (sourceTile == null)
		{
			return;
		}
		Vector2Int zero = Vector2Int.zero;
		for (int i = 0; i < TPSingleton<TileMapManager>.Instance.TileMap.Width; i++)
		{
			zero.x = i;
			if (i != sourceTile.Position.x)
			{
				zero.y = sourceTile.Position.y + (sourceTile.Position.x - i);
				dialsTileMap.SetTile((Vector3Int)zero, null);
				zero.y = sourceTile.Position.y - (sourceTile.Position.x - i);
				dialsTileMap.SetTile((Vector3Int)zero, null);
			}
		}
	}

	public void ClearHoverOutline(TheLastStand.Model.TileMap.Tile tile, BuildingDefinition buildingDefinition)
	{
		List<TheLastStand.Model.TileMap.Tile> occupiedTiles = tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int i = 0; i < occupiedTiles.Count; i++)
		{
			buildingHoverOutlinesTilemap.SetTile((Vector3Int)occupiedTiles[i].Position, null);
		}
	}

	public void ClearInaccurateRangeTiles(IEnumerable<TheLastStand.Model.TileMap.Tile> tiles)
	{
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count());
		List<TileBase> list2 = new List<TileBase>(tiles.Count());
		foreach (TheLastStand.Model.TileMap.Tile tile in tiles)
		{
			list.Add((Vector3Int)tile.Position);
			list2.Add(null);
		}
		skillInaccurateRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void ClearPerkHoverRangeTiles()
	{
		ClearTiles(perkRangeTilemap);
		for (int i = 0; i < perkHoverRangeSeparatorTilemaps.Count; i++)
		{
			ClearTiles(perkHoverRangeSeparatorTilemaps[i]);
		}
		ClearTiles(havenAreaTilemap);
	}

	public void ClearRangedSkillsModifiers()
	{
		for (int num = rangedSkillsDodgeMultiplierTilemaps.Length - 1; num >= 0; num--)
		{
			ClearTiles(rangedSkillsDodgeMultiplierTilemaps[num]);
		}
	}

	public void GenerateBoneZoneTilemaps()
	{
		List<BoneZoneDefinition> boneZoneDefinitions = BonePileDatabase.BoneZonesDefinition.BoneZoneDefinitions;
		TileBase tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/TileShape");
		Color colorA = Color.red;
		Color colorB = Color.white;
		int i = 0;
		int previousMinDistanceFromCity = int.MaxValue;
		int previousMaxMagicCircleDistance = -1;
		HashSet<TheLastStand.Model.TileMap.Tile> usedTiles = new HashSet<TheLastStand.Model.TileMap.Tile>();
		foreach (BoneZoneDefinition boneZoneDefinition in boneZoneDefinitions)
		{
			if (boneZoneDefinition.MinHavenDistance > -1)
			{
				SetBoneZoneTiles(boneZoneDefinition, (TheLastStand.Model.TileMap.Tile o) => o.DistanceToCity >= boneZoneDefinition.MinHavenDistance && o.DistanceToCity < previousMinDistanceFromCity && !usedTiles.Contains(o));
				previousMinDistanceFromCity = boneZoneDefinition.MinHavenDistance;
			}
			else if (boneZoneDefinition.MaxMagicCircleDistance > -1)
			{
				SetBoneZoneTiles(boneZoneDefinition, (TheLastStand.Model.TileMap.Tile o) => o.DistanceToMagicCircle <= boneZoneDefinition.MaxMagicCircleDistance && o.DistanceToMagicCircle > previousMaxMagicCircleDistance && !usedTiles.Contains(o));
				previousMaxMagicCircleDistance = boneZoneDefinition.MaxMagicCircleDistance;
			}
			int num = i;
			i = num + 1;
		}
		boneZoneTilemapTemplate.gameObject.SetActive(value: false);
		void SetBoneZoneTiles(BoneZoneDefinition boneZoneDefinition2, Func<TheLastStand.Model.TileMap.Tile, bool> match)
		{
			Tilemap tilemap = UnityEngine.Object.Instantiate(TPSingleton<TileMapView>.Instance.boneZoneTilemapTemplate, TPSingleton<TileMapView>.Instance.boneZoneTilemapsContainer);
			tilemap.name = "BoneZone_" + boneZoneDefinition2.Id;
			tilemap.ClearAllTiles();
			tilemap.color = Color.LerpUnclamped(colorA, colorB, (float)i / (float)boneZoneDefinitions.Count).WithA(0f);
			foreach (TheLastStand.Model.TileMap.Tile item in TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Where(match))
			{
				tilemap.SetTile((Vector3Int)item.Position, tileBase);
				usedTiles.Add(item);
			}
			TPSingleton<TileMapView>.Instance.boneZoneTilemaps.Add(tilemap);
		}
	}

	public void ClearRangeTiles(Dictionary<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tiles)
	{
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count);
		List<TileBase> list2 = new List<TileBase>(tiles.Count);
		foreach (KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tile in tiles)
		{
			list.Add((Vector3Int)tile.Key.Position);
			list2.Add(null);
		}
		SkillRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void ClearSelectionOutline(TheLastStand.Model.TileMap.Tile tile, BuildingDefinition buildingDefinition)
	{
		List<TheLastStand.Model.TileMap.Tile> occupiedTiles = tile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int i = 0; i < occupiedTiles.Count; i++)
		{
			buildingSelectionOutlinesTilemap.SetTile((Vector3Int)occupiedTiles[i].Position, null);
		}
	}

	public void Display()
	{
		while (activatedTileMaps.Count > 0)
		{
			int index = activatedTileMaps.Dequeue();
			base.transform.GetChild(index).gameObject.SetActive(value: true);
		}
	}

	public void DisplayAllEnemiesReachableTiles()
	{
		TileBase item = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Movement/MoveRange");
		List<Vector3Int> list = new List<Vector3Int>(enemiesReachableTiles.Count);
		List<TileBase> list2 = new List<TileBase>(enemiesReachableTiles.Count);
		foreach (TheLastStand.Model.TileMap.Tile enemiesReachableTile in enemiesReachableTiles)
		{
			list.Add((Vector3Int)enemiesReachableTile.Position);
			list2.Add(item);
		}
		EnemiesReachableTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void DisplayAreaOfEffectHitFeedback(TheLastStand.Model.TileMap.Tile tile)
	{
		UnityEngine.Object.Instantiate(hitPrefab).transform.position = GetWorldPosition(tile);
	}

	public void DisplayAreaOfEffectTile(TheLastStand.Model.TileMap.Tile tile, E_AreaOfEffectTileDisplayType areaOfEffectTileDisplayType, bool unreachable, Tilemap tilemap)
	{
		if (tile != null)
		{
			TileBase tileBase = null;
			Color color = skillAoeValidColor._Color;
			Color color2 = skillAoeInvalidColor._Color;
			switch (areaOfEffectTileDisplayType)
			{
			case E_AreaOfEffectTileDisplayType.AreaOfEffect:
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillAoe Back");
				color = skillAoeValidColor._Color;
				color2 = skillAoeInvalidColor._Color;
				break;
			case E_AreaOfEffectTileDisplayType.Maneuver:
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillManeuver");
				color = skillManeuverValidColor._Color;
				color2 = skillManeuverInvalidColor._Color;
				break;
			case E_AreaOfEffectTileDisplayType.Surrounding:
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillSurrounding");
				color = skillSurroundingValidColor._Color;
				color2 = skillSurroundingInvalidColor._Color;
				break;
			}
			SetTile(tilemap, tile, tileBase);
			SetTileColor(tilemap, tile, unreachable ? color2 : color);
		}
	}

	public void DisplayBuilding(TheLastStand.Model.Building.Building building, TheLastStand.Model.TileMap.Tile baseTile, string suffix = "")
	{
		switch (building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationType)
		{
		case BuildingDefinition.E_ConstructionAnimationType.Instantaneous:
			DisplayBuildingInstantly(building, baseTile, suffix);
			break;
		case BuildingDefinition.E_ConstructionAnimationType.Animated:
			DisplayBuildingAnimated(building, baseTile, suffix);
			break;
		default:
			DisplayBuildingInstantly(building, baseTile, suffix);
			break;
		}
	}

	public void DisplayBuildingAnimated(TheLastStand.Model.Building.Building building, TheLastStand.Model.TileMap.Tile baseTile, string suffix = "")
	{
		int animationSpritesCount = 0;
		for (int i = 0; i < building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(building.BlueprintModule.OccupiedTiles[i]);
			Vector3Int vector3Int = new Vector3Int(baseTile.X + relativeBuildingTilePosition.x - building.BuildingDefinition.BlueprintModuleDefinition.OriginX, baseTile.Y + relativeBuildingTilePosition.y - building.BuildingDefinition.BlueprintModuleDefinition.OriginY, 0);
			Sprite[] array = ResourcePooler<Sprite>.LoadAllOnce(string.Format("{0}/{1}/{2}{3}", "View/Sprites/ConstructionAnimation", building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y));
			Sprite[] spritesLUT = ResourcePooler<Sprite>.LoadAllOnce(string.Format("{0}/{1}/{2}{3}", "View/Sprites/LUTConstructionAnimation", building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y), failSilently: true);
			if (array.Length != 0)
			{
				animationSpritesCount = array.Length;
				SpawnConstructionAnimation(BuildingTilemap.CellToWorld(vector3Int), array, BuildingTilemap.GetComponent<TilemapRenderer>().sortingOrder, building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationFrameRate, building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationShockwaveFrame, spritesLUT, building);
			}
			TileBase tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.ShadowType);
			if (tileBase != null)
			{
				BuildingShadowsTilemap.SetTile(vector3Int, tileBase);
			}
			if (building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType != "None")
			{
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType);
				if (tileBase != null)
				{
					SideWalksTilemap.SetTile(vector3Int, tileBase);
				}
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType + "Shadow");
				if (tileBase != null)
				{
					SideWalkShadowsTilemap.SetTile(vector3Int, tileBase);
				}
			}
		}
		building.BuildingView.transform.position = GetWorldPosition(baseTile);
		building.BuildingView.gameObject.SetActive(value: true);
		building.BuildingView.PlaceBuildingTilesAfterConstructionAnimation(baseTile, animationSpritesCount, building.BuildingDefinition.ConstructionModuleDefinition.ConstructionAnimationFrameRate, suffix);
	}

	public void DisplayBuildingGhost(BuildingDefinition buildingDefinition, TheLastStand.Model.TileMap.Tile originTile)
	{
		List<TheLastStand.Model.TileMap.Tile> occupiedTiles = originTile.GetOccupiedTiles(buildingDefinition.BlueprintModuleDefinition);
		for (int num = occupiedTiles.Count - 1; num >= 0; num--)
		{
			Vector2Int relativeBuildingTilePosition = BlueprintModule.GetRelativeBuildingTilePosition(occupiedTiles[num], originTile, buildingDefinition.BlueprintModuleDefinition);
			Vector3Int position = new Vector3Int(originTile.X + relativeBuildingTilePosition.x - buildingDefinition.BlueprintModuleDefinition.OriginX, originTile.Y + relativeBuildingTilePosition.y - buildingDefinition.BlueprintModuleDefinition.OriginY, 0);
			TileBase ghostTileBase = BuildingView.GetGhostTileBase(buildingDefinition.Id, string.Empty, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y);
			if (ghostTileBase != null)
			{
				GhostBuildingsTilemap.SetTile(position, ghostTileBase);
			}
			if (buildingDefinition.ConstructionModuleDefinition.OccupationVolumeType == BuildingDefinition.E_OccupationVolumeType.Adjacent && TileMapController.CanPlaceBuilding(buildingDefinition, originTile, ignoreUnit: true))
			{
				int buildingDeadZoneRange = BuildingManager.GetBuildingDeadZoneRange(buildingDefinition.Id);
				for (int i = -buildingDeadZoneRange; i <= buildingDeadZoneRange; i++)
				{
					for (int j = -buildingDeadZoneRange; j <= buildingDeadZoneRange; j++)
					{
						TheLastStand.Model.TileMap.Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(occupiedTiles[num].Position.x + i, occupiedTiles[num].Position.y + j);
						if (tile != null && !occupiedTiles.Contains(tile) && TPSingleton<ConstructionManager>.Instance.Construction.BuildingAvailableSpaceTiles.Contains(tile))
						{
							SetTile(OccupationVolumeBuildingTilemap, tile, "View/Tiles/Feedbacks/Occupation Volume Ghost");
						}
					}
				}
			}
		}
		if (buildingDefinition.BattleModuleDefinition == null)
		{
			return;
		}
		string text = string.Empty;
		Dictionary<string, int> skills = buildingDefinition.BattleModuleDefinition.Skills;
		if (skills != null && skills.Count > 0)
		{
			text = buildingDefinition.BattleModuleDefinition.Skills.First().Key;
		}
		else
		{
			BehaviorDefinition behavior = buildingDefinition.BattleModuleDefinition.Behavior;
			if (behavior != null && behavior.GoalDefinitions?.Length > 0)
			{
				text = buildingDefinition.BattleModuleDefinition.Behavior.GoalDefinitions[0].SkillId;
			}
		}
		if (text != string.Empty && SkillManager.TryGetSkillDefinitionOrDatabase(buildingDefinition.BattleModuleDefinition.SkillProgressions, text, TPSingleton<GameManager>.Instance.Game.DayNumber, out var skillDefinition))
		{
			if (buildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Trap))
			{
				DisplaySkillAoE(skillDefinition, originTile, AreaOfEffectTilemap);
			}
			else if (ApplicationManager.Application.State.GetName() != "LevelEditor")
			{
				DisplayBuildingGhostRange(skillDefinition, originTile, buildingDefinition.BlueprintModuleDefinition);
			}
		}
	}

	public void DisplayBuildingOutline(TheLastStand.Model.Building.Building building, bool show, bool hover, string suffix = "")
	{
		if (!show)
		{
			if (hover)
			{
				ClearHoverOutline(building.OriginTile, building.BuildingDefinition);
			}
			else
			{
				ClearSelectionOutline(building.OriginTile, building.BuildingDefinition);
			}
		}
		else
		{
			if ((building.DamageableModule is TrapDamageableModule && building.BattleModule.RemainingTrapCharges == 0) || (building.IsHandledDefense && building.BattleModule.HasDisabledStateAndZeroRemainingCharges))
			{
				return;
			}
			StartHoverOutlineTilemapAlphaTweening();
			for (int num = building.BlueprintModule.OccupiedTiles.Count - 1; num >= 0; num--)
			{
				Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(building.BlueprintModule.OccupiedTiles[num]);
				Vector3Int position = new Vector3Int(building.OriginTile.X + relativeBuildingTilePosition.x - building.BuildingDefinition.BlueprintModuleDefinition.OriginX, building.OriginTile.Y + relativeBuildingTilePosition.y - building.BuildingDefinition.BlueprintModuleDefinition.OriginY, 0);
				TileBase tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}{6}", "View/Tiles/Buildings/Outline", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y, "_Outline") ?? "", failSilently: true);
				Tilemap tilemap = (hover ? buildingHoverOutlinesTilemap : buildingSelectionOutlinesTilemap);
				if (tileBase != null)
				{
					tilemap.SetTile(position, tileBase);
				}
				else
				{
					tileBase = BuildingDatabase.TileBySpriteDictionary.GetTileBySprite(buildingTilemap.GetSprite(position));
					if (tileBase != null)
					{
						tilemap.SetTile(position, tileBase);
					}
				}
			}
		}
	}

	public void DisplayBuildingInstantly(TheLastStand.Model.Building.Building building, TheLastStand.Model.TileMap.Tile baseTile, string suffix = "")
	{
		if (string.IsNullOrEmpty(suffix) && building.BlueprintModule is GateBlueprintModule { IsOpen: not false })
		{
			suffix = "Opened";
		}
		for (int i = 0; i < building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			Vector2Int relativeBuildingTilePosition = building.BlueprintModule.GetRelativeBuildingTilePosition(building.BlueprintModule.OccupiedTiles[i]);
			Vector3Int position = new Vector3Int(baseTile.X + relativeBuildingTilePosition.x - building.BuildingDefinition.BlueprintModuleDefinition.OriginX, baseTile.Y + relativeBuildingTilePosition.y - building.BuildingDefinition.BlueprintModuleDefinition.OriginY, 0);
			TileBase tileBase = null;
			if (!building.BlueprintModule.IsIndestructible && building.DamageableModule.IsUnderDamagedThreshold)
			{
				tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}{6}", "View/Tiles/Buildings/Damaged Diffuse", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y, "_DamagedDiffuse"), failSilently: true);
				if (tileBase != null)
				{
					buildingDamagedTilemap.SetTile(position, tileBase);
				}
				tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}{6}", "View/Tiles/Buildings/Damaged Mask", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y, "_DamagedMask"), failSilently: true);
				if (tileBase != null)
				{
					buildingDamagedMaskTilemap.SetTile(position, tileBase);
				}
			}
			else
			{
				buildingDamagedTilemap.SetTile(position, null);
				buildingDamagedMaskTilemap.SetTile(position, null);
			}
			bool flag = BuildingView.TryGetDiffuseTileBase(building.BuildingDefinition.Id, suffix, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y, out tileBase);
			if (flag && tileBase != null)
			{
				BuildingTilemap.SetTile(position, tileBase);
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}Front{4}{5}", "View/Tiles/Buildings/Diffuse", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y), failSilently: true);
			if (tileBase != null)
			{
				BuildingFrontTilemap.SetTile(position, tileBase);
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}{4}{5}_Mask", "View/Tiles/Buildings/Mask", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y), failSilently: true);
			if (tileBase != null)
			{
				BuildingMasksTilemap.SetTile(position, tileBase);
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce(string.Format("{0}/{1}{2}/{3}Front{4}{5}_Mask", "View/Tiles/Buildings/Mask", building.BuildingDefinition.Id, suffix, building.BuildingDefinition.Id, relativeBuildingTilePosition.x, relativeBuildingTilePosition.y), failSilently: true);
			if (tileBase != null)
			{
				BuildingFrontMasksTilemap.SetTile(position, tileBase);
			}
			else if (!flag)
			{
				building.BuildingView.PlaceholderView = true;
			}
			tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.ShadowType, failSilently: true);
			if (tileBase != null)
			{
				BuildingShadowsTilemap.SetTile(position, tileBase);
			}
			if (building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType != "None")
			{
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType, failSilently: true);
				if (tileBase != null)
				{
					SideWalksTilemap.SetTile(position, tileBase);
				}
				tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Buildings/Diffuse/_Sidewalks/_Shadows/" + building.BuildingDefinition.BlueprintModuleDefinition.SidewalkType + "Shadow", failSilently: true);
				if (tileBase != null)
				{
					SideWalkShadowsTilemap.SetTile(position, tileBase);
				}
			}
		}
		building.BuildingView.transform.position = GetWorldPosition(baseTile);
		building.BuildingView.gameObject.SetActive(value: true);
	}

	public void DisplayBuildingSelectionFeedback(TheLastStand.Model.Building.Building building, bool show)
	{
		TileBase tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/BuildingSelectionFeedback");
		foreach (TheLastStand.Model.TileMap.Tile occupiedTile in building.BlueprintModule.OccupiedTiles)
		{
			SetTile(BuildingSelectionFeedbackTilemap, occupiedTile, show ? tileBase : null);
		}
	}

	public void DisplayDialsTilesFrom(TheLastStand.Model.TileMap.Tile sourceTile, Dictionary<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> inRangeTiles = null)
	{
		if (sourceTile == null)
		{
			return;
		}
		Vector2Int tilePos = Vector2Int.zero;
		TileBase tile = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_LeftRight");
		TileBase tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_top");
		TileBase tileBase2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/Dials/Tiles_cadrans_bot");
		int num = Mathf.Max(TPSingleton<TileMapManager>.Instance.TileMap.Width, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		for (int i = 0; i < num; i++)
		{
			tilePos.x = i;
			if (i == sourceTile.Position.x)
			{
				continue;
			}
			tilePos.y = sourceTile.Position.y + (sourceTile.Position.x - i);
			if (tilePos.y >= 0 && tilePos.y < TPSingleton<TileMapManager>.Instance.TileMap.Height && tilePos.x < TPSingleton<TileMapManager>.Instance.TileMap.Width)
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color color = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> keyValuePair) => keyValuePair.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				dialsTileMap.SetTile((Vector3Int)tilePos, tile);
				dialsTileMap.SetColor((Vector3Int)tilePos, color);
			}
			tilePos.y = sourceTile.Position.y - (sourceTile.Position.x - i);
			if (tilePos.y >= 0 && tilePos.y < TPSingleton<TileMapManager>.Instance.TileMap.Height && tilePos.x < TPSingleton<TileMapManager>.Instance.TileMap.Width)
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color color = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> keyValuePair) => keyValuePair.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				dialsTileMap.SetTile((Vector3Int)tilePos, (i > sourceTile.Position.x) ? tileBase : tileBase2);
				dialsTileMap.SetColor((Vector3Int)tilePos, color);
			}
		}
	}

	public void DisplayFogMinMax(bool show, int densityMin, int densityMax, bool forceRecompute = false)
	{
		FogMinMaxTilemap.ClearAllTiles();
		if (show)
		{
			if (fogMinMaxTiles == null || forceRecompute)
			{
				ComputeFogMinMaxTiles(densityMin, densityMax);
			}
			SetTiles(FogMinMaxTilemap, fogMinMaxTiles, fogMinMaxTileBase ?? ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/InaccurateRange"));
		}
	}

	public void DisplayInaccurateRangeTiles(List<TheLastStand.Model.TileMap.Tile> tiles)
	{
		TileBase item = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/InaccurateRange");
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count);
		List<TileBase> list2 = new List<TileBase>(tiles.Count);
		foreach (TheLastStand.Model.TileMap.Tile tile in tiles)
		{
			list.Add((Vector3Int)tile.Position);
			list2.Add(item);
		}
		skillInaccurateRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
	}

	public void DisplayPerkHoverRangeTiles(Perk perk)
	{
		if (perk.PerkDefinition.DisplayHavenArea)
		{
			bool hasValue = perk.PerkDefinition.HavenAreaColor.HasValue;
			Color color = Color.white;
			if (hasValue)
			{
				color = perk.PerkDefinition.HavenAreaColor.Value;
			}
			foreach (TheLastStand.Model.TileMap.Tile cityTile in TPSingleton<TileMapManager>.Instance.TileMap.CityTiles)
			{
				SetTile(havenAreaTilemap, cityTile, "View/Tiles/Feedbacks/Skill/SkillRange");
				if (hasValue)
				{
					SetTileColor(havenAreaTilemap, cityTile, color);
				}
			}
		}
		if (perk.PerkDefinition.HoverRanges.Count == 0)
		{
			return;
		}
		List<TheLastStand.Model.TileMap.Tile> list = new List<TheLastStand.Model.TileMap.Tile>();
		if (perk.PerkDefinition.HoverRanges.Count == 1)
		{
			foreach (TheLastStand.Model.TileMap.Tile item in perk.Owner.TileObjectController.GetTilesInRange(perk.PerkDefinition.HoverRanges[0].EvalToInt(perk)))
			{
				list.Add(item);
			}
			SetTiles(perkRangeTilemap, list, "View/Tiles/Feedbacks/Skill/SkillRange");
			return;
		}
		int num = -1;
		int num2 = -1;
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < perk.PerkDefinition.HoverRanges.Count; i++)
		{
			int num3 = perk.PerkDefinition.HoverRanges[i].EvalToInt(perk);
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		foreach (TheLastStand.Model.TileMap.Tile item2 in perk.Owner.TileObjectController.GetTilesInRange(num))
		{
			list.Add(item2);
		}
		SetTiles(perkRangeTilemap, list, "View/Tiles/Feedbacks/Skill/SkillRange");
		int num4 = 0;
		for (int j = 0; j < perk.PerkDefinition.HoverRanges.Count; j++)
		{
			if (j == num2)
			{
				continue;
			}
			int num5 = perk.PerkDefinition.HoverRanges[j].EvalToInt(perk);
			if (!hashSet.Add(num5))
			{
				continue;
			}
			list.Clear();
			foreach (TheLastStand.Model.TileMap.Tile item3 in perk.Owner.TileObjectController.GetTilesInRange(num5))
			{
				list.Add(item3);
			}
			if (num4 >= perkHoverRangeSeparatorTilemaps.Count)
			{
				AddPerkHoverRangeSeparatorTilemap();
			}
			SetTiles(perkHoverRangeSeparatorTilemaps[num4], list, "View/Tiles/Feedbacks/Skill/InaccurateRange");
			num4++;
		}
	}

	public void DisplayRangedSkillsModifiers(ITileObject tileObjectSource, TheLastStand.Model.Skill.Skill skill)
	{
		int num = 0;
		int num2 = 0;
		int num3 = skill.SkillController.ComputeMaxRange();
		TileBase tileBase = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/InaccurateRange");
		foreach (int key in SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance.Keys)
		{
			if (key > num3 && ++num2 == 2)
			{
				rangedSkillsDodgeMultiplierTilemaps[num].ClearAllTiles();
				continue;
			}
			int maxRange = Mathf.Min(key - 1, num3);
			List<TheLastStand.Model.TileMap.Tile> tilesInRange = tileObjectSource.TileObjectController.GetTilesInRange(maxRange, 0, skill.SkillDefinition.CardinalDirectionOnly);
			TileBase[] array = new TileBase[tilesInRange.Count];
			Vector3Int[] array2 = new Vector3Int[tilesInRange.Count];
			for (int i = 0; i < tilesInRange.Count; i++)
			{
				array2[i] = (Vector3Int)tilesInRange[i].Position;
				array[i] = tileBase;
			}
			rangedSkillsDodgeMultiplierTilemaps[num].SetTiles(array2, array);
			num++;
		}
	}

	public void DisplayRangeTiles(Dictionary<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tiles)
	{
		TileBase item = ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/Skill/SkillRange");
		List<Vector3Int> list = new List<Vector3Int>(tiles.Count);
		List<TileBase> list2 = new List<TileBase>(tiles.Count);
		foreach (KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tile in tiles)
		{
			list.Add((Vector3Int)tile.Key.Position);
			list2.Add(item);
		}
		SkillRangeTilemap.SetTiles(list.ToArray(), list2.ToArray());
		foreach (KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tile2 in tiles)
		{
			SkillRangeTilemap.SetColor((Vector3Int)tile2.Key.Position, tile2.Value.TileColor);
		}
	}

	public void DisplayReachableTile(TheLastStand.Model.TileMap.Tile tile)
	{
		SetTile(ReachableTilesTilemap, tile, "View/Tiles/Feedbacks/Movement/MoveRange");
		SetTileColor(ReachableTilesTilemap, tile, reachableTilesColor._Color);
	}

	public void DisplaySkillAoE(SkillDefinition skillDefinition, TheLastStand.Model.TileMap.Tile sourceTile, Tilemap tilemap)
	{
		for (int i = 0; i < skillDefinition.AreaOfEffectDefinition.Pattern.Count; i++)
		{
			for (int j = 0; j < skillDefinition.AreaOfEffectDefinition.Pattern[i].Count; j++)
			{
				if (skillDefinition.AreaOfEffectDefinition.Pattern[i][j] == 'X' || skillDefinition.AreaOfEffectDefinition.Pattern[i][j] == 'e')
				{
					Vector2Int vector2Int = new Vector2Int(i - skillDefinition.AreaOfEffectDefinition.Origin.x, j - skillDefinition.AreaOfEffectDefinition.Origin.y);
					TheLastStand.Model.TileMap.Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(sourceTile.Position.x + vector2Int.x, sourceTile.Position.y + vector2Int.y);
					if (tile != null)
					{
						E_AreaOfEffectTileDisplayType areaOfEffectTileDisplayType = ((skillDefinition.AreaOfEffectDefinition.Pattern[i][j] != 'X') ? E_AreaOfEffectTileDisplayType.Surrounding : E_AreaOfEffectTileDisplayType.AreaOfEffect);
						DisplayAreaOfEffectTile(tile, areaOfEffectTileDisplayType, unreachable: true, tilemap);
					}
				}
			}
		}
	}

	public void EndGhostAlphaTilemapsTweening()
	{
		ghostFadeTween?.Kill();
	}

	public void EndHoverOutlineAlphaTilemapTweening()
	{
		hoverOutlineTween?.Kill();
	}

	public IEnumerator FadeTilesAlphaCoroutine(IEnumerable<TheLastStand.Model.TileMap.Tile> tiles, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing, bool completeIfRunningAlready = true)
	{
		if (completeIfRunningAlready && alphaTweens.ContainsKey(tileMap))
		{
			alphaTweens[tileMap].Complete();
		}
		if (fadeIn)
		{
			SetTiles(tileMap, tiles.ToList(), tileBasePath);
			foreach (TheLastStand.Model.TileMap.Tile tile in tiles)
			{
				SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, 0f));
			}
		}
		alphaTweens[tileMap] = DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
		{
			foreach (TheLastStand.Model.TileMap.Tile tile2 in tiles)
			{
				SetTileColor(tileMap, tile2, new Color(1f, 1f, 1f, a));
			}
		}, fadeIn ? 1f : 0f, duration).SetEase(easing);
		alphaTweens[tileMap].OnComplete(delegate
		{
			if (!fadeIn)
			{
				SetTiles(tileMap, tiles.ToList());
			}
			alphaTweens.Remove(tileMap);
		});
		yield return alphaTweens[tileMap].WaitForCompletion();
	}

	public void FadeTilesIndependently(ref Dictionary<TheLastStand.Model.TileMap.Tile, Tween> dico, IEnumerable<TheLastStand.Model.TileMap.Tile> tiles, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing)
	{
		foreach (TheLastStand.Model.TileMap.Tile tile in tiles)
		{
			if (fadeIn)
			{
				SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, 0f));
				SetTile(tileMap, tile, fadeIn ? tileBasePath : null);
			}
			if (dico.ContainsKey(tile))
			{
				if (dico[tile] != null && dico[tile].active)
				{
					dico[tile].Complete();
					dico[tile] = DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
					{
						SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
					}, fadeIn ? 1f : 0f, duration).SetEase(easing);
				}
				else
				{
					dico[tile] = DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
					{
						SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
					}, fadeIn ? 1f : 0f, duration).SetEase(easing);
				}
			}
			else
			{
				dico.Add(tile, DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
				{
					SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
				}, fadeIn ? 1f : 0f, duration).SetEase(easing));
			}
			dico[tile].OnComplete(delegate
			{
				if (!fadeIn)
				{
					SetTile(tileMap, tile);
				}
			});
			dico[tile].Play();
		}
	}

	public void FadeTileIndependently(ref Dictionary<TheLastStand.Model.TileMap.Tile, Tween> dico, TheLastStand.Model.TileMap.Tile tile, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing)
	{
		if (dico.ContainsKey(tile))
		{
			if (dico[tile] != null && dico[tile].active)
			{
				dico[tile].Complete();
				dico[tile] = DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
				{
					SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
				}, fadeIn ? 1f : 0f, duration).SetEase(easing);
			}
			else
			{
				SetTile(tileMap, tile, fadeIn ? tileBasePath : null);
				dico[tile] = DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
				{
					SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
				}, fadeIn ? 1f : 0f, duration).SetEase(easing);
			}
		}
		else
		{
			SetTile(tileMap, tile, fadeIn ? tileBasePath : null);
			dico.Add(tile, DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
			{
				SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
			}, fadeIn ? 1f : 0f, duration).SetEase(easing));
		}
		dico[tile].Play();
	}

	public IEnumerator FadeTileAlphaCoroutine(TheLastStand.Model.TileMap.Tile tile, bool fadeIn, Tilemap tileMap, string tileBasePath, float duration, Ease easing, bool completeIfRunningAlready = true)
	{
		if (completeIfRunningAlready && alphaTweens.ContainsKey(tileMap))
		{
			alphaTweens[tileMap].Complete();
		}
		if (fadeIn)
		{
			SetTile(tileMap, tile, tileBasePath);
			SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, 0f));
		}
		alphaTweens[tileMap] = DOTween.To(() => (!fadeIn) ? 1f : 0f, delegate(float a)
		{
			SetTileColor(tileMap, tile, new Color(1f, 1f, 1f, a));
		}, fadeIn ? 1f : 0f, duration).SetEase(easing);
		alphaTweens[tileMap].OnComplete(delegate
		{
			if (!fadeIn)
			{
				SetTile(tileMap, tile);
			}
			alphaTweens.Remove(tileMap);
		});
		yield return alphaTweens[tileMap].WaitForCompletion();
	}

	public void ForceOpenGate(TheLastStand.Model.Building.Building building, TheLastStand.Model.TileMap.Tile baseTile)
	{
		DisplayBuilding(building, baseTile, "Opened");
	}

	public void Hide()
	{
		activatedTileMaps.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (base.transform.GetChild(i).gameObject.activeInHierarchy)
			{
				activatedTileMaps.Enqueue(i);
				base.transform.GetChild(i).gameObject.SetActive(value: false);
			}
		}
	}

	public void SetWorldLimitTile(Vector3Int position)
	{
		WorldLimitsTilemap.SetTile(position, ResourcePooler<TileBase>.LoadOnce("View/Tiles/Feedbacks/WorldLimits/WorldLimits"));
	}

	public void StartGhostTilemapsAlphaTweening()
	{
		ghostFadeTween?.Kill();
		float alpha = ghostTweenMaxAlpha;
		ghostFadeTween = DOTween.To(() => alpha, delegate(float x)
		{
			SetTilemapsAlpha(x, GhostBuildingsTilemap, GhostBuildingsFrontTilemap);
		}, ghostTweenMinAlpha, ghostTweenDuration).SetEase(ghostTweenEaseCurve).SetLoops(-1, LoopType.Yoyo)
			.SetFullId("BuildingGhostTween", this);
	}

	public void UpdateDisplayRangeTilesColors(Dictionary<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tiles)
	{
		foreach (KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tile in tiles)
		{
			if (SkillRangeTilemap.HasTile((Vector3Int)tile.Key.Position))
			{
				if (!tile.Value.Orientation.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) && SkillManager.SelectedSkill != null && (SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate))
				{
					tile.Value.TileColor = (tile.Value.HasLineOfSight ? skillRangeTilesColorInvalidOrientation._Color : skillHiddenRangeTilesColorInvalidOrientation._Color);
				}
				else
				{
					tile.Value.TileColor = (tile.Value.HasLineOfSight ? skillRangeTilesColor._Color : skillHiddenRangeTilesColor._Color);
				}
				SkillRangeTilemap.SetColor((Vector3Int)tile.Key.Position, tile.Value.TileColor);
			}
		}
	}

	public void UpdateDialsTilesColorsFrom(TheLastStand.Model.TileMap.Tile sourceTile, Dictionary<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> inRangeTiles = null)
	{
		if (sourceTile == null)
		{
			return;
		}
		Vector2Int tilePos = Vector2Int.zero;
		int num = Mathf.Max(TPSingleton<TileMapManager>.Instance.TileMap.Width, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		for (int i = 0; i < num; i++)
		{
			tilePos.x = i;
			if (i == sourceTile.Position.x)
			{
				continue;
			}
			tilePos.y = sourceTile.Position.y + (sourceTile.Position.x - i);
			if (dialsTileMap.HasTile((Vector3Int)tilePos))
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color color = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tile) => tile.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				dialsTileMap.SetColor((Vector3Int)tilePos, color);
			}
			tilePos.y = sourceTile.Position.y - (sourceTile.Position.x - i);
			if (dialsTileMap.HasTile((Vector3Int)tilePos))
			{
				TileObjectSelectionManager.E_Orientation orientationFromSelectionToPos = TileObjectSelectionManager.GetOrientationFromSelectionToPos(tilePos);
				Color color = ((inRangeTiles == null || !inRangeTiles.Any((KeyValuePair<TheLastStand.Model.TileMap.Tile, TilesInRangeInfos.TileDisplayInfos> tile) => tile.Key.Position == tilePos)) ? (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? outOfRangeDialsColor._Color : outOfRangeInvalidOrientationDialsColor._Color) : (orientationFromSelectionToPos.HasFlag(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection) ? inRangeDialsColor._Color : inRangeInvalidOrientationDialsColor._Color));
				dialsTileMap.SetColor((Vector3Int)tilePos, color);
			}
		}
	}

	private void AddGhostRangeTile(TheLastStand.Model.TileMap.Tile tile, Vector2Int distance, TheLastStand.Model.TileMap.Tile sourceTile, bool ignoreLineOfSight, Vector2Int range)
	{
		if (tile == null)
		{
			return;
		}
		if (Mathf.Abs(distance.x) + Mathf.Abs(distance.y) >= range.x)
		{
			if (ignoreLineOfSight)
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: true, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
				return;
			}
			if (TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude.Contains(tile))
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: false, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
				return;
			}
			if (SkillActionExecutionController.CheckAndUpdateLineOfSight(tile, sourceTile, distance, range.y, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Obstacle, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude))
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: true, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
			}
			else
			{
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: false, TileObjectSelectionManager.E_Orientation.NONE, isSkillSelected: false));
				TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude.Add(tile);
			}
		}
		if (distance != Vector2Int.zero && SkillActionExecutionController.IsBlockingLineOfSight(tile) && !ignoreLineOfSight)
		{
			TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Obstacle.Add(tile);
			SkillActionExecutionController.ExcludeTiles(tile, distance, range.y, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Obstacle, TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Exclude);
		}
	}

	private void AddPerkHoverRangeSeparatorTilemap()
	{
		Tilemap tilemap = UnityEngine.Object.Instantiate(perkHoverRangeSeparatorTemplate, perkHoverRangeSeparatorContainer);
		ClearTiles(tilemap);
		perkHoverRangeSeparatorTilemaps.Add(tilemap);
	}

	private void ClearBuildingGhostRangeAndZoneTiles()
	{
		ClearRangeTiles(TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range);
		ClearTiles(AreaOfEffectTilemap);
		TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Clear();
	}

	private void ComputeFogMinMaxTiles(int densityMin, int densityMax)
	{
		TheLastStand.Model.TileMap.Tile centerTile = TileMapController.GetCenterTile();
		fogMinMaxTiles = new HashSet<Vector3Int>();
		int num = Mathf.Abs(centerTile.X - densityMax) + 1;
		int num2 = Mathf.Abs(centerTile.X + densityMax) - 1;
		int num3 = Mathf.Abs(centerTile.Y - densityMax) + 1;
		int num4 = Mathf.Abs(centerTile.Y + densityMax) - 1;
		int num5 = densityMax - densityMin;
		for (int i = 0; i < num5; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				fogMinMaxTiles.Add(new Vector3Int(j, num3 + i, 0));
				fogMinMaxTiles.Add(new Vector3Int(j, num4 - i, 0));
			}
			for (int k = num3; k <= num4; k++)
			{
				fogMinMaxTiles.Add(new Vector3Int(num + i, k, 0));
				fogMinMaxTiles.Add(new Vector3Int(num2 - i, k, 0));
			}
		}
	}

	private void DisplayBuildingGhostRange(SkillDefinition skillDefinition, TheLastStand.Model.TileMap.Tile sourceTile, ITileObjectDefinition tileObjectDefinition)
	{
		if (skillDefinition.InfiniteRange)
		{
			return;
		}
		bool ignoreLineOfSight = skillDefinition.SkillActionDefinition.HasEffect("IgnoreLineOfSight");
		foreach (KeyValuePair<TheLastStand.Model.TileMap.Tile, TheLastStand.Model.TileMap.Tile> item in sourceTile.GetTilesInRangeWithClosestOccupiedTile(tileObjectDefinition, skillDefinition.Range.y, 0, skillDefinition.CardinalDirectionOnly))
		{
			AddGhostRangeTile(distance: new Vector2Int(item.Key.X - item.Value.X, item.Key.Y - item.Value.Y), tile: item.Key, sourceTile: item.Value, ignoreLineOfSight: ignoreLineOfSight, range: skillDefinition.Range);
		}
		TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.ClearLonelyTilesInLineOfSight(sourceTile, skillDefinition.Range.x, skillDefinition.Range.y);
		DisplayRangeTiles(TPSingleton<ConstructionManager>.Instance.LineOfSightTiles.Range);
	}

	private IEnumerator DisplayLevelCoroutine()
	{
		int frames = 0;
		int x = 0;
		while (x < TPSingleton<TileMapManager>.Instance.TileMap.Width)
		{
			int num;
			for (int y = 0; y < TPSingleton<TileMapManager>.Instance.TileMap.Height; y = num)
			{
				TheLastStand.Model.TileMap.Tile tile = TileMapManager.GetTile(x, y);
				string id = tile.GroundDefinition.Id;
				TileBase tile2 = ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + id);
				((tile.GroundDefinition.GroundCategory == GroundDefinition.E_GroundCategory.NoBuilding) ? GroundCraterTilemap : GroundCityTilemap).SetTile(new Vector3Int(x, y, 0), tile2);
				SetTile(GridTilemap, tile, "View/Tiles/Feedbacks/Grid Cell");
				if (frames++ % TileMapManager.LoadingSpeed == 0)
				{
					yield return SharedYields.WaitForEndOfFrame;
				}
				if (tile.Building != null && tile.Building.OriginTile == tile)
				{
					DisplayBuilding(tile.Building, tile);
					if (frames++ % TileMapManager.LoadingSpeed == 0)
					{
						yield return SharedYields.WaitForEndOfFrame;
					}
				}
				num = y + 1;
			}
			num = x + 1;
			x = num;
		}
	}

	private void LoadTileAssets()
	{
		foreach (BuildingDefinition value in BuildingDatabase.BuildingDefinitions.Values)
		{
			for (int num = value.BlueprintModuleDefinition.Tiles.Count - 1; num >= 0; num--)
			{
				for (int num2 = value.BlueprintModuleDefinition.Tiles[num].Count - 1; num2 >= 0; num2--)
				{
					if (value.BlueprintModuleDefinition.Tiles[num][num2] == TheLastStand.Model.TileMap.Tile.E_UnitAccess.Blocked || value.BlueprintModuleDefinition.Tiles[num][num2] == TheLastStand.Model.TileMap.Tile.E_UnitAccess.Hero)
					{
						ResourcePooler<Sprite>.CacheAll(string.Format("{0}/{1}/{2}{3}", "View/Sprites/ConstructionAnimation", value.Id, num, num2));
					}
				}
			}
		}
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/BuildingSelectionFeedback");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/MistRange/MistRange");
		ResourcePooler<TileBase>.Cache("View/Tiles/World/Fog");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Grid Cell");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Movement/MoveRange");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Occupation Volume");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Occupation Volume Ghost");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/PanicOnEnemy");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/PoisonDeath");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillAoe Back");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillManeuver");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillRange");
		ResourcePooler<TileBase>.Cache("View/Tiles/Feedbacks/Skill/SkillSurrounding");
	}

	private void SetTilemapsAlpha(float alpha, params Tilemap[] tilemaps)
	{
		Color color = tilemaps[0].color;
		color.a = alpha;
		for (int num = tilemaps.Length - 1; num >= 0; num--)
		{
			tilemaps[num].color = color;
		}
	}

	private void Start()
	{
		rangedSkillsDodgeMultiplierTilemaps = new Tilemap[SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance.Count];
		int num = 0;
		foreach (int key in SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance.Keys)
		{
			_ = key;
			rangedSkillsDodgeMultiplierTilemaps[num] = UnityEngine.Object.Instantiate(rangedSkillsDodgeMultiplierTemplate, rangedSkillsDodgeMultiplierContainer);
			rangedSkillsDodgeMultiplierTilemaps[num].ClearAllTiles();
			rangedSkillsDodgeMultiplierTilemaps[num].transform.name = rangedSkillsDodgeMultiplierTilemaps[num].transform.name.Replace("Template(Clone)", num.ToString());
			num++;
		}
		rangedSkillsDodgeMultiplierTemplate.gameObject.SetActive(value: false);
		foreach (KeyValuePair<TileFlagDefinition.E_TileFlagTag, List<TheLastStand.Model.TileMap.Tile>> tilesByFlag in TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag)
		{
			TileFlagDefinition tileFlagDefinition = TileMapManager.TileFlagDefinitions.FirstOrDefault((TileFlagDefinition o) => o.TileFlagTag == tilesByFlag.Key);
			if (tileFlagDefinition == null)
			{
				TPSingleton<TileMapManager>.Instance.LogError($"{tilesByFlag.Key} was not found in TileMapManager.TileFlagDefinitions even though it is present in TileMapManager.Instance.TileMap.TilesWithFlag. Something is wrong here!");
				continue;
			}
			for (int num2 = tilesByFlag.Value.Count - 1; num2 >= 0; num2--)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayFlagTile(tileFlagDefinition, tilesByFlag.Value[num2]);
			}
		}
		if (TPSingleton<LevelEditorManager>.Exist())
		{
			FogMinMaxTilemap.ClearAllTiles();
		}
	}

	private void StartHoverOutlineTilemapAlphaTweening()
	{
		if (hoverOutlineTween == null || !hoverOutlineTween.IsActive() || !hoverOutlineTween.IsPlaying())
		{
			hoverOutlineTween = DOTween.To(() => hoverOutlineTweenMaxAlpha, delegate(float x)
			{
				SetTilemapsAlpha(x, buildingHoverOutlinesTilemap);
			}, hoverOutlineTweenMinAlpha, hoverOutlineTweenDuration).SetEase(hoverOutlineTweenEaseCurve).SetLoops(-1, LoopType.Yoyo)
				.SetFullId("BuildingHoverOutlineTween", this);
		}
	}

	private void UpdateBackground(bool blackenBackground = false)
	{
		int num = Mathf.Max(TPSingleton<TileMapManager>.Instance.TileMap.Width, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		levelBackground.position = GetCellCenterWorldPosition(TPSingleton<TileMapManager>.Instance.TileMap.GetTile(Mathf.FloorToInt((float)TPSingleton<TileMapManager>.Instance.TileMap.Width / 2f), Mathf.FloorToInt((float)TPSingleton<TileMapManager>.Instance.TileMap.Height / 2f)));
		levelBackground.position += new Vector3(0f, 0.75f, 0f);
		levelBackground.transform.localScale = Vector3.one * ((float)num * backgroundSizeRatio / (float)mapSizeReference);
		if (blackenBackground)
		{
			levelBackgroundRenderer.color = Color.black;
		}
	}

	public static void ToggleLevelArt()
	{
		TPSingleton<TileMapView>.Instance.levelArtContainer.gameObject.SetActive(!TPSingleton<TileMapView>.Instance.levelArtContainer.gameObject.activeSelf);
	}

	public static void ToggleTilesFlag(TileFlagDefinition.E_TileFlagTag flag = TileFlagDefinition.E_TileFlagTag.None, bool? forcedState = null, bool clearPreviousState = true)
	{
		if (clearPreviousState)
		{
			foreach (Tilemap value in TilemapsByFlag.Values)
			{
				value.gameObject.SetActive(value: false);
			}
		}
		if (flag != TileFlagDefinition.E_TileFlagTag.None)
		{
			TilemapsByFlag[flag].gameObject.SetActive(forcedState ?? (!TilemapsByFlag[flag].gameObject.activeSelf));
		}
	}

	public static void ToggleTilesFlagAll(bool? state = null)
	{
		foreach (Tilemap value in TilemapsByFlag.Values)
		{
			value.gameObject.SetActive(state ?? (!value.gameObject.activeSelf));
		}
	}

	public void ClearFlagTile(TileFlagDefinition flagDefinition, TheLastStand.Model.TileMap.Tile tile)
	{
		SetTile(TilemapsByFlag[flagDefinition.TileFlagTag], tile);
	}

	public void DisplayFlagTile(TileFlagDefinition flagDefinition, TheLastStand.Model.TileMap.Tile tile)
	{
		Tilemap tileMap = TilemapsByFlag[flagDefinition.TileFlagTag];
		SetTile(tileMap, tile, "View/Tiles/Feedbacks/Skill/SkillRange");
		SetTileColor(tileMap, tile, flagDefinition.DebugColor);
	}

	public void DisplayGround(TheLastStand.Model.TileMap.Tile tile, string cityId)
	{
		TileBase tile2 = ((!TPSingleton<TileMapView>.Instance.levelArtLoaded) ? (ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id + "_" + cityId) ?? ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/" + tile.GroundDefinition.Id)) : ResourcePooler<TileBase>.LoadOnce("View/Tiles/World/TileShape"));
		switch (tile.GroundDefinition.GroundCategory)
		{
		case GroundDefinition.E_GroundCategory.City:
			GroundCityTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), tile2);
			GroundCraterTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), null);
			break;
		case GroundDefinition.E_GroundCategory.NoBuilding:
			GroundCraterTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), tile2);
			GroundCityTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), null);
			break;
		case GroundDefinition.E_GroundCategory.Outside:
			GroundCraterTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), null);
			GroundCityTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), null);
			break;
		}
	}

	public void SetGroundTilemapsAlpha(float value)
	{
		GroundCityTilemap.color = GroundCityTilemap.color.WithA(value);
		GroundCraterTilemap.color = GroundCraterTilemap.color.WithA(value);
	}

	[DevConsoleCommand(Name = "TilesFlagsHideAll")]
	public static void DebugHideTilesFlagsAll()
	{
		ToggleTilesFlagAll(false);
	}

	[DevConsoleCommand(Name = "TilesFlagShow")]
	public static void DebugShowTilesFlag([StringConverter(typeof(TileFlagDefinition.E_TileFlagTag))] TileFlagDefinition.E_TileFlagTag flag, bool show = true)
	{
		ToggleTilesFlag(flag, show, clearPreviousState: false);
	}

	[DevConsoleCommand(Name = "TilesFlagsShowAll")]
	public static void DebugShowTilesFlagsAll()
	{
		ToggleTilesFlagAll(true);
	}

	[DevConsoleCommand(Name = "FogShowMinMax")]
	public static void DebugShowFogMinMax(bool show = true)
	{
		FogDefinition fogDefinition = FogDatabase.FogsDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.FogDefinitionId];
		TPSingleton<TileMapView>.Instance.DisplayFogMinMax(show, fogDefinition.FogDensities[^1].Value, fogDefinition.FogDensities[0].Value);
	}

	[DevConsoleCommand(Name = "ShowGroundsLogic")]
	public static void DebugShowGroundsLogic(float alpha = 0.4f)
	{
		TPSingleton<TileMapView>.Instance.SetGroundTilemapsAlpha(alpha);
	}

	[DevConsoleCommand(Name = "ShowBoneZones")]
	public static void DebugShowBoneZones(float alpha = 0.8f)
	{
		if (TPSingleton<TileMapView>.Instance.boneZoneTilemaps.Count == 0)
		{
			TPSingleton<TileMapView>.Instance.GenerateBoneZoneTilemaps();
		}
		foreach (Tilemap boneZoneTilemap in TPSingleton<TileMapView>.Instance.boneZoneTilemaps)
		{
			boneZoneTilemap.color = boneZoneTilemap.color.WithA(alpha);
		}
	}

	[DevConsoleCommand(Name = "HideBoneZones")]
	public static void DebugHideBoneZones()
	{
		DebugShowBoneZones(0f);
	}

	[DevConsoleCommand(Name = "ShowBonePilesPercentage")]
	public static void DebugShowBonePilesPercentage()
	{
		UnityEngine.Object.FindObjectOfType<BonePilePercentagesView>()?.Toggle(state: true);
	}

	[DevConsoleCommand(Name = "HideBonePilesPercentage")]
	public static void DebugHideBonePilesPercentage()
	{
		UnityEngine.Object.FindObjectOfType<BonePilePercentagesView>()?.Toggle(state: false);
	}

	[DevConsoleCommand(Name = "SetBonePilePercentage")]
	public static void DebugSetBonePilePercentage([StringConverter(typeof(StringToBonePileIdConverter))] string pileId, int percentage)
	{
		TheLastStand.Model.TileMap.Tile selectedTile = TileObjectSelectionManager.SelectedTile;
		if (selectedTile != null)
		{
			if (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(selectedTile, out var value))
			{
				value = new Dictionary<string, int>();
				TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Add(selectedTile, value);
			}
			if (value.ContainsKey(pileId))
			{
				value[pileId] += percentage;
			}
			else
			{
				value.Add(pileId, percentage);
			}
		}
	}

	[DevConsoleCommand(Name = "SetBonePilePercentageForTile")]
	public static void DebugSetBonePilePercentageForTile(int x, int y, [StringConverter(typeof(StringToBonePileIdConverter))] string pileId, int percentage)
	{
		TheLastStand.Model.TileMap.Tile tile = TileMapManager.GetTile(x, y);
		if (tile != null)
		{
			if (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(tile, out var value))
			{
				value = new Dictionary<string, int>();
				TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Add(tile, value);
			}
			if (value.ContainsKey(pileId))
			{
				value[pileId] += percentage;
			}
			else
			{
				value.Add(pileId, percentage);
			}
		}
	}

	[DevConsoleCommand(Name = "AddBonePiles")]
	public static void DebugAddBonePiles([StringConverter(typeof(StringToBonePileIdConverter))] string pileId, int tilesCount)
	{
		for (int i = 0; i < tilesCount; i++)
		{
			TheLastStand.Model.TileMap.Tile randomTile = TileMapManager.GetRandomTile();
			if (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(randomTile, out var value))
			{
				value = new Dictionary<string, int>();
				TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Add(randomTile, value);
			}
			if (value.ContainsKey(pileId))
			{
				value[pileId] += 100;
			}
			else
			{
				value.Add(pileId, 100);
			}
		}
	}
}
