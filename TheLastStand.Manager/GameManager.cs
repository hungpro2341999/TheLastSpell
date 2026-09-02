using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TPLib.Yield.CustomYieldInstructions;
using TheLastStand.Controller;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Fog;
using TheLastStand.Framework;
using TheLastStand.Framework.Encryption;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Modding;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.Serialization.Building;
using TheLastStand.Serialization.Item;
using TheLastStand.Serialization.Meta;
using TheLastStand.Serialization.SpawnWave;
using TheLastStand.Serialization.Trophy;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.Building.Construction;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Cursor;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.NightReport;
using TheLastStand.View.Shop;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Manager;

public sealed class GameManager : Manager<GameManager>, ISerializable, IDeserializable
{
	public delegate void LateDeserialize();

	public static class Constants
	{
		public class Debug
		{
			public static string LevelsPath = "Assets/DataFiles/Levels/WIP/Runtime/";
		}

		public const string AmbientSoundsFormat = "Sounds/SFX/Ambient/AMB_Towns/AMB_{0}";
	}

	private readonly IEnumerator waitForGameInit = new WaitUntil(() => TPSingleton<GameManager>.Instance.GameInitialized);

	[SerializeField]
	[Tooltip("Disable auto load (editor only!)")]
	private bool disableAutoLoad;

	[SerializeField]
	[Tooltip("Disable auto save (editor only!)")]
	private bool disableAutoSave;

	[SerializeField]
	[Tooltip("Disables save safety check - ONLY WORKS with encryption")]
	private bool disableSaveCheck;

	[SerializeField]
	[Range(1f, 10f)]
	[Tooltip("Time before the enemies start their turn")]
	private float newNightTransitionDuration = 3f;

	[SerializeField]
	[Range(1f, 10f)]
	[Tooltip("Time before the night report panel show up")]
	private float newDayTransitionDuration = 3f;

	[SerializeField]
	[Range(0f, 10f)]
	[Tooltip("Time waited after bone piles got generated (only applies if there were bone piles to generate)")]
	private float delayAfterBonePiles = 1f;

	[SerializeField]
	private AudioClip winAudioClip;

	[SerializeField]
	private AudioClip defeatAudioClip;

	[SerializeField]
	private AudioClip tutorialDefeatAudioClip;

	[SerializeField]
	private Transform viewTransform;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioSource ambienceAudioSource;

	[SerializeField]
	private AudioClip productionPhaseAudioClip;

	[SerializeField]
	private AudioClip deploymentPhaseAudioClip;

	[SerializeField]
	private AudioClip nightPlayerPhaseAudioClip;

	[SerializeField]
	private AudioClip nightEnemyPhaseAudioClip;

	[SerializeField]
	private float ambientSoundsFadeInDuration = 2f;

	[SerializeField]
	private float ambientSoundsFadeOutDuration = 2f;

	[SerializeField]
	private Game.E_DayTurn debugStartingDayTurn;

	[SerializeField]
	private bool loadLevelEditorCityAssets;

	[SerializeField]
	private Game.E_State currentStateName;

	private bool currentGameIsLoaded;

	private float previousTimeSpent;

	private float timeAtGameStart;

	private Tween ambientSoundFadeTween;

	public static bool DisplayWillBeReachedBy = false;

	public static float AmbientSoundsFadeOutDuration => TPSingleton<GameManager>.Instance.ambientSoundsFadeOutDuration;

	public static AudioSource AudioSource => TPSingleton<GameManager>.Instance.audioSource;

	public static Game.E_State CurrentStateName
	{
		get
		{
			return TPSingleton<GameManager>.Instance.currentStateName;
		}
		set
		{
			TPSingleton<GameManager>.Instance.currentStateName = value;
		}
	}

	public static AudioClip WinAudioClip => TPSingleton<GameManager>.Instance.winAudioClip;

	public static AudioClip DefeatAudioClip => TPSingleton<GameManager>.Instance.defeatAudioClip;

	public static AudioClip TutorialDefeatAudioClip => TPSingleton<GameManager>.Instance.tutorialDefeatAudioClip;

	public static AudioClip DeploymentPhaseAudioClip => TPSingleton<GameManager>.Instance.deploymentPhaseAudioClip;

