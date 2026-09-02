using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.BonePile;
using TheLastStand.Definition.Brazier;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Framework.Serialization;
using TheLastStand.Helpers;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.UI;
using UnityEngine;

namespace TheLastStand.Manager.Unit;

public class EnemyUnitManager : BehaviorManager<EnemyUnitManager>, ISerializable, IDeserializable
{
	public static class Constants
	{
		public static class Sound
		{
			public static class Death
			{
				public const string EnemyDeathAudioSourcePoolId = "Enemies Death";

				public const string EnemyDeathSpatializedAudioSourcePoolId = "Enemies Death Spatialized";

				public const string EnemyDeathSoundDefaultFolderId = "Clawer";

				public const string EnemyDeathSoundNoFolderId = "None";

				public const string EnemyDeathSoundFolderPrefix = "Sounds/SFX/Enemy/Deaths/";
			}

			public static class Move
			{
				public const string EnemyMoveAudioSourcePoolId = "Enemies Move";

				public const string EnemyMoveSpatializedAudioSourcePoolId = "Enemies Move Spatialized";

				public const string EnemyMoveSoundDefaultFolderId = "Clawer";

				public const string EnemyMoveSoundFolderPrefix = "Sounds/SFX/Enemy/Move/";
			}

			public static class Skill
			{
				public const string EnemySkillAudioSourcePoolId = "EnemySkillSFX";

				public const string EnemySkillSpatializedAudioSourcePoolId = "EnemySkillSFX Spatialized";

				public const string EnemySkillSoundClipPathFormat = "Sounds/SFX/Enemy/Skills/{0}/SFX_{0}_{1}";

				public const string EnemySkillSoundLaunchClipPathFormat = "Sounds/SFX/Enemy/Skills/{0}/SFX_{0}_{1}_Launch";

				public const string EnemySkillSoundImpactClipPathFormat = "Sounds/SFX/Enemy/Skills/{0}/SFX_{0}_{1}_Impact";

				public const string EnemySkillSoundAssetPrefix = "Sounds/SFX/Enemy/Skills/";
			}

			public const string EnemySoundNoFolderId = "None";
		}
	}

	[SerializeField]
	private Vector2 idleAnimSpeedMultRange = new Vector2(0.8f, 1.25f);

	[SerializeField]
	[Tooltip("Set it to true to add extra check in the pathfinding to better path correction.\nIncreases the pathfinding efficiency but can reduce performance.")]
	private bool checkValidPathMoveRange = true;

	[SerializeField]
	private OneShotSound enemyDeathSFXPrefab;

	[SerializeField]
	private OneShotSound enemyDeathSpatializedSFXPrefab;

	[SerializeField]
	private Vector2 delayBetweenDeathAndSound = new Vector2(0f, 0.2f);

	[SerializeField]
	private OneShotSound enemyMoveSFXPrefab;

	[SerializeField]
	private float moveMaxRandomDelay = 1f;

	[SerializeField]
	private Transform unitsTransform;

	[SerializeField]
	private EnemyUnitView enemyUnitViewPrefab;

	[SerializeField]
	private EliteEnemyUnitView eliteEnemyUnitViewPrefab;

	[SerializeField]
	private EnemyUnitTooltip enemyUnitInfoPanel;

	[SerializeField]
	private EliteEnemyUnitTooltip eliteEnemyUnitInfoPanel;

	[SerializeField]
	private Material defaultEnemyMaterial;

	[SerializeField]
	private EnemyUnitHUD enemyUnitHUDSmall;

	[SerializeField]
	private EnemyUnitHUD enemyUnitHUDLarge;

	private bool isDisplayingAllEnemiesMoveRange;

	private bool lastTileWasEnemyUnit;

	private Dictionary<BraziersDefinition.GuardiansGroup, List<int>> spawnedGuardians = new Dictionary<BraziersDefinition.GuardiansGroup, List<int>>();

	private BraziersDefinition.GuardiansGroup guardiansGroupToSpawn;

	private TheLastStand.Model.Skill.Skill previewedSkill;

	private List<IBehaviorModel> sortedEnemies;

	private Dictionary<string, Dictionary<UnitStatDefinition.E_Stat, float>> statsProgressionBonusForToday;

	private HashSet<EnemyUnitView> zoneControlEnemies = new HashSet<EnemyUnitView>();

	public List<EnemyUnit> EnemiesDeathRattling;

	public HashSet<EnemyUnit> EnemiesDying;

	public HashSet<EnemyUnit> EnemiesExecutingSkillsOnSpawn;

	public List<TheLastStand.Model.Unit.Unit> DyingEnemiesWithContagion = new List<TheLastStand.Model.Unit.Unit>();

	[SerializeField]
	private bool disableHuman;

	[SerializeField]
	private bool turboSkillMode;

	[SerializeField]
	private bool debugEnemyAttackFeedback = true;

	public EnemyUnitTemplateDefinition DebugEnemyUnitTemplateDefinition;

	public EliteEnemyUnitTemplateDefinition DebugEliteEnemyUnitToSpawnTemplateDefinition;

	private EnemyAffixDefinition debugForcedEliteAffixDefinition;

	private GameDefinition.E_Direction debugEnemySpawnDirection = GameDefinition.E_Direction.None;

	private bool debugEnemySpawnAllEnabled;

	private bool debugEnemySpawnAllIncludeElites;

	private bool willSpawnEnemyNextFrame;

	public static bool CheckValidPathMoveRange => TPSingleton<EnemyUnitManager>.Instance.checkValidPathMoveRange;

	public static Material DefaultEnemyMaterial => TPSingleton<EnemyUnitManager>.Instance.defaultEnemyMaterial;

	public static Vector2 DelayBetweenDeathAndSound => TPSingleton<EnemyUnitManager>.Instance.delayBetweenDeathAndSound;

	public static EliteEnemyUnitTooltip EliteEnemyUnitInfoPanel => TPSingleton<EnemyUnitManager>.Instance.eliteEnemyUnitInfoPanel;

	public static Dictionary<EnemyUnitTemplateDefinition, Color> EnemyAttackFeedbackColorsByTemplate { get; private set; } = new Dictionary<EnemyUnitTemplateDefinition, Color>();

	public static OneShotSound EnemyDeathSpatializedSFXPrefab => TPSingleton<EnemyUnitManager>.Instance.enemyDeathSpatializedSFXPrefab;

	public static EnemyUnitTooltip EnemyUnitInfoPanel => TPSingleton<EnemyUnitManager>.Instance.enemyUnitInfoPanel;

	public static Vector2 IdleAnimSpeedMultRange => TPSingleton<EnemyUnitManager>.Instance.idleAnimSpeedMultRange;

	public static TheLastStand.Model.Skill.Skill PreviewedSkill
	{
		get
		{
			return TPSingleton<EnemyUnitManager>.Instance.previewedSkill;
		}
		set
		{
			TheLastStand.Model.Skill.Skill skill = TPSingleton<EnemyUnitManager>.Instance.previewedSkill;
			TPSingleton<EnemyUnitManager>.Instance.previewedSkill = value;
			skill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		}
	}

	public static bool TurboMode => TPSingleton<EnemyUnitManager>.Instance.turboMode;

	public static OneShotSound EnemyMoveSFXPrefab => TPSingleton<EnemyUnitManager>.Instance.enemyDeathSFXPrefab;

	public static Transform UnitsTransform => TPSingleton<EnemyUnitManager>.Instance.unitsTransform;

	public static UnitView EnemyUnitViewPrefab => TPSingleton<EnemyUnitManager>.Instance.enemyUnitViewPrefab;

	public static EnemyUnitHUD EnemyUnitHUDSmall => TPSingleton<EnemyUnitManager>.Instance.enemyUnitHUDSmall;

	public static EnemyUnitHUD EnemyUnitHUDLarge => TPSingleton<EnemyUnitManager>.Instance.enemyUnitHUDLarge;

	public Dictionary<Tile, Dictionary<string, int>> BonePilesPercentages { get; } = new Dictionary<Tile, Dictionary<string, int>>();

	public List<EnemyUnit> EnemyUnits { get; } = new List<EnemyUnit>();

	public int ComputedEnemyUnitsCount => EnemyUnits.Count - EnemyUnitsToIgnoreFromCount;

	public int EnemyUnitsToIgnoreFromCount { get; set; }

	public TaskGroup MoveUnitsTaskGroup { get; private set; }

