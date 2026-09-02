using System;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Settings;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit;
using TheLastStand.DRM.Achievements;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Trap;
using TheLastStand.Manager.Turret;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization;
using TheLastStand.View;
using TheLastStand.View.Building.Construction;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller;

public class GameController
{
	public static bool LockEndTurn;

	public Game Game { get; private set; }

	public GameController()
	{
		Game = new Game();
	}

	public GameController(SerializedGame container)
	{
		Game = new Game(container);
	}

	public static void GoBackToMainMenu(bool killRunSave = false)
	{
		TPSingleton<SoundManager>.Instance.TransitionToDefaultSnapshot(GameManager.AmbientSoundsFadeOutDuration);
		TPSingleton<GameManager>.Instance.StopAmbientSounds();
		if (!killRunSave)
		{
			GameManager.Save();
		}
		else
		{
			GameManager.EraseSave(SaveManager.CurrentProfileIndex);
		}
		ApplicationManager.Application.ApplicationController.SetState("LoadMainMenuFromGameState");
	}

	public static void GoToMetaShops()
	{
		ApplicationManager.Application.ApplicationQuitInOraculum = true;
		SaveManager.SaveApp();
		ApplicationManager.Application.ApplicationController.SetState("MetaShops");
	}

	public static bool CanExitConstructionMode()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Production)
		{
			return ConstructionManager.DebugForceConstructionAllowed;
		}
		return TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction;
	}

	public static bool CanEndPlayerTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits)
		{
			return false;
		}
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Construction:
			switch (TPSingleton<ConstructionManager>.Instance.Construction.State)
			{
			case Construction.E_State.Repair:
			case Construction.E_State.Destroy:
			case Construction.E_State.ChooseBuilding:
				return true;
			default:
				return false;
			}
		case Game.E_State.Management:
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				return !EnemyUnitManager.IsThereAnyEnemyDying();
			}
			return true;
		default:
			return false;
		}
	}

	public static bool CanOpenConstructionMode(BuildingDefinition.E_ConstructionCategory buildingCategory)
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Production)
		{
			return ConstructionManager.DebugForceConstructionAllowed;
		}
		Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
		if (state == Game.E_State.Management || (state == Game.E_State.Construction && TPSingleton<ConstructionView>.Instance.CurrentlyDisplayedCategory != buildingCategory))
		{
			return true;
		}
		return false;
	}

	public static bool EndNightIfNeeded()
	{
		if (TPSingleton<GameManager>.Instance.Game.IsNightEnd)
		{
			GameManager.ExileAllEnemies(countAsKills: false, resetSpawnWave: true);
			TrophyManager.SetValueToTrophiesConditions<NightCompletedTrophyConditionController>(new object[2]
			{
				TPSingleton<GameManager>.Instance.Game.DayNumber,
				TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id
			});
			TrophyManager.SetValueToTrophiesConditions<NightCompletedXTurnsAfterSpawnEndConditionController>(new object[1] { TPSingleton<GameManager>.Instance.Game.CurrentNightHour - TPSingleton<TrophyManager>.Instance.SpawnWaveDuration });
			TrophyManager.SetValueToTrophiesConditions<PerfectPanicTrophyConditionController>(new object[1] { PanicManager.Panic.Value });
			for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
			{
				PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i];
				TrophyManager.SetValueToTrophiesConditions<HealthRemainingAtMostTrophyConditionController>(new object[2] { playableUnit.RandomId, playableUnit.Health });
			}
			DebugLogNightData();
			TPSingleton<TrophyManager>.Instance.OnNightEnd(TPSingleton<GameManager>.Instance.Game.IsDefeat);
			TPSingleton<AchievementManager>.Instance.HandleNightEnd();
			TPSingleton<BuildingManager>.Instance.DestroyLightFogSpawners();
			TPSingleton<BuildingManager>.Instance.ExtinguishBraziers();
			if (TPSingleton<GameManager>.Instance.DayNumber != TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount)
			{
				TPSingleton<BuildingManager>.Instance.ComputeRandomBuildingsPositions();
			}
			ApplicationManager.Application.DaysPlayed++;
			TPSingleton<GameManager>.Instance.Game.Cycle = Game.E_Cycle.Day;
			TPSingleton<GameManager>.Instance.Game.DayTurn = Game.E_DayTurn.Production;
			TPSingleton<GameManager>.Instance.Game.NightTurn = Game.E_NightTurn.Undefined;
			TPSingleton<GameManager>.Instance.Game.LastNightLastHour = TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
			TPSingleton<GameManager>.Instance.Game.CurrentNightHour = 0;
			UIManager.CloseAllOpenedPopups();
			TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot(SoundManager.DayTransitionDuration);
			if (PanicManager.Panic.IsAtMaxValue)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_MAXIMUM_PANIC_NIGHT);
			}
			return true;
		}
		return false;
	}

	public static void EndTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
		{
			TPSingleton<GameManager>.Instance.Log("Game is Over ! EndTurn is locked.", CLogLevel.DETAILED);
		}
		else
		{
			if (TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying)
			{
				return;
			}
			PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction)
			{
				ConstructionManager.ExitConstructionMode();
			}
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying)
			{
				ACameraView.AllowUserPan = true;
			}
			TileObjectSelectionManager.DeselectAll();
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnEndTurn);
			PlayableUnitManager.EndTurn();
			EnemyUnitManager.EndTurn();
			BossManager.EndTurn();
			switch (TPSingleton<GameManager>.Instance.Game.Cycle)
			{
			case Game.E_Cycle.Day:
				BuildingManager.EndTurn();
				switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
				{
				case Game.E_DayTurn.Production:
					TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnEndProductionTurn);
					GameManager.TryToSaveAuto();
					TPSingleton<GameManager>.Instance.Game.DayTurn = Game.E_DayTurn.Deployment;
					TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot(SoundManager.NightTransitionDuration);
					break;
				case Game.E_DayTurn.Deployment:
					foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
					{
						playableUnit.PlayableUnitPerksController.ResetLockedPerksModulesData();
					}
					TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnEndDeploymentTurn);
					GameManager.TryToSaveAuto();
					TPSingleton<TrophyManager>.Instance.RenewTrophies();
					TPSingleton<GameManager>.Instance.Game.Cycle = Game.E_Cycle.Night;
					TPSingleton<GameManager>.Instance.Game.DayNumber++;
					Analytics.GenerateMapDayData();
					CameraView.CameraLutView.UpdateLutTextures();
					TPSingleton<GameManager>.Instance.Game.DayTurn = Game.E_DayTurn.Undefined;
					TPSingleton<GameManager>.Instance.Game.NightTurn = Game.E_NightTurn.EnemyUnits;
					UIManager.HideInfoPanels();
					foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
					{
						playableUnit2.PlayableUnitController.FillArmor();
					}
					if (TPSingleton<GameManager>.Instance.Game.DayNumber == 2 && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap)
					{
						CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.TutorialSequenceView, StartTurnCallback2);
						return;
					}
					break;
				}
				break;
			case Game.E_Cycle.Night:
				BuildingManager.EndTurn();
				switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
				{
				case Game.E_NightTurn.PlayableUnits:
					TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnEndNightTurnPlayable);
					UIManager.HideInfoPanels();
					TPSingleton<GameManager>.Instance.Game.NightTurn = Game.E_NightTurn.EnemyUnits;
					if (TPSingleton<GameManager>.Instance.Game.DayNumber == 2 && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap)
					{
						CutsceneManager.PlayCutscene(TPSingleton<CutsceneManager>.Instance.TutorialSequenceView, StartTurnCallback);
						return;
					}
					CameraView.CameraLutView.RefreshLut();
					break;
				case Game.E_NightTurn.EnemyUnits:
					ApocalypseManager.CheckEnemyUnitTurnEnded();
					TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnEndNightTurnEnemy);
					if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying)
					{
						TPSingleton<GameManager>.Instance.Game.NightTurn = Game.E_NightTurn.PlayableUnits;
					}
					break;
				}
				EndNightIfNeeded();
				break;
			}
			StartTurn();
		}
		static void StartTurnCallback()
		{
			StartTurn();
		}
		static void StartTurnCallback2()
		{
			StartTurn();
		}
	}

	public static void RestartLevel()
	{
		ApplicationManager.Application.ApplicationController.SetState("ReloadGame");
	}

	public static void SetState(Game.E_State newState)
	{
		Game.E_State state = TPSingleton<GameManager>.Instance.Game.State;
		if (state == Game.E_State.GameOver)
		{
			return;
		}
		TPSingleton<GameManager>.Instance.Game.PreviousState = state;
		TPSingleton<GameManager>.Instance.Game.State = newState;
		GameManager.CurrentStateName = newState;
		InputManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.LevelEdition)
		{
			if (TPSingleton<SoundManager>.Instance.DebugChangeMusicAfterReportPopup && (state == Game.E_State.NightReport || state == Game.E_State.ProductionReport) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
			{
				TPSingleton<SoundManager>.Instance.ChangeMusic();
			}
			BuildingManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State, state);
			ConstructionManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
			TileObjectSelectionManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State, state);
			EnemyUnitManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
			PlayableUnitManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State, state);
			MetaUpgradesManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State, state);
			SpawnWaveManager.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State, state);
			CursorController.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
			CameraView.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
			GameView.BottomScreenPanel.BottomLeftPanel.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State);
			GameView.BottomScreenPanel.Refresh();
			GameView.GameAccelerationPanel.Refresh();
			TPSingleton<CharacterSheetPanel>.Instance.OnGameStateChange(TPSingleton<GameManager>.Instance.Game.State, state);
		}
	}

	public static void StartTurn(bool instant = false)
	{
		TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnStartTurn);
		TPSingleton<AchievementManager>.Instance.HandleTurnStart();
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: true);
			GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
			PlayableUnitManager.StartTurn();
			EnemyUnitManager.StartTurn();
			BossManager.StartTurn();
			TurretManager.StartTurn();
			TrapManager.StartTurn();
			BonePileManager.StartTurn();
			switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
			{
			case Game.E_DayTurn.Production:
				TPSingleton<GameManager>.Instance.Log("[Day] Start Production turn", CLogLevel.MAJOR);
				TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnStartProductionTurn);
				TPSingleton<MetaConditionManager>.Instance.RefreshNightsReached(TPSingleton<GameManager>.Instance.Game.DayNumber);
				if (GameManager.StartingDayTurn == Game.E_DayTurn.Production && TPSingleton<GameManager>.Instance.Game.DayNumber == 0)
				{
					SetState(Game.E_State.Management);
					TPSingleton<SoundManager>.Instance.ChangeMusic();
					TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
				}
				else
				{
					TPSingleton<MetaConditionManager>.Instance.RefreshLowestPanicLevelReached(PanicManager.Panic.Level);
					SetState(Game.E_State.NightReport);
					TPSingleton<SoundManager>.Instance.ChangePlaylist();
					if (!TPSingleton<SoundManager>.Instance.DebugChangeMusicAfterReportPopup)
					{
						TPSingleton<SoundManager>.Instance.ChangeMusic();
					}
				}
				TPSingleton<FogManager>.Instance.FogView.DisplayFog(instant);
				TPSingleton<BuildingManager>.Instance.RefreshBuildingsProductionPanels();
				TPSingleton<ResourceManager>.Instance.RefillWorkers();
				TPSingleton<BarkManager>.Instance.CheckNewCycle(TPSingleton<GameManager>.Instance.Game.Cycle);
				RecruitmentController.ClearCurrentRoster();
				PanicManager.Panic.PanicController.ResetExpectedValue();
				PanicManager.Panic.PanicReward.PanicRewardController.GetReward();
				PanicManager.Panic.PanicReward.PanicRewardController.ReloadBaseNbRerollReward();
				PanicManager.Panic.PanicReward.PanicRewardController.ReloadRemainingNbRerollReward();
				PanicManager.Panic.PanicView.DisplayOrHide();
				TPSingleton<GameManager>.Instance.StartCoroutine(TPSingleton<GameManager>.Instance.WaitNewCycleTransition());
				break;
			case Game.E_DayTurn.Deployment:
				TPSingleton<GameManager>.Instance.Log("[Day] Start Deployment turn", CLogLevel.MAJOR);
				TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnStartDeploymentTurn);
				TPSingleton<SoundManager>.Instance.ChangeMusic();
				TPSingleton<BuildingManager>.Instance.StartTurn();
				if (GameManager.StartingDayTurn == TPSingleton<GameManager>.Instance.Game.DayTurn && TPSingleton<GameManager>.Instance.Game.DayNumber == 0)
				{
					SetState(Game.E_State.Management);
					TPSingleton<ResourceManager>.Instance.RefillWorkers();
				}
				TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
				SoundManager.PlayAudioClip(GameManager.AudioSource, GameManager.DeploymentPhaseAudioClip);
				break;
			}
			ApocalypseManager.DeactivateModifiersWithTurnConditions();
			CameraView.RefreshDayTimeEffects(instant);
			break;
		case Game.E_Cycle.Night:
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
				TPSingleton<GameManager>.Instance.Log("[Night] Start Enemy turn", CLogLevel.MAJOR);
				GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: false);
				TPSingleton<GameManager>.Instance.Game.CurrentNightHour++;
				Analytics.GenerateMapNightData();
				if (TPSingleton<GameManager>.Instance.Game.CurrentNightHour == 1)
				{
					SpawnWaveManager.LastPlayedSpawnWaveId = SpawnWaveManager.CurrentSpawnWave?.SpawnWaveDefinition.Id ?? string.Empty;
					CameraView.RefreshDayTimeEffects(instant);
					FogController.RefreshFog(instant);
					TPSingleton<BarkManager>.Instance.CheckNewCycle(TPSingleton<GameManager>.Instance.Game.Cycle);
					TileMapController.CleanDeadBodies();
					if (!SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.IsBossWave)
					{
						TPSingleton<SoundManager>.Instance.ChangeMusic();
					}
					else
					{
						TPSingleton<SoundManager>.Instance.StopMusic();
					}
					TPSingleton<ToDoListView>.Instance.Hide();
					int num = (TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.ContainsKey(TPSingleton<GameManager>.Instance.DayNumber) ? TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber].Count : 0);
					for (int i = 0; i < num; i++)
					{
						PlayableUnitView.RemoveUsedPortraitColor(TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber][i].PortraitColor);
						PlayableUnitView.RemoveUsedPortrait(TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber][i].PortraitSprite);
					}
					TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightHpLost = 0f;
					PanicManager.Panic.PanicController.Reset();
					TPSingleton<AchievementManager>.Instance.HandleNightStart();
					TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxNightReached = Mathf.Max(TPSingleton<GameManager>.Instance.Game.DayNumber, TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxNightReached);
					TPSingleton<GameManager>.Instance.StartCoroutine(TPSingleton<GameManager>.Instance.WaitNewCycleTransition());
				}
				else
				{
					PlayableUnitManager.StartTurn();
					BossManager.StartTurn();
					EnemyUnitManager.StartTurn();
					TPSingleton<BuildingManager>.Instance.StartTurn();
					NightTurnsManager.StartTurn();
				}
				TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnStartNightTurnEnemy);
				SoundManager.PlayAudioClip(GameManager.AudioSource, GameManager.NightEnemyPhaseAudioClip);
				break;
			case Game.E_NightTurn.PlayableUnits:
				TPSingleton<GameManager>.Instance.Log("[Night] Start Player turn", CLogLevel.MAJOR);
				GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: true);
				PlayableUnitManager.StartTurn();
				EnemyUnitManager.StartTurn();
				TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnStartNightTurnPlayable);
				EffectManager.DisplayEffects();
				TPSingleton<TrophyManager>.Instance.EnemiesKilledThisTurn = 0;
				TPSingleton<BuildingManager>.Instance.StartTurn();
				SoundManager.PlayAudioClip(GameManager.AudioSource, GameManager.NightPlayerPhaseAudioClip);
				SetState(Game.E_State.Management);
				break;
			}
			break;
		}
		PanicManager.StartTurn();
		TPSingleton<FogManager>.Instance.StartTurn();
		PlayableUnitManager.OnTurnStart();
		BuildingManager.OnTurnStart();
		InventoryManager.StartTurn();
		TPSingleton<SpawnWaveManager>.Instance.OnTurnStart();
		ResourceManager.OnTurnStart();
		TPSingleton<GlyphManager>.Instance.StartTurn();
		MetaNarrationsManager.OnTurnStart();
		PlayableUnitManagementView.OnTurnStart();
		GameView.TopScreenPanel.TurnPanel.Refresh();
		TPSingleton<LightningSDKManager>.Instance.HandleGameCycleColor();
		TPSingleton<SinkManager>.Instance.StartTurn();
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			GameManager.TryToSaveAuto();
		}
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment)
			{
				TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnDeploymentStart);
			}
			break;
		case Game.E_Cycle.Night:
			if (TPSingleton<GameManager>.Instance.Game.CurrentNightHour == 1)
			{
				TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnNightStart);
			}
			if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
			{
				TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnPlayableUnitTurn);
			}
			break;
		}
	}

	public static void StartTurnOnLoad(bool instant = false)
	{
		CameraView.RefreshDayTimeEffects(instant, onLoad: true);
		TPSingleton<SoundManager>.Instance.ChangeMusic();
		SetState(Game.E_State.Management);
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
			{
			case Game.E_DayTurn.Production:
				TPSingleton<GameManager>.Instance.Log("[Day] Loading at Production turn", CLogLevel.MAJOR);
				TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
				TPSingleton<FogManager>.Instance.FogView.DisplayFog(instant);
				TPSingleton<BuildingManager>.Instance.RefreshBuildingsProductionPanels();
				TPSingleton<ToDoListView>.Instance.RefreshSpawnWavePositionView();
				PanicManager.Panic.PanicView.DisplayOrHide();
				break;
			case Game.E_DayTurn.Deployment:
				TPSingleton<GameManager>.Instance.Log("[Day] Loading at Deployment turn", CLogLevel.MAJOR);
				TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
				TPSingleton<ToDoListView>.Instance.RefreshSpawnWavePositionView();
				SoundManager.PlayAudioClip(GameManager.AudioSource, GameManager.DeploymentPhaseAudioClip);
				break;
			}
			break;
		case Game.E_Cycle.Night:
			TPSingleton<GameManager>.Instance.Log("[Night] Loading at Night Player turn", CLogLevel.MAJOR);
			GameView.TopScreenPanel.UnitPortraitsPanel.Display(show: true);
			TPSingleton<TrophyManager>.Instance.EnemiesKilledThisTurn = 0;
			TPSingleton<ToDoListView>.Instance.Hide();
			PanicManager.Panic.PanicView.DisplayOrHide();
			break;
		}
		PlayableUnitManager.OnTurnStart();
		BuildingManager.OnTurnStart();
		TPSingleton<SpawnWaveManager>.Instance.OnTurnStart();
		PlayableUnitManagementView.OnTurnStart();
		GameView.TopScreenPanel.TurnPanel.Refresh();
		FogController.RefreshFogArea();
		TPSingleton<LightningSDKManager>.Instance.HandleGameCycleColor();
	}

	public static void TriggerGameOver(Game.E_GameOverCause cause)
	{
		if ((TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap && cause == Game.E_GameOverCause.Abandon) || TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying)
		{
			ApplicationManager.Application.TutorialDone = true;
			ApplicationManager.Application.TutorialSkipped = cause == Game.E_GameOverCause.Abandon;
			TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		}
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendEndRunEvent(cause);
		}
		ApplicationManager.HandleGameOver(cause);
		TPSingleton<WorldMapCityManager>.Instance.HandleGameOver(cause);
		TPSingleton<GlyphManager>.Instance.HandleGameOver(cause);
		SpawnWaveManager.LastPlayedSpawnWaveId = string.Empty;
		bool flag = cause == Game.E_GameOverCause.MagicSealsCompleted;
		TPSingleton<MetaConditionManager>.Instance.IncreaseRunsCompleted(!flag);
		TPSingleton<AchievementManager>.Instance.HandleGameOver(cause);
		SettingsController.ToggleGameSpeed(isOn: false);
		TPSingleton<GameManager>.Instance.Game.GameOverCause = cause;
		GameManager.EraseSave(SaveManager.CurrentProfileIndex);
		if (cause == Game.E_GameOverCause.Abandon || TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying)
		{
			ApplicationManager.Application.ApplicationQuitInOraculum = true;
			SaveManager.SaveApp();
			ApplicationManager.Application.ApplicationController.SetState("MetaShops");
			return;
		}
		SaveManager.SaveApp();
		SetState(Game.E_State.GameOver);
		UIManager.CloseAllOpenedPopups();
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap)
		{
			CanvasFadeManager.FadeIn(0.6f, TPSingleton<RetryTutorialPanel>.Instance.Canvas.sortingOrder - 1, Ease.Unset, null, TPSingleton<RetryTutorialPanel>.Instance.Open);
		}
		else
		{
			TPSingleton<GameOverPanel>.Instance.Open();
		}
	}

	public static void DebugLogNightData()
	{
		string text = $"--- END OF NIGHT {TPSingleton<GameManager>.Instance.Game.DayNumber} ---";
		text += "\n--- General ---\n";
		text += $"\n- {TPSingleton<GameManager>.Instance.Game.CurrentNightHour} turns";
		text += "\n--- Run Modifiers ---\n";
		text += $"\n- Apocalypse level: {ApocalypseManager.CurrentApocalypseLevel}";
		text += "\n- Kills: ";
		foreach (KillReportData item3 in TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight)
		{
			text += $"{item3.KillAmount} {item3.SpecificId}, ";
		}
		TimeSpan timeSpan = TimeSpan.FromSeconds(TPSingleton<GameManager>.Instance.TotalTimeSpent);
		text += $"\n- Current time spent in run : {Math.Floor(timeSpan.TotalHours)} hour(s) {timeSpan.Minutes} min {timeSpan.Seconds} sec";
		text += "\r\n ";
		text += "\n--- Heroes ---\n";
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			text += $"\n- {playableUnit.Name} ({playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPointsTotal)} AP)";
			text += "\n Weapons: ";
			for (int i = 0; i < 2; i++)
			{
				TheLastStand.Model.Item.Item item = null;
				if (playableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value) && value.Count > i)
				{
					item = value[i].Item;
				}
				TheLastStand.Model.Item.Item item2 = null;
				if (playableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out var value2) && value2.Count > i)
				{
					item2 = value2[i].Item;
				}
				if (i == 1 && (item != null || item2 != null))
				{
					text += " | ";
				}
				text = text + item2?.Name + ((item != null && item2 != null) ? " + " : string.Empty) + item?.Name;
			}
			text += $"\n Health: {playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Health)}/{playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.HealthTotal)}";
			text += $"\n Mana: {playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana)}/{playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal)}";
		}
		text += "\r\n ";
		text += "\n--- Trophies ---\n";
		text += TPSingleton<TrophyManager>.Instance.GetProgressionLog(!TPSingleton<GameManager>.Instance.Game.IsVictory);
		TPSingleton<GameManager>.Instance.Log(text, CLogLevel.MAJOR);
	}
}