	public static bool DisableAutoLoad => TPSingleton<GameManager>.Instance.disableAutoLoad;

	public static FormulaInterpreterContext FormulaInterpreterContext { get; private set; }

	public static bool LoadLevelEditorCityAssets
	{
		get
		{
			if (TPSingleton<GameManager>.Instance.loadLevelEditorCityAssets)
			{
				TPSingleton<GameManager>.Instance.LogError("Trying to load city text assets using Level Editor folder while NOT being in editor. Using normal folder instead.", CLogLevel.MAJOR);
			}
			return false;
		}
	}

	public static AudioClip NightEnemyPhaseAudioClip => TPSingleton<GameManager>.Instance.nightEnemyPhaseAudioClip;

	public static AudioClip NightPlayerPhaseAudioClip => TPSingleton<GameManager>.Instance.nightPlayerPhaseAudioClip;

	public static AudioClip ProductionPhaseAudioClip => TPSingleton<GameManager>.Instance.productionPhaseAudioClip;

	public static Game.E_DayTurn StartingDayTurn
	{
		get
		{
			if (DebugStartingDayTurn != Game.E_DayTurn.Undefined)
			{
				return DebugStartingDayTurn;
			}
			return TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.StartingDayTurn;
		}
	}

	public static Game.E_DayTurn DebugStartingDayTurn => TPSingleton<GameManager>.Instance.debugStartingDayTurn;

	public static Transform ViewTransform => TPSingleton<GameManager>.Instance.viewTransform;

	public static IEnumerator WaitForGameInit => TPSingleton<GameManager>.Instance.waitForGameInit;

	public bool IsDebugStartingDayTurnOn => debugStartingDayTurn != Game.E_DayTurn.Undefined;

	public int FogDensityIndex => TPSingleton<FogManager>.Instance.Fog.DensityIndex;

	public int FogDensityValue => TPSingleton<FogManager>.Instance.Fog.DensityValue;

	[DevConsoleCommand(Options = (DevConsoleCommandOptions.CanReadWrite | DevConsoleCommandOptions.ForceStatic))]
	public int DayNumber
	{
		get
		{
			return Game.DayNumber;
		}
		set
		{
			int num = value - Game.DayNumber;
			Game.DayNumber = Mathf.Max(0, value);
			Analytics.GenerateMapDayData();
			SpawnWaveManager.GenerateSpawnWave();
			SpawnWaveManager.SpawnWaveView.Refresh();
			TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
			GameView.TopScreenPanel.TurnPanel.Refresh();
			TPSingleton<SoundManager>.Instance.ChangePlaylist();
			TPSingleton<SoundManager>.Instance.ChangeMusic();
			if (num > 0)
			{
				ApplicationManager.Application.DaysPlayed += (uint)num;
			}
			TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		}
	}

	public Game Game { get; private set; }

	public GameAnalytics GameAnalytics { get; private set; }

	public float TotalTimeSpent => previousTimeSpent + Time.unscaledTime - timeAtGameStart;

	public bool GameInitialized
	{
		get
		{
			if (Game != null)
			{
				return Game.State != Game.E_State.Off;
			}
			return false;
		}
	}

	public bool NightReportToDayCoroutineRunning { get; private set; }

	[DevConsoleCommand]
	public static float MoveSpeedMultiplier { get; set; } = 1f;