	public int TotalCasters { get; private set; }

	public WaitUntil WaitUntilDeathRattlingEnemiesAreDone { get; private set; }

	public WaitUntil WaitUntilDyingEnemiesAreDone { get; private set; }

	public WaitUntil WaitUntilEnemiesExecutingSkillsOnSpawnAreDone { get; private set; }

	public override List<IBehaviorModel> BehaviorModels => EnemyUnits.Cast<IBehaviorModel>().ToList();

	public static bool DebugEnemyAttackFeedback => TPSingleton<EnemyUnitManager>.Instance.debugEnemyAttackFeedback;

	public static bool DisableHuman
	{
		get
		{
			if (TPSingleton<EnemyUnitManager>.Instance.disableHuman && TPSingleton<NightTurnsManager>.Instance.DisableAI)
			{
				TPSingleton<EnemyUnitManager>.Instance.LogError("You cannot disable both AI & human!", CLogLevel.DETAILED);
				return false;
			}
			return TPSingleton<EnemyUnitManager>.Instance.disableHuman;
		}
	}

	public static bool IsThereAnyEnemyDying()
	{
		if (TPSingleton<EnemyUnitManager>.Instance.EnemyUnits == null || TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count == 0)
		{
			return false;
		}
		foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
		{
			if (enemyUnit.IsDead || enemyUnit.IsDeathRattling)
			{
				return true;
			}
		}
		return false;
	}

	public static EliteEnemyUnit CreateEliteEnemyUnit(EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition, Tile tile, UnitCreationSettings unitCreationSettings, EnemyAffixDefinition enemyAffixDefinition = null)
	{
		if (tile == null)
		{
			TPSingleton<EnemyUnitManager>.Instance.LogWarning("There is no tile!", CLogLevel.DETAILED);
			return null;
		}
		if (tile.Unit == null)
		{
			TheLastStand.Model.Building.Building building = tile.Building;
			if (building == null || building.CanSpawnEnemyOnIt)
			{
				UnitView pooledComponent = ObjectPooler.GetPooledComponent("EliteEnemyUnitViews", TPSingleton<EnemyUnitManager>.Instance.eliteEnemyUnitViewPrefab, TPSingleton<EnemyUnitManager>.Instance.unitsTransform);
				EliteEnemyUnitController eliteEnemyUnitController = new EliteEnemyUnitController(eliteEnemyUnitTemplateDefinition, pooledComponent, tile, unitCreationSettings, enemyAffixDefinition);
				CreateEnemyUnit(eliteEnemyUnitController, tile, unitCreationSettings);
				return eliteEnemyUnitController.EliteEnemyUnit;
			}
		}
		return null;
	}

	public static EnemyUnit CreateEnemyUnit(EnemyUnitTemplateDefinition enemyUnitTemplateDefinition, Tile tile, UnitCreationSettings unitCreationSettings)
	{
		if (tile == null)
		{
			TPSingleton<EnemyUnitManager>.Instance.LogWarning("There is no tile!", CLogLevel.DETAILED);
			return null;
		}
		if (tile.Unit == null)
		{
			TheLastStand.Model.Building.Building building = tile.Building;
			if (building == null || building.CanSpawnEnemyOnIt)
			{
				EnemyUnitView pooledComponent = ObjectPooler.GetPooledComponent("EnemyUnitViews", TPSingleton<EnemyUnitManager>.Instance.enemyUnitViewPrefab, TPSingleton<EnemyUnitManager>.Instance.unitsTransform);
				EnemyUnitController enemyUnitController = new EnemyUnitController(enemyUnitTemplateDefinition, pooledComponent, tile, unitCreationSettings);
				CreateEnemyUnit(enemyUnitController, tile, unitCreationSettings);
				return enemyUnitController.EnemyUnit;
			}
		}
		return null;
	}

	public static EnemyUnit CreateEnemyUnit(EnemyUnitTemplateDefinition enemyUnitTemplateDefinition, TileFlagDefinition.E_TileFlagTag tileFlagTag, UnitCreationSettings unitCreationSettings)
	{
		Tile randomSpawnableTileWithFlag = TileMapManager.GetRandomSpawnableTileWithFlag(tileFlagTag, enemyUnitTemplateDefinition);
		if (randomSpawnableTileWithFlag == null)
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError("Cannot find a non-null tile for actor spawn! Ignoring spawn", CLogLevel.MAJOR);
			return null;
		}
		if (!(enemyUnitTemplateDefinition is EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition))
		{
			return CreateEnemyUnit(enemyUnitTemplateDefinition, randomSpawnableTileWithFlag, unitCreationSettings);
		}
		return CreateEliteEnemyUnit(eliteEnemyUnitTemplateDefinition, randomSpawnableTileWithFlag, unitCreationSettings);
	}

	public static EnemyUnit CreateEnemyUnit(EnemyUnitController unitController, Tile tile, UnitCreationSettings unitCreationSettings, bool onLoad = false, int saveVersion = -1)
	{
		TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Add(unitController.EnemyUnit);
		if (unitCreationSettings.IgnoreFromEnemyUnitsCount)
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemyUnitsToIgnoreFromCount++;
		}
		InitCreatedEnemy(tile, unitController, unitController.EnemyUnit.EnemyUnitView, unitCreationSettings);
		TPSingleton<EnemyUnitManager>.Instance.StartCoroutine(InitCreatedEnemyOnAppear(unitController, unitController.EnemyUnit.EnemyUnitView, unitCreationSettings));
		return unitController.EnemyUnit;
	}

	public static EliteEnemyUnit CreateEliteEnemyUnit(EliteEnemyUnitController eliteEnemyUnitController, Tile tile, UnitCreationSettings unitCreationSettings, EnemyAffixDefinition enemyAffixDefinition = null, bool onLoad = false, int saveVersion = -1)
	{
		CreateEnemyUnit(eliteEnemyUnitController, tile, unitCreationSettings, onLoad, saveVersion);
		return eliteEnemyUnitController.EliteEnemyUnit;
	}

	public static void AddBonePilePercentage(EnemyUnit enemyUnit)
	{
		if (!TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(enemyUnit.OriginTile, out var value))
		{
			value = new Dictionary<string, int>();
			TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.Add(enemyUnit.OriginTile, value);
		}
		BoneZoneDefinition boneZoneDefinition = null;
		foreach (BoneZoneDefinition boneZoneDefinition2 in BonePileDatabase.BoneZonesDefinition.BoneZoneDefinitions)
		{
			if (boneZoneDefinition2.MaxMagicCircleDistance > -1 && enemyUnit.OriginTile.DistanceToMagicCircle <= boneZoneDefinition2.MaxMagicCircleDistance)
			{
				boneZoneDefinition = boneZoneDefinition2;
				break;
			}
			if (boneZoneDefinition2.MinHavenDistance > -1 && enemyUnit.OriginTile.DistanceToCity >= boneZoneDefinition2.MinHavenDistance)
			{
				boneZoneDefinition = boneZoneDefinition2;
				break;
			}
		}
		if (boneZoneDefinition == null)
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError("Could not find a bone zone for tile " + enemyUnit.OriginTile.Id + "!");
			return;
		}
		if (!BonePileDatabase.BonePileGeneratorsDefinition.GeneratorsByZoneId.TryGetValue(boneZoneDefinition.Id, out var value2))
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError("Could not find BonePileGeneratorDefinition for ZoneId " + boneZoneDefinition.Id + ". Using first BonePileGeneratorDefinition in database.");
			value2 = BonePileDatabase.BonePileGeneratorsDefinition.GeneratorsByZoneId.FirstOrDefault().Value;
		}
		if (!value2.TryGetGroup(enemyUnit.EnemyUnitTemplateDefinition.Tier, enemyUnit is EliteEnemyUnit, out var boneGroup))
		{
			TPSingleton<EnemyUnitManager>.Instance.Log($"Could not find a bone group for {enemyUnit.Id} (tier {enemyUnit.EnemyUnitTemplateDefinition.Tier})!", CLogLevel.DETAILED);
			return;
		}
		foreach (BonePileGeneratorDefinition.BonePileGenerationInfo item in boneGroup.BonePileGenerationInfo)
		{
			value.AddValueOrCreateKey(item.BuildingId, item.AddedPercentage, (int a, int b) => a + b);
		}
	}

	public static void DestroyUnit(EnemyUnit enemyUnit)
	{
		TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Remove(enemyUnit);
		if (!enemyUnit.HasBeenDestroyed && enemyUnit.IgnoreFromEnemyUnitsCount)
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemyUnitsToIgnoreFromCount--;
		}
		if (enemyUnit.IsBossPhaseActor)
		{
			TPSingleton<BossManager>.Instance.HandleBossPhaseActorDeath(enemyUnit);
		}
		enemyUnit.UnitView.StartCoroutine(enemyUnit.UnitView.DisableWhenPossible());
		GameView.TopScreenPanel.TurnPanel.PhasePanel.RefreshSoulsText();
		GameView.TopScreenPanel.TurnPanel.PhasePanel.RefreshNightSliderValues();
		if (!NightTurnsManager.HandleUnitsDeath(enemyUnit) && TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(TileObjectSelectionManager.SelectedPlayableUnit);
		}
		enemyUnit.HasBeenDestroyed = true;
		enemyUnit.Log("Destroyed.");
	}

	public static void EndTurn()
	{
		for (int i = 0; i < TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count; i++)
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemyUnits[i].UnitController.EndTurn();
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			TPSingleton<EnemyUnitManager>.Instance.ClearEnemiesGroundAndIconFeedback();
		}
	}

	public static EnemyUnitTooltip GetDisplayedEnemyTootlip()
	{
		if (EliteEnemyUnitInfoPanel.Displayed)
		{
			return EliteEnemyUnitInfoPanel;
		}
		if (EnemyUnitInfoPanel.Displayed)
		{
			return EnemyUnitInfoPanel;
		}
		return null;
	}

	public static Color GetEnemyAttackFeedbackColor(EnemyUnitTemplateDefinition enemyUnitTemplateDefinition)
	{
		if (!EnemyAttackFeedbackColorsByTemplate.TryGetValue(enemyUnitTemplateDefinition, out var value))
		{
			value = ColorExtensions.GenerateRandomColor();
			EnemyAttackFeedbackColorsByTemplate.Add(enemyUnitTemplateDefinition, value);
		}
		return value;
	}

	public static EnemyUnitTooltip GetTooltipForEnemy(TheLastStand.Model.Unit.Unit unit)
	{
		if (!(unit is EliteEnemyUnit))
		{
			return EnemyUnitInfoPanel;
		}
		return EliteEnemyUnitInfoPanel;
	}

	public static float GetUnitProgressionStatBonus(string unitId, UnitStatDefinition.E_Stat unitStat)
	{
		if (TPSingleton<EnemyUnitManager>.Instance.statsProgressionBonusForToday == null)
		{
			TPSingleton<EnemyUnitManager>.Instance.statsProgressionBonusForToday = new Dictionary<string, Dictionary<UnitStatDefinition.E_Stat, float>>();
		}
		if (TPSingleton<EnemyUnitManager>.Instance.statsProgressionBonusForToday.TryGetValue(unitId, out var value))
		{
			if (value.TryGetValue(unitStat, out var value2))
			{
				return value2;
			}
			return 0f;
		}
		TPSingleton<EnemyUnitManager>.Instance.LogWarning($"Someone asked for the progression bonus for {unitId} and stat {unitStat}, but this unit was NOT planned for tonight. Adding a void dictionary to their stat bonus.", CLogLevel.DETAILED);
		TPSingleton<EnemyUnitManager>.Instance.statsProgressionBonusForToday[unitId] = new Dictionary<UnitStatDefinition.E_Stat, float>();
		return 0f;
	}

	public static void HideInfoPanels()
	{
		EnemyUnitInfoPanel.Hide();
		EliteEnemyUnitInfoPanel.Hide();
	}

	public static bool IsAnyEnemyTooltipDisplayed()
	{
		if (!EliteEnemyUnitInfoPanel.Displayed)
		{
			return EnemyUnitInfoPanel.Displayed;
		}
		return true;
	}

	public static void OnGameStateChange(Game.E_State state)
	{
		switch (state)
		{
		case Game.E_State.UnitPreparingSkill:
			if (!TPSingleton<EnemyUnitManager>.Instance.isDisplayingAllEnemiesMoveRange)
			{
				TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
			}
			return;
		case Game.E_State.Management:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.Construction:
		case Game.E_State.Wait:
			return;
		}
		HideInfoPanels();
		if (!TPSingleton<EnemyUnitManager>.Instance.isDisplayingAllEnemiesMoveRange)
		{
			TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
		}
	}

	public static void StartTurn()
	{
		TPSingleton<EnemyUnitManager>.Instance.Log("Starting enemy turn", CLogLevel.MAJOR);
		TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
		if (!TPSingleton<NightTurnsManager>.Instance.DisableAI)
		{
			TPSingleton<EnemyUnitManager>.Instance.AssertNoMoreDyingEnemiesWithContagion();
			for (int i = 0; i < TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count; i++)
			{
				TPSingleton<EnemyUnitManager>.Instance.EnemyUnits[i].UnitController.StartTurn();
			}
			TPSingleton<EnemyUnitManager>.Instance.ApplyContagionToDyingEnemies();
			PanicManager.Panic.PanicView.RefreshPanicValue();
		}
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
			{
			case Game.E_DayTurn.Production:
				SpawnWaveManager.GenerateSpawnWave();
				TPSingleton<EnemyUnitManager>.Instance.EnemyUnitsToIgnoreFromCount = 0;
				break;
			case Game.E_DayTurn.Deployment:
				if (SpawnWaveManager.CurrentSpawnWave == null)
				{
					SpawnWaveManager.GenerateSpawnWave();
				}
				break;
			}
			break;
		case Game.E_Cycle.Night:
			RandomManager.ClearSavedState(TPSingleton<EnemyUnitManager>.Instance);
			if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
			{
				TPSingleton<EnemyUnitManager>.Instance.DisplayEnemiesIconAndTileFeedback();
			}
			break;
		}
	}

	private static void InitCreatedEnemy(Tile tile, EnemyUnitController enemyController, UnitView unitView, UnitCreationSettings unitCreationSettings)
	{
		tile.TileController.SetUnit(enemyController.Unit);
		unitView.Unit = enemyController.Unit;
		if (unitCreationSettings.BossPhaseActorId != null)
		{
			enemyController.EnemyUnit.BossPhaseActorId = unitCreationSettings.BossPhaseActorId;
		}
		enemyController.EnemyUnit.IsExecutingSkillOnSpawn = unitCreationSettings.CastSpawnSkill && enemyController.EnemyUnit.HasSpawnGoals();
		if (enemyController.EnemyUnit.IsExecutingSkillOnSpawn)
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemiesExecutingSkillsOnSpawn.Add(enemyController.EnemyUnit);
		}
		TPSingleton<EnemyUnitManager>.Instance.Log("Spawned unit " + ((EnemyUnit)unitView.Unit).SpecificId);
	}

	private static IEnumerator InitCreatedEnemyOnAppear(EnemyUnitController enemyController, UnitView unitView, UnitCreationSettings unitCreationSettings)
	{
		if (unitCreationSettings.PlaySpawnCutscene && enemyController.EnemyUnit.EnemyUnitTemplateDefinition.SpawnCutsceneId != null)
		{
			CutsceneData cutsceneData = new CutsceneData(null, enemyController.EnemyUnit.OriginTile, enemyController.EnemyUnit);
			GenericCutsceneView genericCutsceneView = TPSingleton<CutsceneManager>.Instance.GetGenericCutsceneView();
			genericCutsceneView.Init(enemyController.EnemyUnit.EnemyUnitTemplateDefinition.SpawnCutsceneId, cutsceneData);
			CutsceneManager.PlayCutscene(genericCutsceneView);
			yield return genericCutsceneView.WaitUntilIsOver;
			enemyController.TriggerAffixes(E_EffectTime.OnCreationAfterViewInitialized);
			yield break;
		}
		float appearanceDelay = enemyController.EnemyUnit.EnemyUnitTemplateDefinition.AppearanceDelay;
		if (appearanceDelay > 0f)
		{
			yield return SharedYields.WaitForSeconds(appearanceDelay);
		}
		unitView.InitVisuals(unitCreationSettings.PlaySpawnAnim);
		unitView.UpdatePosition();
		unitView.LookAtDirection(enemyController.Unit.LookDirection);
		enemyController.TriggerAffixes(E_EffectTime.OnCreationAfterViewInitialized);
		if (unitCreationSettings.WaitSpawnAnim)
		{
			yield return unitView.WaitUntilAnimatorStateIsIdle;
		}
		if (unitCreationSettings.CastSpawnSkill)
		{
			float castSpawnSkillDelay = enemyController.EnemyUnit.EnemyUnitTemplateDefinition.CastSpawnSkillDelay;
			if (castSpawnSkillDelay > 0f)
			{
				yield return SharedYields.WaitForSeconds(castSpawnSkillDelay);
			}
			enemyController.ExecuteSpawnGoals();
		}
		unitView.UnitHUD.ToggleFollowElement(toggle: true);
		yield return SharedYields.WaitForEndOfFrame;
		unitView.UnitHUD.ToggleFollowElement(toggle: false);
	}

	public BraziersDefinition.GuardiansGroup GetGuardiansGroupToSpawn()
	{
		if (guardiansGroupToSpawn != null)
		{
			return guardiansGroupToSpawn;
		}
		GuardiansGroupsToSpawn guardiansGroupsToSpawn = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.BrazierDefinition.GetGuardiansGroupsToSpawn();
		guardiansGroupToSpawn = ListHelpers.GetRandomItemFromWeights(guardiansGroupsToSpawn, TPSingleton<SpawnWaveManager>.Instance);
		return guardiansGroupToSpawn;
	}

	public EnemyUnitTemplateDefinition GetGuardianToSpawn()
	{
		return GetGuardianToSpawn(GetGuardiansGroupToSpawn());
	}

	public EnemyUnitTemplateDefinition GetGuardianToSpawn(BraziersDefinition.GuardiansGroup guardiansGroup)
	{
		if (!spawnedGuardians.ContainsKey(guardiansGroup))
		{
			List<int> list = new List<int>(guardiansGroup.GuardiansPerWeight.Count);
			for (int i = 0; i < guardiansGroup.GuardiansPerWeight.Count; i++)
			{
				list.Add(1);
			}
			spawnedGuardians.Add(guardiansGroup, list);
		}
		float num = -1f;
		int num2 = -1;
		for (int j = 0; j < spawnedGuardians[guardiansGroup].Count; j++)
		{
			float num3 = (float)guardiansGroup.GuardiansPerWeight[j].Item2 / (float)spawnedGuardians[guardiansGroup][j];
			if (num3 > num)
			{
				num = num3;
				num2 = j;
			}
		}
		if (num2 == -1)
		{
			LogError("Something went wrong with the weights algorithm for the guardians ! Could not find a valid index.");
			return null;
		}
		if (!EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(guardiansGroup.GuardiansPerWeight[num2].Item1, out var value))
		{
			LogError("Could not find guardian with id " + guardiansGroup.GuardiansPerWeight[num2].Item1 + ".");
			return null;
		}
		spawnedGuardians[guardiansGroup][num2]++;
		return value;
	}

	public void ResetSpawnedGuardians()
	{
		spawnedGuardians.Clear();
		guardiansGroupToSpawn = null;
	}

	public void AssertNoMoreDyingEnemiesWithContagion()
	{
		if (DyingEnemiesWithContagion.Count > 0)
		{
			LogError($"There are {DyingEnemiesWithContagion.Count} dying enemies with contagion registered. This is not supposed to happen.", CLogLevel.MAJOR);
			DyingEnemiesWithContagion.Clear();
		}
	}

	public void ApplyContagionToDyingEnemies()
	{
		foreach (TheLastStand.Model.Unit.Unit item in DyingEnemiesWithContagion)
		{
			item?.UnitController.ApplyContagionToAdjacentUnits();
		}
		DyingEnemiesWithContagion.Clear();
	}

	public void DisplayEnemiesIconAndTileFeedback()
	{
		List<Tile> list = new List<Tile>();
		List<Tile> list2 = new List<Tile>();
		List<EnemyUnit> list3 = new List<EnemyUnit>(EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
		list3.AddRange(EnemyUnits);
		list3.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
		for (int i = 0; i < list3.Count; i++)
		{
			if (!list3[i].IsDead && !list3[i].IsDeathRattling)
			{
				list3[i].UnitView.UnitHUD.DisplayIconFeedback();
				if (list3[i].WillDieByPoison)
				{
					list2.AddRange(list3[i].OccupiedTiles);
				}
				else if (list3[i].ShouldCausePanic)
				{
					list.AddRange(list3[i].OccupiedTiles);
				}
			}
		}
		TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, list, "View/Tiles/Feedbacks/PanicOnEnemy");
		TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, list2, "View/Tiles/Feedbacks/PoisonDeath");
	}

	public void DisplayOneEnemyReachableTiles(EnemyUnit enemyUnit)
	{
		ClearAllReachableTiles();
		TPSingleton<TileMapView>.Instance.AddEnemyReachableTiles(enemyUnit.GetReachableTiles());
		TPSingleton<TileMapView>.Instance.DisplayAllEnemiesReachableTiles();
	}

	public override void ExecuteTurnForBehaviorModels(List<IBehaviorModel> behaviors)
	{
		base.IsDone = false;
		sortedEnemies = RemoveSkippedBehaviours(behaviors);
		sortedEnemies = SortBehaviors(sortedEnemies);
		ComputeGoals(sortedEnemies);
		GatherUnitsByGroups(sortedEnemies);
		if (skillGroups.Count > 0)
		{
			Log("Executing turn for skillGroups.", CLogLevel.MAJOR);
			TPSingleton<EnemyUnitManager>.Instance.StartCoroutine(PrepareSkillsForGroups());
		}
		else
		{
			Log("Behaviors skill groups are all empty, end of turn.", CLogLevel.MAJOR);
			base.IsDone = true;
		}
	}

	public void ExileAllUnits(bool countAsKills, bool disableDieAnim = false, List<TheLastStand.Model.Unit.Unit> unitsToSkip = null)
	{
		for (int num = EnemyUnits.Count - 1; num >= 0; num--)
		{
			if ((unitsToSkip == null || !unitsToSkip.Contains(EnemyUnits[num])) && !EnemyUnits[num].IsDead)
			{
				if (countAsKills)
				{
					TrophyManager.AddEnemyKill(Mathf.RoundToInt(EnemyUnits[num].UnitStatsController.UnitStats.Stats[UnitStatDefinition.E_Stat.DamnedSoulsEarned].FinalClamped));
				}
				bool forcePlayDieAnim = !disableDieAnim && EnemyUnits[num].EnemyUnitView != null && EnemyUnits[num].EnemyUnitView.IsVisible;
				EnemyUnits[num].EnemyUnitController.PrepareForExile(forcePlayDieAnim);
				EnemyUnits[num].EnemyUnitController.ExecuteExile();
			}
		}
	}

	public void ForceHideHUD()
	{
		foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
		{
			enemyUnit.EnemyUnitView.EnemyUnitHUD.ForceHideHUD();
		}
	}

	public void ForceRefreshHUD()
	{
		foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
		{
			enemyUnit.EnemyUnitView.EnemyUnitHUD.ForceDisplayHUD();
		}
	}

	public void Init()
	{
		sortedEnemies = new List<IBehaviorModel>();
		skillGroups.Clear();
		EnemiesDeathRattling = new List<EnemyUnit>();
		EnemiesDying = new HashSet<EnemyUnit>();
		EnemiesExecutingSkillsOnSpawn = new HashSet<EnemyUnit>();
		WaitUntilDeathRattlingEnemiesAreDone = new WaitUntil(() => EnemiesDeathRattling.Count == 0);
		WaitUntilDyingEnemiesAreDone = new WaitUntil(() => EnemiesDying.Count == 0);
		WaitUntilEnemiesExecutingSkillsOnSpawnAreDone = new WaitUntil(() => EnemiesExecutingSkillsOnSpawn.Count == 0);
	}

	public IEnumerator MoveUnitsCoroutine(List<EnemyUnit> enemyUnits, bool moveCamera = false)
	{
		sortedEnemies.Clear();
		sortedEnemies = RemoveSkippedBehaviours(enemyUnits.Cast<IBehaviorModel>().ToList(), updateSkippedTurns: false);
		sortedEnemies = SortBehaviors(sortedEnemies, shuffleList: false);
		List<EnemyUnit> enemies = sortedEnemies.Cast<EnemyUnit>().ToList();
		SetUnitsComputingStepsTo(enemies, IBehaviorModel.E_GoalComputingStep.BeforeMoving);
		ComputeGoals(sortedEnemies);
		if (moveCamera)
		{
			List<EnemyUnit> list = new List<EnemyUnit>();
			foreach (EnemyUnit item in enemies)
			{
				if (item.Path[item.Path.Count - 1] != item.OriginTile)
				{
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				Vector3 zero = Vector3.zero;
				foreach (EnemyUnit item2 in list)
				{
					zero += item2.Path[item2.Path.Count - 1].TileView.transform.position;
				}
				zero /= (float)list.Count;
				ACameraView.MoveTo(zero, CameraView.AnimationMoveSpeed);
				yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
			}
		}
		MoveUnitsTaskGroup = new TaskGroup();
		GameController.SetState(Game.E_State.Wait);
		HashSet<string> hashSet = new HashSet<string>();
		for (int num = sortedEnemies.Count - 1; num >= 0; num--)
		{
			EnemyUnit enemyUnit = enemies[num];
			if (enemyUnit.Health > 0f)
			{
				MoveUnitsTaskGroup.AddTask(enemyUnit.EnemyUnitController.PrepareForMovement());
				if (enemyUnit.Path.Count > 1)
				{
					string text = ((enemyUnit.EnemyUnitTemplateDefinition.MoveSoundFolderName != string.Empty) ? enemyUnit.EnemyUnitTemplateDefinition.MoveSoundFolderName : "Clawer");
					if (text != "None" && !hashSet.Contains(text))
					{
						hashSet.Add(text);
					}
				}
			}
			if (enemyUnit.TargetTile != null && enemyUnit.TargetTile != enemyUnit.OriginTile)
			{
				enemyUnit.Log($"I will not reach my current targeted tile {enemyUnit.TargetTile.Position}, so I'm setting it to null", CLogLevel.DETAILED);
				enemyUnit.OccupiedTiles.ForEach(delegate(Tile tile)
				{
					tile.WillBeReachedBy = null;
				});
				enemyUnit.TargetTile = null;
			}
		}
		PlayMoveSounds(hashSet);
		MoveUnitsTaskGroup.OnCompleteAction = delegate
		{
			MoveUnitsTaskGroup = null;
		};
		MoveUnitsTaskGroup.Run();
		yield return new WaitUntil(() => MoveUnitsTaskGroup == null);
		SetUnitsComputingStepsTo(enemies, IBehaviorModel.E_GoalComputingStep.AfterMoving);
	}

	public void TriggerEnemiesAffix(E_EffectTime effectTime)
	{
		for (int i = 0; i < EnemyUnits.Count; i++)
		{
			EnemyUnits[i].EnemyUnitController.TriggerAffixes(effectTime);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		sortedEnemies = new List<IBehaviorModel>();
		skillGroups = new SkillCasterAttackGroups();
		EnemiesDeathRattling = new List<EnemyUnit>();
		EnemiesDying = new HashSet<EnemyUnit>();
		EnemiesExecutingSkillsOnSpawn = new HashSet<EnemyUnit>();
		WaitUntilDeathRattlingEnemiesAreDone = new WaitUntil(() => EnemiesDeathRattling.Count == 0);
		WaitUntilDyingEnemiesAreDone = new WaitUntil(() => EnemiesDying.Count == 0);
		WaitUntilEnemiesExecutingSkillsOnSpawnAreDone = new WaitUntil(() => EnemiesExecutingSkillsOnSpawn.Count == 0);
	}

	private void ActivateAllEnemiesReachableTilesDisplay()
	{
		isDisplayingAllEnemiesMoveRange = true;
		if (CanDisplayAllEnemiesReachableTiles())
		{
			DisplayAllEnemiesReachableTiles();
			if (CanDisplayOneEnemyReachableTiles() && TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
		}
	}

	private bool CanDisplayAllEnemiesReachableTiles()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Wait && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill)
			{
				return TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingPreparingSkill;
			}
			return true;
		}
		return false;
	}

	private bool CanDisplayOneEnemyReachableTiles()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management)
			{
				return TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Wait;
			}
			return true;
		}
		return false;
	}

	private void ClearAllReachableTiles()
	{
		TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
		TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
	}

	private void DeactivateAllEnemiesReachableTilesDisplay()
	{
		isDisplayingAllEnemiesMoveRange = false;
		if (!CanDisplayOneEnemyReachableTiles() || !TryDisplayHoveredEnemyReachableTiles())
		{
			TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
		}
	}

	private void DisplayAllEnemiesReachableTiles()
	{
		TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
		List<EnemyUnit> list = new List<EnemyUnit>(EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
		list.AddRange(EnemyUnits);
		list.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
		foreach (EnemyUnit item in list)
		{
			if (!item.OriginTile.HasFog && !item.IsDead && !item.IsDeathRattling && !item.WillDieByPoison)
			{
				TPSingleton<TileMapView>.Instance.AddEnemyReachableTiles(item.GetReachableTiles());
			}
		}
		TPSingleton<TileMapView>.Instance.DisplayAllEnemiesReachableTiles();
	}

	protected override IEnumerator ExecuteSkillsForGroups(SkillCasterCluster skillCasterCluster)
	{
		SkillCasterAttackGroup.E_Target targetType = skillCasterCluster.TargetType;
		Log($"Executing skills for SkillCasterAttackGroups in Cluster {skillCasterCluster.ClusterOrder} with target {targetType}");
		GameController.SetState(Game.E_State.UnitExecutingSkill);
		HashSet<string> hashSet = new HashSet<string>();
		TotalCasters = skillCasterCluster.SkillCasterAttackGroups.Select((SkillCasterAttackGroup x) => x.GoalsToExecute.Count).Sum();
		HashSet<PlayableUnit> targetedPlayableUnits = skillCasterCluster.TargetedPlayableUnits;
		if (targetedPlayableUnits != null && targetedPlayableUnits.Count > 0 && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying)
		{
			foreach (PlayableUnit targetedPlayableUnit in skillCasterCluster.TargetedPlayableUnits)
			{
				if (targetedPlayableUnit.Health > 0f && targetedPlayableUnit.LastTurnHealth - targetedPlayableUnit.Health > targetedPlayableUnit.HealthTotal * BarkManager.BigAmountOfHealthLost)
				{
					TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitLoseABigAmountofHealth", targetedPlayableUnit, BarkManager.DelayPostAttack);
				}
			}
		}
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in skillCasterCluster.SkillCasterAttackGroups)
		{
			ExecuteSkillsForGroup(skillCasterAttackGroup);
			if (!hashSet.Contains(skillCasterAttackGroup.SkillSoundId))
			{
				hashSet.Add(skillCasterAttackGroup.SkillSoundId);
				PlaySkillSound(skillCasterAttackGroup.SkillSoundId, skillCasterAttackGroup.GoalsToExecute.Count, skillCasterAttackGroup.GoalsToExecute[0].Goal.Owner.OriginTile, skillCasterAttackGroup.GoalsToExecute[0].TargetTileInfo.Tile);
			}
		}
		yield return new WaitUntil(() => CheckIfSkillGroupsAreDoneWithSkillExecution(skillCasterCluster.SkillCasterAttackGroups));
		if (targetType == SkillCasterAttackGroup.E_Target.ATTACK_HERO && (skillCasterCluster.TargetedPlayableUnits?.Any((PlayableUnit playableUnit) => playableUnit.Health > 0f) ?? false))
		{
			if (EnemyUnitDatabase.HitByEnemySoundDefinitions.TryGetValue("PlayableUnits", out var value))
			{
				string soundId = value.GetSoundId(TotalCasters);
				ObjectPooler.GetPooledComponent("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab).Play(ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/PlayableUnitHits/" + soundId));
			}
			else
			{
				LogWarning("No sound found for PlayableUnits on hit");
			}
			yield break;
		}
		switch (targetType)
		{
		case SkillCasterAttackGroup.E_Target.ATTACK_BUILDING:
		{
			if (EnemyUnitDatabase.HitByEnemySoundDefinitions.TryGetValue("Buildings", out var value3))
			{
				string soundId3 = value3.GetSoundId(TotalCasters);
				ObjectPooler.GetPooledComponent("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab).Play(ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/BuildingHits/" + soundId3));
			}
			else
			{
				LogWarning("No sound found for Buildings on hit");
			}
			break;
		}
		case SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE:
		{
			if (EnemyUnitDatabase.HitByEnemySoundDefinitions.TryGetValue("MagicCircle", out var value2))
			{
				string soundId2 = value2.GetSoundId(TotalCasters);
				ObjectPooler.GetPooledComponent("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab).Play(ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/BuildingHits/" + soundId2));
			}
			else
			{
				LogWarning("No sound found for MagicCircle on hit");
			}
			break;
		}
		}
	}

	private void ClearEnemiesGroundAndIconFeedback()
	{
		List<EnemyUnit> list = new List<EnemyUnit>(EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
		list.AddRange(EnemyUnits);
		list.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			list[num].EnemyUnitView.EnemyUnitHUD.DisplayIconFeedback(show: false);
		}
		TileMapView.ClearTiles(TileMapView.UnitFeedbackTilemap);
	}

	public void HookZoneControlEnemy(EnemyUnitView enemyUnitView, bool hook)
	{
		if (hook)
		{
			zoneControlEnemies.Add(enemyUnitView);
		}
		else
		{
			zoneControlEnemies.Remove(enemyUnitView);
		}
		TileMapView.ClearTiles(TileMapView.EnemiesHoverAreaOfEffectTilemap);
		foreach (EnemyUnitView zoneControlEnemy in zoneControlEnemies)
		{
			zoneControlEnemy.DisplayZoneControlSkill();
		}
	}

	public void PlayMoveSounds(HashSet<string> unitMoveSoundIds)
	{
		if (unitMoveSoundIds.Count != 0)
		{
			StartCoroutine(PlayMoveSoundsCoroutine(unitMoveSoundIds));
		}
	}

	private IEnumerator PlayMoveSoundsCoroutine(HashSet<string> unitMoveSoundIds)
	{
		string text = unitMoveSoundIds.First();
		List<Tuple<string, float>> unitMoveSoundIdsWithDelay = new List<Tuple<string, float>>
		{
			new Tuple<string, float>(text, 0f)
		};
		unitMoveSoundIds.Remove(text);
		foreach (string unitMoveSoundId in unitMoveSoundIds)
		{
			unitMoveSoundIdsWithDelay.Add(new Tuple<string, float>(unitMoveSoundId, UnityEngine.Random.Range(0f, moveMaxRandomDelay)));
		}
		unitMoveSoundIdsWithDelay.Sort((Tuple<string, float> a, Tuple<string, float> b) => (a.Item2 > b.Item2) ? 1 : ((a.Item2 != b.Item2) ? (-1) : 0));
		int index = 0;
		float time = 0f;
		while (index < unitMoveSoundIdsWithDelay.Count)
		{
			for (; index < unitMoveSoundIdsWithDelay.Count && unitMoveSoundIdsWithDelay[index].Item2 <= time; index++)
			{
				AudioClip[] array = ResourcePooler<AudioClip>.LoadAllOnce("Sounds/SFX/Enemy/Move/" + unitMoveSoundIdsWithDelay[index].Item1);
				if (array != null && array.Length != 0)
				{
					ObjectPooler.GetPooledComponent("Enemies Move", enemyMoveSFXPrefab).Play(array.RandomElement());
				}
				else
				{
					LogError(" The move sounds folder (" + unitMoveSoundIdsWithDelay[index].Item1 + ") doesn't exist or is empty. Check this folder name in EnemyUnitTemplateDefinition.xml", CLogLevel.DETAILED);
				}
			}
			yield return null;
			time += Time.deltaTime;
		}
	}

	private void RefreshReachableTiles()
	{
		if (isDisplayingAllEnemiesMoveRange)
		{
			return;
		}
		if (!TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
		{
			if (TileMapView.EnemiesReachableTiles.Count != 0 && (TPSingleton<GameManager>.Instance.Game.Cursor.Tile == null || !(TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Unit is EnemyUnit)) && !TileObjectSelectionManager.HasEnemyUnitSelected)
			{
				TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
			}
		}
		else
		{
			UpdateAllReachableTiles();
		}
	}

	private void SetUnitsComputingStepsTo(List<EnemyUnit> enemyUnits, IBehaviorModel.E_GoalComputingStep computingStep)
	{
		foreach (EnemyUnit enemyUnit in enemyUnits)
		{
			enemyUnit.GoalComputingStep = computingStep;
		}
	}

	private bool TryDisplayHoveredEnemyReachableTiles()
	{
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (tile != null && !tile.HasFog && tile.Unit is EnemyUnit { State: not TheLastStand.Model.Unit.Unit.E_State.Dead, IsDeathRattling: false } enemyUnit)
		{
			DisplayOneEnemyReachableTiles(enemyUnit);
			return true;
		}
		return false;
	}

	private void SelectNextSkill(bool nextSkill)
	{
		if (TileObjectSelectionManager.SelectedUnit is EnemyUnit || TileObjectSelectionManager.SelectedUnit is BossUnit)
		{
			TPSingleton<EnemyUnitManagementView>.Instance.SkillBar.SelectNextSkill(nextSkill);
		}
	}

	private void Update()
	{
		Debug_Update();
		RefreshReachableTiles();
		if (InputManager.GetButtonDown(59))
		{
			ActivateAllEnemiesReachableTilesDisplay();
		}
		else if (InputManager.GetButtonUp(59))
		{
			DeactivateAllEnemiesReachableTilesDisplay();
		}
		else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
		{
			TPSingleton<EnemyUnitManagementView>.Instance.SkillBar.JoystickSkillBar.DeselectCurrentSkill();
		}
		else if (InputManager.GetButtonDown(83))
		{
			SelectNextSkill(nextSkill: false);
		}
		else if (InputManager.GetButtonDown(82))
		{
			SelectNextSkill(nextSkill: true);
		}
		if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
		{
			TheLastStand.Model.Cursor cursor = TPSingleton<GameManager>.Instance.Game.Cursor;
			lastTileWasEnemyUnit = cursor.Tile?.Unit != null && cursor.Tile.Unit is EnemyUnit;
		}
	}

	private void UpdateAllReachableTiles()
	{
		if (CanDisplayOneEnemyReachableTiles() && !TryDisplayHoveredEnemyReachableTiles())
		{
			TPSingleton<TileMapView>.Instance.ClearAllEnemiesReachableTiles();
			if (lastTileWasEnemyUnit && TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
		}
	}

	protected override OneShotSound GetPooledSkillSoundAudioSource()
	{
		return ObjectPooler.GetPooledComponent("EnemySkillSFX", SoundManager.EnemySkillSFXPrefab);
	}

	protected override OneShotSound GetSpatializedPooledSkillSoundAudioSource()
	{
		return ObjectPooler.GetPooledComponent("EnemySkillSFX Spatialized", SoundManager.EnemySkillSpatializedSFXPrefab);
	}

	protected override string GetSkillSoundClipPathFormat()
	{
		return "Sounds/SFX/Enemy/Skills/{0}/SFX_{0}_{1}";
	}

	protected override string GetSkillSoundLaunchPathFormat()
	{
		return "Sounds/SFX/Enemy/Skills/{0}/SFX_{0}_{1}_Launch";
	}

	protected override string GetSkillSoundImpactPathFormat()
	{
		return "Sounds/SFX/Enemy/Skills/{0}/SFX_{0}_{1}_Impact";
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (!(container is SerializedEnemyUnits serializedEnemyUnits))
		{
			return;
		}
		foreach (SerializedEnemyUnit serializedEnemyUnit in serializedEnemyUnits.EnemyUnits)
		{
			TheLastStand.Model.Building.Building linkedBuilding = null;
			if (serializedEnemyUnit.LinkedBuilding.HasValue)
			{
				linkedBuilding = TPSingleton<BuildingManager>.Instance.Buildings.FirstOrDefault((TheLastStand.Model.Building.Building x) => x.RandomId == serializedEnemyUnit.LinkedBuilding);
			}
			UnitCreationSettings unitCreationSettings = new UnitCreationSettings(serializedEnemyUnit.BossPhaseActorId, castSpawnSkill: false, playSpawnAnim: false, playSpawnCutscene: false, waitSpawnAnim: false, serializedEnemyUnit.OverrideVariantId, linkedBuilding, serializedEnemyUnit.IsGuardian, serializedEnemyUnit.IgnoreFromEnemyUnitsCount);
			EnemyUnitView pooledComponent = ObjectPooler.GetPooledComponent("EnemyUnitViews", TPSingleton<EnemyUnitManager>.Instance.enemyUnitViewPrefab, TPSingleton<EnemyUnitManager>.Instance.unitsTransform);
			EnemyUnit enemyUnit = new EnemyUnitController(serializedEnemyUnit, pooledComponent, unitCreationSettings, saveVersion).EnemyUnit;
			CreateEnemyUnit(enemyUnit.EnemyUnitController, enemyUnit.OriginTile, unitCreationSettings, onLoad: true, saveVersion);
			if (enemyUnit.Health <= 0f)
			{
				TPSingleton<EnemyUnitManager>.Instance.LogError(enemyUnit.Id + " is deserialized with no health.", CLogLevel.MAJOR);
				enemyUnit.Health = 1f;
			}
			enemyUnit.EnemyUnitController.UpdatePoisonFeedbackCondition();
			enemyUnit.EnemyUnitController.UpdateInjuryStage();
		}
		foreach (SerializedEliteEnemyUnit serializedEliteEnemyUnit in serializedEnemyUnits.EliteEnemyUnits)
		{
			TheLastStand.Model.Building.Building linkedBuilding2 = null;
			if (serializedEliteEnemyUnit.LinkedBuilding.HasValue)
			{
				linkedBuilding2 = TPSingleton<BuildingManager>.Instance.Buildings.FirstOrDefault((TheLastStand.Model.Building.Building x) => x.RandomId == serializedEliteEnemyUnit.LinkedBuilding);
			}
			UnitCreationSettings unitCreationSettings2 = new UnitCreationSettings(serializedEliteEnemyUnit.BossPhaseActorId, castSpawnSkill: false, playSpawnAnim: false, playSpawnCutscene: false, waitSpawnAnim: false, serializedEliteEnemyUnit.OverrideVariantId, linkedBuilding2, serializedEliteEnemyUnit.IsGuardian, serializedEliteEnemyUnit.IgnoreFromEnemyUnitsCount);
			EliteEnemyUnitView pooledComponent2 = ObjectPooler.GetPooledComponent("EliteEnemyUnitViews", eliteEnemyUnitViewPrefab, TPSingleton<EnemyUnitManager>.Instance.unitsTransform);
			EliteEnemyUnit eliteEnemyUnit = new EliteEnemyUnitController(serializedEliteEnemyUnit, pooledComponent2, unitCreationSettings2, saveVersion).EliteEnemyUnit;
			CreateEliteEnemyUnit(eliteEnemyUnit.EliteEnemyUnitController, eliteEnemyUnit.OriginTile, unitCreationSettings2, null, onLoad: true, saveVersion);
			eliteEnemyUnit.EnemyUnitController.UpdatePoisonFeedbackCondition();
			eliteEnemyUnit.EnemyUnitController.UpdateInjuryStage();
		}
		foreach (SerializedBonePilesPercentages bonePilesPercentage in serializedEnemyUnits.BonePilesPercentages)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			BonePilesPercentages.Add(TileMapManager.GetTile(bonePilesPercentage.TileX, bonePilesPercentage.TileY), dictionary);
			foreach (SerializedBonePilesPercentages.Percentage percentage in bonePilesPercentage.Percentages)
			{
				dictionary.Add(percentage.Id, percentage.Value);
			}
		}
	}

	public ISerializedData Serialize()
	{
		List<SerializedBonePilesPercentages> list = new List<SerializedBonePilesPercentages>();
		foreach (KeyValuePair<Tile, Dictionary<string, int>> bonePilesPercentage in BonePilesPercentages)
		{
			SerializedBonePilesPercentages serializedBonePilesPercentages = new SerializedBonePilesPercentages
			{
				TileX = bonePilesPercentage.Key.X,
				TileY = bonePilesPercentage.Key.Y,
				Percentages = new List<SerializedBonePilesPercentages.Percentage>()
			};
			foreach (KeyValuePair<string, int> item in bonePilesPercentage.Value)
			{
				serializedBonePilesPercentages.Percentages.Add(new SerializedBonePilesPercentages.Percentage
				{
					Id = item.Key,
					Value = item.Value
				});
			}
			list.Add(serializedBonePilesPercentages);
		}
		List<SerializedEnemyUnit> list2 = new List<SerializedEnemyUnit>();
		foreach (EnemyUnit enemyUnit in EnemyUnits)
		{
			if (!enemyUnit.IsDying && !enemyUnit.IsDead && !(enemyUnit is EliteEnemyUnit))
			{
				list2.Add((SerializedEnemyUnit)enemyUnit.Serialize());
			}
		}
		List<SerializedEliteEnemyUnit> list3 = new List<SerializedEliteEnemyUnit>();
		foreach (EnemyUnit enemyUnit2 in EnemyUnits)
		{
			if (enemyUnit2 is EliteEnemyUnit { IsDying: false, IsDead: false } eliteEnemyUnit)
			{
				list3.Add((SerializedEliteEnemyUnit)eliteEnemyUnit.Serialize());
			}
		}
		return new SerializedEnemyUnits
		{
			EnemyUnits = list2,
			EliteEnemyUnits = list3,
			BonePilesPercentages = list
		};
	}

	[DevConsoleCommand(Name = "EnemySpawnDisable")]
	public static void Debug_DisableEnemySpawn()
	{
		TPSingleton<EnemyUnitManager>.Instance.DebugEnemyUnitTemplateDefinition = null;
		TPSingleton<EnemyUnitManager>.Instance.DebugEliteEnemyUnitToSpawnTemplateDefinition = null;
		TPSingleton<BossManager>.Instance.DebugBossUnitTemplateDefinition = null;
	}

	[DevConsoleCommand(Name = "EnemySpawnEnable")]
	public static void Debug_EnableEnemySpawn([StringConverter(typeof(EnemyUnit.StringToEnemyUnitTemplateIdConverter))] string enemyTemplateId = "")
	{
		TPSingleton<BossManager>.Instance.DebugBossUnitTemplateDefinition = null;
		TPSingleton<EnemyUnitManager>.Instance.DebugEliteEnemyUnitToSpawnTemplateDefinition = null;
		if (enemyTemplateId == string.Empty)
		{
			enemyTemplateId = "Clawer";
		}
		if (!EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(enemyTemplateId, out TPSingleton<EnemyUnitManager>.Instance.DebugEnemyUnitTemplateDefinition))
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError("Enemy " + enemyTemplateId + " not found in database!");
		}
	}

	[DevConsoleCommand(Name = "EnemyEliteSpawnEnable")]
	public static void Debug_EnableEliteEnemySpawn([StringConverter(typeof(EliteEnemyUnit.StringToEliteEnemyUnitTemplateIdConverter))] string enemyTemplateId = "", [StringConverter(typeof(EliteEnemyUnit.StringToEliteAffixIdConverter))] string affixId = null)
	{
		TPSingleton<BossManager>.Instance.DebugBossUnitTemplateDefinition = null;
		TPSingleton<EnemyUnitManager>.Instance.DebugEnemyUnitTemplateDefinition = null;
		if (enemyTemplateId == string.Empty)
		{
			enemyTemplateId = "ClawerElite";
		}
		if (!EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.TryGetValue(enemyTemplateId, out TPSingleton<EnemyUnitManager>.Instance.DebugEliteEnemyUnitToSpawnTemplateDefinition))
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError("Elite Enemy " + enemyTemplateId + " not found in database!");
		}
		TPSingleton<EnemyUnitManager>.Instance.debugForcedEliteAffixDefinition = null;
		if (affixId != null && !EnemyUnitDatabase.EnemyAffixDefinitions.TryGetValue(affixId, out TPSingleton<EnemyUnitManager>.Instance.debugForcedEliteAffixDefinition))
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError("Elite Affix " + affixId + " not found in database !");
		}
	}

	private void Debug_Update()
	{
		Debug_UpdateSpawnEnemy();
	}

	private void Debug_UpdateSpawnEnemy()
	{
		if (debugEnemySpawnAllEnabled && InputManager.GetButton(24) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
		{
			Debug_SpawnAllEnemiesAroundCursor();
			return;
		}
		if ((DebugEnemyUnitTemplateDefinition != null || DebugEliteEnemyUnitToSpawnTemplateDefinition != null) && InputManager.GetButton(24) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
		{
			if (!willSpawnEnemyNextFrame)
			{
				willSpawnEnemyNextFrame = true;
				return;
			}
			Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
			if (tile != null)
			{
				EnemyUnitTemplateDefinition debugEnemyUnitTemplateDefinition = DebugEnemyUnitTemplateDefinition;
				if (debugEnemyUnitTemplateDefinition == null || !debugEnemyUnitTemplateDefinition.CanSpawnOn(tile))
				{
					EliteEnemyUnitTemplateDefinition debugEliteEnemyUnitToSpawnTemplateDefinition = DebugEliteEnemyUnitToSpawnTemplateDefinition;
					if (debugEliteEnemyUnitToSpawnTemplateDefinition == null || !debugEliteEnemyUnitToSpawnTemplateDefinition.CanSpawnOn(tile))
					{
						goto IL_0134;
					}
				}
				if (DebugEliteEnemyUnitToSpawnTemplateDefinition != null)
				{
					CreateEliteEnemyUnit(DebugEliteEnemyUnitToSpawnTemplateDefinition, tile, new UnitCreationSettings(), debugForcedEliteAffixDefinition).UnitView.LookAtDirection(debugEnemySpawnDirection);
				}
				else
				{
					CreateEnemyUnit(DebugEnemyUnitTemplateDefinition, tile, new UnitCreationSettings()).UnitView.LookAtDirection(debugEnemySpawnDirection);
				}
			}
			goto IL_0134;
		}
		willSpawnEnemyNextFrame = false;
		return;
		IL_0134:
		willSpawnEnemyNextFrame = false;
	}

	[DevConsoleCommand(Name = "EnemiesSpawnEverywhere")]
	private static void Debug_SpawnEnemiesEverywhere([StringConverter(typeof(EnemyUnit.StringToEnemyUnitTemplateIdConverter))] string enemyTemplateId = "", int spawnEveryXTiles = 1)
	{
		if (enemyTemplateId == string.Empty)
		{
			enemyTemplateId = "Clawer";
		}
		EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = EnemyUnitDatabase.EnemyUnitTemplateDefinitions[enemyTemplateId];
		UnitCreationSettings unitCreationSettings = new UnitCreationSettings();
		int num = 0;
		Tile[] tiles = TPSingleton<TileMapManager>.Instance.TileMap.Tiles;
		foreach (Tile tile in tiles)
		{
			if (tile.Building == null && tile.Unit == null)
			{
				if (num++ == 0)
				{
					CreateEnemyUnit(enemyUnitTemplateDefinition, tile, unitCreationSettings).UnitView.LookAtDirection(TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnDirection);
				}
				if (num >= spawnEveryXTiles)
				{
					num = 0;
				}
			}
		}
	}

	[DevConsoleCommand(Name = "EnemiesSpawnDirection")]
	private static void Debug_SetSpawnEnemiesDirection(GameDefinition.E_Direction direction = GameDefinition.E_Direction.East)
	{
		TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnDirection = direction;
	}

	[DevConsoleCommand(Name = "EnemiesSpawnAllEnable")]
	private static void Debug_SpawnAllEnemiesEnable(bool enable = true, bool includeElites = true)
	{
		TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnAllEnabled = enable;
		TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnAllIncludeElites = includeElites;
	}

	[DevConsoleCommand(Name = "EnemiesSetStatAll")]
	private static void DebugEnemiesSetStatAll([StringConverter(typeof(PlayableUnit.StringToStatIdConverter))] string statId, float newStatValue)
	{
		UnitStatDefinition.E_Stat statType = (UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), statId);
		foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
		{
			enemyUnit.UnitStatsController.SetBaseStat(statType, newStatValue);
			enemyUnit.UnitController.UpdateInjuryStage();
			enemyUnit.UnitView.RefreshInjuryStage();
			enemyUnit.UnitController.DisplayEffects();
		}
		if (TileObjectSelectionManager.HasEnemyUnitSelected)
		{
			UnitManagementView<EnemyUnitManagementView>.Refresh();
			EnemyUnitInfoPanel.Refresh();
		}
	}

	[DevConsoleCommand(Name = "EnemiesKillAllButOne")]
	private static void DebugKillAllEnemiesButOne()
	{
		for (int num = TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count - 1; num >= 1; num--)
		{
			TrophyManager.AddEnemyKill(Mathf.RoundToInt(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits[num].UnitStatsController.UnitStats.Stats[UnitStatDefinition.E_Stat.DamnedSoulsEarned].FinalClamped));
			TPSingleton<EnemyUnitManager>.Instance.EnemyUnits[num].EnemyUnitController.PrepareForExile();
			TPSingleton<EnemyUnitManager>.Instance.EnemyUnits[num].EnemyUnitController.ExecuteExile();
		}
	}

	[DevConsoleCommand(Name = "EnemiesCount")]
	private static int DebugEnemiesCount()
	{
		return TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count;
	}

	[DevConsoleCommand(Name = "EnemiesAddStatusAll")]
	private static void DebugAddStatusToAllEnemies([StringConverter(typeof(Status.StringToStatusForDefaultCommand))] string statusId, int turnsCount, int value = 1)
	{
		if (!Enum.TryParse<Status.E_StatusType>(statusId, out var result))
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError("Incorrect Status (unable to parse)", TPSingleton<EnemyUnitManager>.Instance);
			return;
		}
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = null,
			TurnsCount = turnsCount,
			Value = value
		};
		foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
		{
			SkillManager.AddStatus(enemyUnit, result, statusCreationInfo);
		}
	}

	private static int GetInitialOverrideVariantId(EnemyUnitTemplateDefinition eutd)
	{
		if (eutd.VisualVariants == null || eutd.VisualVariants.Count <= 0)
		{
			return -1;
		}
		return 0;
	}

	private static void Debug_SpawnAllEnemiesAroundCursor()
	{
		TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnAllEnabled = false;
		UnitCreationSettings unitCreationSettings = new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, 0);
		List<EnemyUnitTemplateDefinition> enemyDefs = EnemyUnitDatabase.EnemyUnitTemplateDefinitions.Select((KeyValuePair<string, EnemyUnitTemplateDefinition> kvp) => kvp.Value).ToList();
		List<EliteEnemyUnitTemplateDefinition> eliteEnemyDefs = (TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnAllIncludeElites ? EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.Select((KeyValuePair<string, EliteEnemyUnitTemplateDefinition> kvp) => kvp.Value).ToList() : new List<EliteEnemyUnitTemplateDefinition>());
		if (enemyDefs.Count == 0 && eliteEnemyDefs.Count == 0)
		{
			return;
		}
		int currentVariantId = ((enemyDefs.Count > 0) ? GetInitialOverrideVariantId(enemyDefs[0]) : GetInitialOverrideVariantId(eliteEnemyDefs[0]));
		TileMapManager.GoThroughTilesInRangeUntil(100, TPSingleton<GameManager>.Instance.Game.Cursor.Tile, delegate(Tile tile, Vector2Int distance)
		{
			if (enemyDefs.Count == 0 && eliteEnemyDefs.Count == 0)
			{
				return true;
			}
			if (tile.Unit == null && tile.Building == null)
			{
				unitCreationSettings.OverrideVariantId = currentVariantId;
				if (enemyDefs.Count > 0)
				{
					CreateEnemyUnit(enemyDefs[0], tile, unitCreationSettings).UnitView.LookAtDirection(TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnDirection);
					if (currentVariantId == -1 || ++currentVariantId >= enemyDefs[0].VisualVariants.Count)
					{
						currentVariantId = ((enemyDefs.Count > 1) ? GetInitialOverrideVariantId(enemyDefs[1]) : ((eliteEnemyDefs.Count > 0) ? GetInitialOverrideVariantId(eliteEnemyDefs[0]) : (-1)));
						enemyDefs.RemoveAt(0);
					}
				}
				else
				{
					CreateEliteEnemyUnit(eliteEnemyDefs[0], tile, unitCreationSettings).UnitView.LookAtDirection(TPSingleton<EnemyUnitManager>.Instance.debugEnemySpawnDirection);
					if (currentVariantId == -1 || ++currentVariantId >= eliteEnemyDefs[0].VisualVariants.Count)
					{
						currentVariantId = ((eliteEnemyDefs.Count > 1) ? GetInitialOverrideVariantId(eliteEnemyDefs[1]) : (-1));
						eliteEnemyDefs.RemoveAt(0);
					}
				}
			}
			return false;
		});
	}
}