	[DevConsoleCommand]
	public static float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{
			Time.timeScale = value;
		}
	}

	[DevConsoleCommand("FpsEnabled", Options = (DevConsoleCommandOptions.CanReadWrite | DevConsoleCommandOptions.ForceStatic))]
	public static bool FpsEnabled
	{
		get
		{
			return DebugManager.FpsEnabled;
		}
		private set
		{
			DebugManager.FpsEnabled = value;
		}
	}

	public event LateDeserialize FinalizeDeserialize;

	public static void EraseSave(int profileIndex)
	{
		SaverLoader.Erase(SaveManager.GetGameSaveFilePath(profileIndex));
		SaverLoader.Erase(SaveManager.GetGameSaveBackupFilePath(profileIndex));
	}

	public static void ExileAllEnemies(bool countAsKills, bool resetSpawnWave, bool disableDieAnim = false, List<TheLastStand.Model.Unit.Unit> unitsToSkip = null)
	{
		if (resetSpawnWave)
		{
			SpawnWaveManager.RefreshEnemyWeightModifierXpRatio();
			SpawnWaveManager.CurrentSpawnWave = null;
		}
		TPSingleton<BossManager>.Instance.ExileAllUnits(countAsKills, disableDieAnim, unitsToSkip);
		TPSingleton<EnemyUnitManager>.Instance.ExileAllUnits(countAsKills, disableDieAnim, unitsToSkip);
	}

	public static bool HandleEndTurnInput()
	{
		if (GameController.CanEndPlayerTurn())
		{
			if (TurnEndValidationManager.EndTurnIsBlocked)
			{
				ACameraView.AllowUserPan = false;
				GenericBlockingPopup.OpenAsComplex("GenericBlocking_NotYetQuiteReadyYet", "GenericBlocking_BeforeProceeding", delegate
				{
					ACameraView.AllowUserPan = true;
				});
			}
			else if (TurnEndValidationManager.AnyBlockingPlayableUnitInFog)
			{
				ACameraView.AllowUserPan = false;
				GenericBlockingPopup.OpenAsSimple("GenericPopup_TitleMoveUnitOutOfFog", "GenericPopup_MoveUnitOutOfFog", delegate
				{
					ACameraView.AllowUserPan = true;
				}, smallVersion: true);
			}
			else
			{
				if (TurnEndValidationManager.CanEndTurnWithoutConsentAsking(out var localizedConsentAsk))
				{
					GameController.EndTurn();
					return true;
				}
				ACameraView.AllowUserPan = false;
				GenericConsent.OpenLocalized(localizedConsentAsk, delegate
				{
					GameController.SetState(Game.E_State.Management);
					GameController.EndTurn();
				}, delegate
				{
					GameController.SetState(Game.E_State.Management);
				});
			}
		}
		return false;
	}

	public static void Load()
	{
		if (ApplicationManager.Application.State.GetName() == "LevelEditor")
		{
			TPSingleton<GameManager>.Instance.Game = new GameController().Game;
			TPSingleton<GameManager>.Instance.GameAnalytics = new GameAnalytics();
			TPSingleton<GlyphManager>.Instance.InitGlyphEffects();
			TPSingleton<ConstructionManager>.Instance.Init();
			TPSingleton<BuildingManager>.Instance.Deserialize(null);
			return;
		}
		try
		{
			try
			{
				SerializedContainer serializedContainer = TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave?.LoadedContainer;
				TPSingleton<GameManager>.Instance.Deserialize(serializedContainer, ((int?)serializedContainer?.SaveVersion) ?? (-1));
			}
			catch (Exception e)
			{
				string currentGameSaveBackupFilePath = SaveManager.GetCurrentGameSaveBackupFilePath();
				SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> currentPreloadedGameSave = TPSingleton<SaveManager>.Instance.CurrentPreloadedGameSave;
				if ((currentPreloadedGameSave == null || !currentPreloadedGameSave.FailedLoadsInfo[0].Reason.HasValue) && File.Exists(currentGameSaveBackupFilePath))
				{
					SerializedContainer serializedContainer2 = TPSingleton<GameManager>.Instance.TryLoadBackup(e);
					TPSingleton<GameManager>.Instance.Deserialize(serializedContainer2, ((int?)serializedContainer2?.SaveVersion) ?? (-1));
					return;
				}
				throw;
			}
		}
		catch (Exception ex)
		{
			string fileCopyPath = SaveManager.CorruptGameSave(SaveManager.CurrentProfileIndex);
			ApplicationManager.Application.ApplicationController.SetState("GameLobby");
			SaveManager.LoadFailedInfos.Add(new LoadFailedInfos(fileCopyPath));
			TPSingleton<GameManager>.Instance.LogError("Caught exception while loading game. Error Message : " + ex.Message, CLogLevel.MAJOR, forcePrintInUnity: true, printStackTrace: false);
			TPSingleton<GameManager>.Instance.LogError($"A critical error occured while loading the savegame\n{ex}\nPlease catch this specific exception earlier on and add a proper message for it.", CLogLevel.DETAILED);
		}
	}

	public static void Save()
	{
		if (!TPSingleton<GameManager>.Instance.IsSaveAllowed())
		{
			TPSingleton<GameManager>.Instance.Log("Tried to save game but it was not allowed.", CLogLevel.DETAILED);
			return;
		}
		SaverLoader.EnqueueSave(E_SaveType.Game);
		SaveManager.SaveApp();
	}

	public static void TryToSaveAuto()
	{
		if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.Undefined)
		{
			Save();
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		this.FinalizeDeserialize = null;
		SerializedGameState serializedGameState = container as SerializedGameState;
		currentGameIsLoaded = serializedGameState != null;
		BackwardCompatibilityBeforeDeserialize(serializedGameState, saveVersion);
		TPSingleton<ApocalypseManager>.Instance.Deserialize(serializedGameState?.Apocalypse, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<MetaConditionManager>.Instance.DeserializeFromGameSave(serializedGameState?.MetaConditionsRunContext);
		TPSingleton<GlyphManager>.Instance.InitGlyphEffects();
		TPSingleton<GlyphManager>.Instance.Deserialize(serializedGameState?.SerializedGlyphsContainer, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		Game = new GameController(serializedGameState?.Game).Game;
		GameAnalytics = new GameAnalytics(serializedGameState?.SerializedGameAnalytics);
		FormulaInterpreterContext = new FormulaInterpreterContext(Game);
		Analytics.GenerateMapDayData();
		Analytics.GenerateMapNightData();
		ApocalypseManager.CheckEffectsActivationOnTurnCondition();
		previousTimeSpent = serializedGameState?.TotalTimeSpent ?? 0f;
		timeAtGameStart = Time.unscaledTime;
		TPSingleton<InputManager>.Instance.Init();
		TPSingleton<RandomManager>.Instance.Deserialize(serializedGameState?.Random);
		TPSingleton<PathfindingManager>.Instance.Init();
		TPSingleton<TileObjectSelectionManager>.Instance.Init();
		TPSingleton<FogManager>.Instance.Deserialize(serializedGameState?.Fog);
		CharacterSheetPanel.Init();
		TPSingleton<UnitLevelUpView>.Instance.Init();
		TPSingleton<PanicManager>.Instance.Init();
		ShopView.Init();
		TPSingleton<ConstructionManager>.Instance.Init();
		TPSingleton<ShopManager>.Instance.Init();
		TPSingleton<SinkManager>.Instance.Init();
		TPSingleton<SinkManager>.Instance.Deserialize(serializedGameState?.Sinks);
		if (currentGameIsLoaded)
		{
			TPSingleton<BuildingManager>.Instance.Deserialize(serializedGameState?.Buildings, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		}
		else
		{
			TPSingleton<BuildingManager>.Instance.Deserialize(null);
		}
		TPSingleton<InventoryManager>.Instance.Deserialize(serializedGameState?.Inventory);
		TPSingleton<ItemManager>.Instance.Init();
		TPSingleton<ResourceManager>.Instance.Deserialize(serializedGameState?.Resources);
		TPSingleton<SpawnWaveManager>.Instance.Deserialize(serializedGameState?.SpawnWaveContainer);
		TPSingleton<SpawnWaveManager>.Instance.Init();
		if (serializedGameState?.SpawnWaveContainer?.CurrentSpawnWave == null)
		{
			SpawnWaveManager.GenerateSpawnWave();
		}
		else
		{
			SpawnWaveManager.DeserializeSpawnWave(serializedGameState.SpawnWaveContainer, serializedGameState.SaveVersion);
		}
		CameraView.CameraLutView.Deserialize(serializedGameState?.SerializedLut);
		TPSingleton<EnemyUnitManager>.Instance.Init();
		TPSingleton<PlayableUnitManagementView>.Instance.Init();
		TPSingleton<PlayableUnitManager>.Instance.Deserialize(serializedGameState?.PlayableUnits, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<PlayableUnitManager>.Instance.NightReport.Deserialize(serializedGameState?.SerializedNightReport, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<EnemyUnitManager>.Instance.Deserialize(serializedGameState?.EnemyUnits, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<BossManager>.Instance.Deserialize(serializedGameState?.BossData, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		if (serializedGameState != null)
		{
			TPSingleton<EnemyUnitManager>.Instance.DisplayEnemiesIconAndTileFeedback();
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count == 0)
		{
			PlayableUnitManager.CreateStartUnits();
		}
		TPSingleton<ConstructionView>.Instance.Init();
		TPSingleton<PanicManager>.Instance.Deserialize(serializedGameState?.Panic, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		TPSingleton<TrophyManager>.Instance.Deserialize(serializedGameState?.Trophies, ((int?)serializedGameState?.SaveVersion) ?? (-1));
		this.FinalizeDeserialize?.Invoke();
		TPSingleton<GameManager>.Instance.Log((serializedGameState != null) ? "Game loaded!" : "New game!", CLogLevel.MAJOR);
		BackwardCompatibilityAfterDeserialize(serializedGameState, saveVersion);
		if (serializedGameState != null && ApplicationManager.LastLoadedVersion == 11)
		{
			TPSingleton<AchievementManager>.Instance.TriggerGameBackwardCompatibility();
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedGameState
		{
			Resources = (SerializedResources)TPSingleton<ResourceManager>.Instance.Serialize(),
			Game = (SerializedGame)Game.Serialize(),
			Random = (SerializedRandoms)TPSingleton<RandomManager>.Instance.Serialize(),
			PlayableUnits = (SerializedPlayableUnits)TPSingleton<PlayableUnitManager>.Instance.Serialize(),
			EnemyUnits = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedEnemyUnits)TPSingleton<EnemyUnitManager>.Instance.Serialize()) : null),
			BossData = (SerializedBossData)TPSingleton<BossManager>.Instance.Serialize(),
			Buildings = (SerializedBuildings)TPSingleton<BuildingManager>.Instance.Serialize(),
			Inventory = (SerializedItems)TPSingleton<InventoryManager>.Instance.Serialize(),
			Apocalypse = (SerializedApocalypse)TPSingleton<ApocalypseManager>.Instance.Serialize(),
			Fog = (SerializedFog)TPSingleton<FogManager>.Instance.Serialize(),
			Panic = (SerializedPanic)TPSingleton<PanicManager>.Instance.Serialize(),
			MetaConditionsRunContext = (SerializedMetaConditionsContext)TPSingleton<MetaConditionManager>.Instance.SerializeToGameSave(),
			SpawnWaveContainer = (SerializedSpawnWaveContainer)TPSingleton<SpawnWaveManager>.Instance.Serialize(),
			TotalTimeSpent = TotalTimeSpent,
			DLCsInUse = new List<string>(TPSingleton<DLCManager>.Instance.OwnedDLCIds),
			ModsInUse = new List<string>(ModManager.ModIdsInUse),
			SerializedLut = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedLUT)CameraView.CameraLutView.Serialize()) : null),
			Trophies = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedTrophies)TPSingleton<TrophyManager>.Instance.Serialize()) : null),
			SerializedNightReport = ((Game.Cycle == Game.E_Cycle.Night) ? ((SerializedNightReport)TPSingleton<PlayableUnitManager>.Instance.NightReport.Serialize()) : null),
			SerializedGlyphsContainer = TPSingleton<GlyphManager>.Instance.SerializeGlyphs(),
			SerializedGameAnalytics = TPSingleton<GameManager>.Instance.GameAnalytics.Serialize(),
			Sinks = TPSingleton<SinkManager>.Instance.Serialize()
		};
	}

	public void FinalizeDayTransition()
	{
		if (DayNumber < TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount)
		{
			StartCoroutine(NightReportToProductionPhaseCoroutine());
			return;
		}
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendEndNightEvent(0);
		}
		GameAnalytics.ClearSkillAnalytics();
		GameController.SetState(Game.E_State.CutscenePlaying);
		CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.VictorySequenceView, VictorySequenceCallback);
	}

	public bool IsSaveAllowed()
	{
		return TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
	}

	public void OnGameExitButtonClick()
	{
		UnityEngine.Application.Quit();
	}

	public IEnumerator WaitNewCycleTransition()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			if (DayNumber >= TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount)
			{
				FinalizeDayTransition();
			}
			else if (StartingDayTurn != Game.E_DayTurn.Production || TPSingleton<GameManager>.Instance.Game.DayNumber != 0)
			{
				yield return SharedYields.WaitForSeconds(TPSingleton<GameManager>.Instance.newDayTransitionDuration);
				TPSingleton<NightReportPanel>.Instance.Open();
			}
			yield break;
		}
		yield return SharedYields.WaitForSeconds(TPSingleton<GameManager>.Instance.newNightTransitionDuration);
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendEndDayEvent();
		}
		GameAnalytics.ClearBuildingBuiltAnalytics();
		PlayableUnitManager.StartTurn();
		BossManager.StartTurn();
		EnemyUnitManager.StartTurn();
		TPSingleton<BuildingManager>.Instance.StartTurn();
		NightTurnsManager.StartTurn();
	}

	protected override void Awake()
	{
		base.Awake();
		SaveEncoder.enableHashCheck = !disableSaveCheck;
		DOTween.Init();
		DOTween.SetTweensCapacity(500, 50);
		TileMapView.DisplayLevel();
		CursorController cursorController = new CursorController();
		Load();
		Game.Cursor = cursorController.Cursor;
		if (!TPSingleton<LevelEditorManager>.Exist())
		{
			ApplicationManager.Application.ApplicationController.SetState("Game");
		}
	}

	private static void TriggerMagicSealsCompleted()
	{
		GameController.TriggerGameOver(Game.E_GameOverCause.MagicSealsCompleted);
	}

	private IEnumerator NightReportToProductionPhaseCoroutine()
	{
		NightReportToDayCoroutineRunning = true;
		SpawnWaveManager.SpawnWaveView.RefreshPosition();
		CameraView.RefreshDayTimeEffects();
		yield return SharedYields.WaitForSeconds(TPSingleton<FogManager>.Instance.FogView.WaitBeforeFogIncreaseSequence);
		bool isDayException = false;
		foreach (FogDefinition.FogDayException fogDayException in TPSingleton<FogManager>.Instance.Fog.FogDefinition.DayExceptions)
		{
			if (fogDayException.DayNumber != TPSingleton<GameManager>.Instance.Game.DayNumber)
			{
				continue;
			}
			isDayException = true;
			if (FogController.IsDensityEqualTo(fogDayException.FogDensityName))
			{
				SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
				continue;
			}
			yield return TPSingleton<FogManager>.Instance.MoveCameraToNearestFogWithWave(delegate
			{
				FogController.SetDensity(fogDayException.FogDensityName);
				SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
			}, TPSingleton<FogManager>.Instance.FogView.WaitBeforeFogIncreaseShow);
			yield return SharedYields.WaitForSeconds(TPSingleton<FogManager>.Instance.FogView.WaitAfterFogIncreaseShow);
		}
		if (!isDayException)
		{
			if (TPSingleton<GameManager>.Instance.Game.DayNumber % TPSingleton<FogManager>.Instance.Fog.DailyUpdateFrequency == 0 && !FogController.IsDensityAtMaximum())
			{
				yield return TPSingleton<FogManager>.Instance.MoveCameraToNearestFogWithWave(delegate
				{
					FogController.IncreaseDensity();
					SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
				}, TPSingleton<FogManager>.Instance.FogView.WaitBeforeFogIncreaseShow);
				yield return SharedYields.WaitForSeconds(TPSingleton<FogManager>.Instance.FogView.WaitAfterFogIncreaseShow);
			}
			else
			{
				SpawnWaveManager.CurrentSpawnWave.SpawnWaveView.Refresh(onDayStart: false, forceDisplayArrows: true);
			}
		}
		yield return ACameraView.Zoom(zoomIn: false);
		ACameraView.MoveTo(BuildingManager.MagicCircle.BuildingView.transform);
		yield return SharedYields.WaitForSeconds(1f);
		int generatedBonePilesCount = TPSingleton<BuildingManager>.Instance.GenerateBonePiles();
		if (generatedBonePilesCount > 0)
		{
			TPSingleton<BuildingManager>.Instance.PlayBonePileConstructionSound();
			yield return SharedYields.WaitForSeconds(delayAfterBonePiles);
		}
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendEndNightEvent(generatedBonePilesCount);
		}
		GameAnalytics.ClearSkillAnalytics();
		TPSingleton<FogManager>.Instance.Fog.HasBeenRepelled = false;
		yield return TPSingleton<BuildingManager>.Instance.GenerateRandomBuildingsCoroutine();
		TPSingleton<FogManager>.Instance.GenerateLightFogSpawners();
		yield return TPSingleton<BuildingManager>.Instance.TriggerBuildingPassiveCoroutine();
		TPSingleton<BuildingManager>.Instance.ResetShopRerollIndex();
		PlayableUnitManager.RespawnUnits();
		TPSingleton<ToDoListView>.Instance.Show();
		GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
		GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: true);
		GameView.TopScreenPanel.TurnPanel.Display(show: true);
		NightReportToDayCoroutineRunning = false;
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnProductionStart);
	}

	private void BackwardCompatibilityAfterDeserialize(SerializedGameState saveGame, int saveVersion)
	{
		if (saveGame != null)
		{
			TPSingleton<BuildingManager>.Instance.BackwardCompatibilityAfterDeserialize(saveGame, saveVersion);
		}
	}

	private void BackwardCompatibilityBeforeDeserialize(SerializedGameState saveGame, int saveVersion)
	{
	}

	private IEnumerator StartGame()
	{
		yield return new WaitForFrames();
		if (TPSingleton<LevelEditorManager>.Exist())
		{
			GameController.SetState(Game.E_State.LevelEdition);
			yield break;
		}
		ACameraView.MoveTo(TileMapView.GetTileCenter(BuildingManager.MagicCircle.OriginTile));
		StartAmbientSounds();
		TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot();
		if (currentGameIsLoaded)
		{
			GameController.StartTurnOnLoad(instant: true);
			TPSingleton<AchievementManager>.Instance.HandleRunLoad();
		}
		else
		{
			GameController.StartTurn(instant: true);
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnGameStart);
			TPSingleton<AchievementManager>.Instance.HandleRunStart();
		}
	}

	private void Start()
	{
		if (ApplicationManager.Application.State.GetName() == "NewGame")
		{
			TryToSaveAuto();
		}
		StartCoroutine(StartGame());
	}

	private SerializedContainer TryLoadBackup(Exception e)
	{
		try
		{
			string currentGameSaveFilePath = SaveManager.GetCurrentGameSaveFilePath();
			string currentGameSaveBackupFilePath = SaveManager.GetCurrentGameSaveBackupFilePath();
			SaveManager.LoadFailedInfos.Add(new LoadFailedInfos(SaverLoader.MarkFileAsCorrupted(currentGameSaveFilePath)));
			TPSingleton<GameManager>.Instance.LogWarning("First Game loading failed! Trying to load BACKUP file.", CLogLevel.MAJOR);
			TPSingleton<GameManager>.Instance.LogWarning($"Failed loading exception message : {e}", CLogLevel.MAJOR);
			SerializedGameState serializedGameState = SaverLoader.Load<SerializedGameState>(currentGameSaveBackupFilePath, !SaveManager.IsSaveEncryptionDisabled);
			TPSingleton<GameManager>.Instance.Log($"Game file save version : {serializedGameState.SaveVersion}", CLogLevel.MAJOR, forcePrintInUnity: true);
			if (serializedGameState.SaveVersion < SaveManager.MinimumSupportedGameSaveVersion)
			{
				throw new SaverLoader.WrongSaveVersionException(currentGameSaveBackupFilePath, shouldMarkAsCorrupted: true);
			}
			SaverLoader.CopyFileTo(currentGameSaveBackupFilePath, currentGameSaveFilePath);
			SaveManager.LoadFailedInfos[^1].BackupHasBeenLoaded = true;
			return serializedGameState;
		}
		catch (Exception)
		{
			throw e;
		}
	}

	private void Update()
	{
		if (!GameInitialized)
		{
			return;
		}
		if (InputManager.IsPointerOverWorld || InputManager.IsPointerOverAllowingCursorUI)
		{
			Game.Cursor.CursorController.SetTile();
		}
		else
		{
			Game.Cursor.PreviousTile = Game.Cursor.Tile;
			Game.Cursor.PreviousTilePosition = Game.Cursor.TilePosition;
			if (Game.Cursor.Tile != null)
			{
				CursorView.ClearTiles(Game.Cursor.Tile);
				PlayableUnitManager.OnCursorTileBecomeNull();
				TileObjectSelectionManager.UpdateCursorOrientationFromSelection();
				Game.Cursor.Tile = null;
			}
		}
		if (InputManager.GetButtonDown(5))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive);
		}
		else if (InputManager.GetButtonDown(62))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production);
		}
		if (!(ApplicationManager.Application.State is GameState))
		{
			return;
		}
		if (InputManager.GetButtonDown(7) && GameController.CanEndPlayerTurn() && !InputManager.GetButtonDown(55))
		{
			HandleEndTurnInput();
		}
		else
		{
			if (SaveManager.LoadFailedInfos.Count <= 0 || !UIManager.DisplayGameSaveErrorPopUp(SaveManager.LoadFailedInfos))
			{
				return;
			}
			foreach (LoadFailedInfos loadFailedInfo in SaveManager.LoadFailedInfos)
			{
				if (!loadFailedInfo.BackupHasBeenLoaded)
				{
					TPSingleton<ApplicationManager>.Instance.LogError("===============================[ LOGBAR ]===============================", CLogLevel.NORMAL, forcePrintInUnity: true, printStackTrace: false);
					TPSingleton<ApplicationManager>.Instance.LogError("Hello again - from now on, you can stop ignoring NullRefs.", CLogLevel.NORMAL, forcePrintInUnity: true, printStackTrace: false);
					break;
				}
			}
			SaveManager.LoadFailedInfos.Clear();
		}
	}

	public static void VictorySequenceCallback()
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (selectedCity.CityDefinition.PostVictoryCutscene != null)
		{
			AnimatedCutsceneManager.PlayPostVictoryAnimatedCutscene(selectedCity.CityDefinition.PostVictoryCutscene, TriggerMagicSealsCompleted);
		}
		else
		{
			TriggerMagicSealsCompleted();
		}
	}

	public void StopAmbientSounds()
	{
		SoundManager.FadeOutAudioSource(ambienceAudioSource, ref ambientSoundFadeTween, ambientSoundsFadeOutDuration);
	}

	private void StartAmbientSounds()
	{
		string text = $"Sounds/SFX/Ambient/AMB_Towns/AMB_{TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id}";
		AudioClip audioClip = ResourcePooler.LoadOnce<AudioClip>(text);
		if (audioClip != null)
		{
			SoundManager.PlayFadeInAudioClip(ambienceAudioSource, ref ambientSoundFadeTween, audioClip, ambientSoundsFadeInDuration);
		}
		else
		{
			LogWarning("No ambient sounds found at path " + text + ".");
		}
	}

	[DevConsoleCommand("ReloadScene")]
	public static void DebugReloadGameScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		GC.Collect();
	}

	[DevConsoleCommand("SaveGame", Options = DevConsoleCommandOptions.ForceStatic)]
	private static void DebugSave()
	{
		if (!TPSingleton<GameManager>.Instance.IsSaveAllowed())
		{
			TPSingleton<GameManager>.Instance.Log("Tried to save game but it was not allowed.", CLogLevel.DETAILED, forcePrintInUnity: true);
		}
		else
		{
			SaverLoader.EnqueueSave(E_SaveType.Game);
		}
	}

	[DevConsoleCommand("VictorySequence", Options = DevConsoleCommandOptions.ForceStatic)]
	private static void DebugTriggerVictorySequence([StringConverter(typeof(StringToCityIdConverter))] string sequenceCityId = "")
	{
		GameController.SetState(Game.E_State.CutscenePlaying);
		GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: false);
		GameView.TopScreenPanel.TurnPanel.Display(show: false);
		TPSingleton<ToDoListView>.Instance.Hide();
		PlayableUnitManager.GatherUnitsForVictorySequence();
		TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride = sequenceCityId;
		CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.VictorySequenceView, VictorySequenceCallback);
	}

	[DevConsoleCommand("TutorialSequence", Options = DevConsoleCommandOptions.ForceStatic)]
	private static void DebugTriggerTutorialSequence()
	{
		CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.TutorialSequenceView);
	}

	[DevConsoleCommand("DefeatSequence", Options = DevConsoleCommandOptions.ForceStatic)]
	private static void DebugTriggerDefeatSequence()
	{
		BuildingManager.MagicCircle.BuildingController.DamageableModuleController.Demolish();
	}
}
