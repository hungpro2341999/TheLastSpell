using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Framework.Serialization;
using TheLastStand.Framework.Utils;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.UI;
using UnityEngine;

namespace TheLastStand.Manager.Unit;

public class BossManager : BehaviorManager<BossManager>, ISerializable, IDeserializable
{
	[SerializeField]
	private float bossKilledCameraMovementDuration = 1f;

	[SerializeField]
	private Ease bossKilledCameraMovementEasing = Ease.OutQuad;

	[SerializeField]
	private EnemyUnitHUD bossUnitHUDSmall;

	[SerializeField]
	private EnemyUnitHUD bossUnitHUDLarge;

	private List<IBehaviorModel> sortedBossUnits = new List<IBehaviorModel>();

	private string currentBossPhaseId;

	public BossUnitTemplateDefinition DebugBossUnitTemplateDefinition;

	private bool willSpawnBossNextFrame;

	public static EnemyUnitHUD BossUnitHUDSmall => TPSingleton<BossManager>.Instance.bossUnitHUDSmall;

	public static EnemyUnitHUD BossUnitHUDLarge => TPSingleton<BossManager>.Instance.bossUnitHUDLarge;

	private Coroutine PlayFocusOnEachSpawnedUnitsCoroutine { get; set; }

	public override List<IBehaviorModel> BehaviorModels => BossUnits.Cast<IBehaviorModel>().ToList();

	public Dictionary<string, BossPhase> BossPhases { get; } = new Dictionary<string, BossPhase>();

	public List<MutableTuple<int, ABossPhaseActionController>> BossPhasesActionsToExecute { get; } = new List<MutableTuple<int, ABossPhaseActionController>>();

	public Dictionary<string, List<IBossPhaseActor>> BossPhaseActors { get; } = new Dictionary<string, List<IBossPhaseActor>>();

	public Dictionary<string, int> BossPhaseActorsKills { get; } = new Dictionary<string, int>();

	public List<BossUnit> BossUnits { get; } = new List<BossUnit>();

	public string CurrentBossPhaseId
	{
		get
		{
			return currentBossPhaseId;
		}
		private set
		{
			if (!(currentBossPhaseId == value))
			{
				currentBossPhaseId = value;
				CurrentBossPhase?.Init();
			}
		}
	}

	public string NextBossPhaseId { get; set; } = string.Empty;

	public int NextBossPhaseDelay { get; set; } = -1;

	public BossPhase CurrentBossPhase => BossPhases.GetValueOrDefault(CurrentBossPhaseId);

	public string CurrentBossUnitTemplateId { get; private set; }

	public GenericCutsceneView DeathCutscene { get; private set; }

	public bool IsBossVanquished { get; set; }

	public bool IsPlayingDeathCutscene
	{
		get
		{
			if (DeathCutscene != null)
			{
				return DeathCutscene.IsPlaying;
			}
			return false;
		}
	}

	public bool VictoryConditionIsToFinishWave
	{
		get
		{
			SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
			if (currentSpawnWave != null && currentSpawnWave.SpawnWaveDefinition.IsBossWave)
			{
				return CurrentBossPhase?.VictoryConditionIsToFinishWave ?? false;
			}
			return false;
		}
	}

	public bool ShouldTriggerBossWaveVictory
	{
		get
		{
			SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
			if (currentSpawnWave != null && currentSpawnWave.SpawnWaveDefinition.IsBossWave)
			{
				return CurrentBossPhase?.ShouldTriggerVictory ?? false;
			}
			return false;
		}
	}

	public bool ShouldTriggerBossWaveDefeat
	{
		get
		{
			SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
			if (currentSpawnWave != null && currentSpawnWave.SpawnWaveDefinition.IsBossWave)
			{
				return CurrentBossPhase?.ShouldTriggerDefeat ?? false;
			}
			return false;
		}
	}

	public float BossKilledCameraMovementDuration => bossKilledCameraMovementDuration;

	public Ease BossKilledCameraMovementEasing => bossKilledCameraMovementEasing;

	public TaskGroup MoveUnitsTaskGroup { get; private set; }

	public UnitsToSpawnBySector RecentlySpawnedUnitsBySector { get; private set; }

	public static IEnumerator CreateBossUnit(BossUnitTemplateDefinition bossUnitTemplateDefinition, Tile tile, UnitCreationSettings unitCreationSettings)
	{
		List<Tile> occupiedTiles = tile.GetOccupiedTiles(bossUnitTemplateDefinition);
		if (bossUnitTemplateDefinition.MoveMethod != UnitTemplateDefinition.E_MoveMethod.AboveAll)
		{
			TileMapManager.ClearBuildingOnTiles(occupiedTiles);
		}
		TileMapManager.ClearEnemiesOnTiles(occupiedTiles);
		TileMapManager.FreeTilesFromPlayableUnits(occupiedTiles);
		TPSingleton<BossManager>.Instance.IsBossVanquished = false;
		UnitView unitView = UnityEngine.Object.Instantiate(EnemyUnitManager.EnemyUnitViewPrefab, EnemyUnitManager.UnitsTransform);
		BossUnitController unitController = new BossUnitController(bossUnitTemplateDefinition, unitView, tile, unitCreationSettings);
		yield return InitCreatedBossUnit(tile, unitController, unitView, unitCreationSettings);
		if (unitController.BossUnit.CanBeDamaged() && Analytics.AllowedToSendData)
		{
			Analytics.SendVulnerabilityBossEvent(unitController.BossUnit.BossPhaseActorId);
		}
	}

	public static IEnumerator InitCreatedBossUnit(Tile tile, BossUnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings)
	{
		tile.TileController.SetUnit(unitController.BossUnit);
		unitView.Unit = unitController.Unit;
		unitController.EnemyUnit.IsExecutingSkillOnSpawn = unitCreationSettings.CastSpawnSkill && unitController.EnemyUnit.HasSpawnGoals();
		TPSingleton<BossManager>.Instance.BossUnits.Add(unitController.BossUnit);
		if (unitCreationSettings.BossPhaseActorId != null)
		{
			unitController.EnemyUnit.BossPhaseActorId = unitCreationSettings.BossPhaseActorId;
		}
		if (unitController.EnemyUnit.IsExecutingSkillOnSpawn)
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemiesExecutingSkillsOnSpawn.Add(unitController.EnemyUnit);
		}
		if (unitCreationSettings.PlaySpawnCutscene && unitController.BossUnit.BossUnitTemplateDefinition.SpawnCutsceneId != null)
		{
			CutsceneData cutsceneData = new CutsceneData(null, unitController.BossUnit.OriginTile, unitController.BossUnit);
			GenericCutsceneView genericCutsceneView = TPSingleton<CutsceneManager>.Instance.GetGenericCutsceneView();
			genericCutsceneView.Init(unitController.BossUnit.BossUnitTemplateDefinition.SpawnCutsceneId, cutsceneData);
			CutsceneManager.PlayCutscene(genericCutsceneView);
			yield return genericCutsceneView.WaitUntilIsOver;
			unitController.TriggerAffixes(E_EffectTime.OnCreationAfterViewInitialized);
		}
		else
		{
			unitView.InitVisuals(unitCreationSettings.PlaySpawnAnim);
			unitView.UpdatePosition();
			unitView.LookAtDirection(unitController.Unit.LookDirection);
			unitView.RefreshHudPositionInstantly();
			unitController.TriggerAffixes(E_EffectTime.OnCreationAfterViewInitialized);
			if (unitCreationSettings.WaitSpawnAnim)
			{
				yield return unitView.WaitUntilAnimatorStateIsIdle;
			}
			if (unitCreationSettings.CastSpawnSkill)
			{
				if (unitController.BossUnit.BossUnitTemplateDefinition.CastSpawnSkillDelay > 0f)
				{
					yield return SharedYields.WaitForSeconds(unitController.BossUnit.BossUnitTemplateDefinition.CastSpawnSkillDelay);
				}
				unitController.ExecuteSpawnGoals();
			}
			unitView.UnitHUD.ToggleFollowElement(toggle: true);
			yield return SharedYields.WaitForEndOfFrame;
			unitView.UnitHUD.ToggleFollowElement(toggle: false);
		}
		TPSingleton<BossManager>.Instance.Log("Spawned unit " + unitView.Unit.Id);
	}

	public static void DestroyUnit(BossUnit bossUnit)
	{
		TPSingleton<BossManager>.Instance.BossUnits.Remove(bossUnit);
		if (bossUnit.IsBossPhaseActor)
		{
			TPSingleton<BossManager>.Instance.HandleBossPhaseActorDeath(bossUnit);
		}
		UnityEngine.Object.Destroy(bossUnit.UnitView.gameObject);
		GameView.TopScreenPanel.TurnPanel.PhasePanel.RefreshSoulsText();
		GameView.TopScreenPanel.TurnPanel.PhasePanel.RefreshNightSliderValues();
		if (!NightTurnsManager.HandleUnitsDeath(bossUnit) && TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(TileObjectSelectionManager.SelectedPlayableUnit);
		}
		TPSingleton<BossManager>.Instance.Log("Destroyed unit " + bossUnit.Id);
	}

	public static void EndTurn()
	{
		for (int i = 0; i < TPSingleton<BossManager>.Instance.BossUnits.Count; i++)
		{
			TPSingleton<BossManager>.Instance.BossUnits[i].UnitController.EndTurn();
		}
	}

	public static void Init(string bossUnitTemplateId, bool setCurrentBossPhase = true)
	{
		if (BossPhasesDatabase.BossPhasesDefinitions.TryGetValue(bossUnitTemplateId + "Phases", out var value))
		{
			TPSingleton<BossManager>.Instance.CurrentBossUnitTemplateId = bossUnitTemplateId;
			foreach (KeyValuePair<string, BossPhaseDefinition> bossPhaseDefinition in value.BossPhaseDefinitions)
			{
				TPSingleton<BossManager>.Instance.BossPhases[bossPhaseDefinition.Key] = new BossPhase(bossPhaseDefinition.Value);
			}
			if (setCurrentBossPhase)
			{
				TPSingleton<BossManager>.Instance.CurrentBossPhaseId = TPSingleton<BossManager>.Instance.BossPhases.First().Key;
			}
		}
		else
		{
			TPSingleton<BossManager>.Instance.LogError("BossPhasesDefinitions " + bossUnitTemplateId + " not found in BossPhasesDatabase");
		}
	}

	public static void ResetPhases()
	{
		TPSingleton<BossManager>.Instance.CurrentBossUnitTemplateId = null;
		TPSingleton<BossManager>.Instance.IsBossVanquished = false;
		TPSingleton<BossManager>.Instance.BossPhases.Clear();
		TPSingleton<BossManager>.Instance.CurrentBossPhaseId = string.Empty;
	}

	public static void StartTurn()
	{
		for (int i = 0; i < TPSingleton<BossManager>.Instance.BossUnits.Count; i++)
		{
			TPSingleton<BossManager>.Instance.BossUnits[i].UnitController.StartTurn();
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			RandomManager.ClearSavedState(TPSingleton<BossManager>.Instance);
		}
	}

	public void ExileAllUnits(bool countAsKills, bool disableDieAnim = false, List<TheLastStand.Model.Unit.Unit> unitsToSkip = null)
	{
		for (int num = BossUnits.Count - 1; num >= 0; num--)
		{
			if ((unitsToSkip == null || !unitsToSkip.Contains(BossUnits[num])) && !BossUnits[num].IsDead)
			{
				if (countAsKills)
				{
					TrophyManager.AddEnemyKill(Mathf.RoundToInt(BossUnits[num].UnitStatsController.UnitStats.Stats[UnitStatDefinition.E_Stat.DamnedSoulsEarned].FinalClamped));
				}
				bool forcePlayDieAnim = !disableDieAnim && BossUnits[num].EnemyUnitView != null && BossUnits[num].EnemyUnitView.IsVisible;
				BossUnits[num].BossUnitController.PrepareForExile(forcePlayDieAnim);
				BossUnits[num].BossUnitController.ExecuteExile();
			}
		}
	}

	public IEnumerator HandleBossPhases()
	{
		if (CurrentBossPhase == null)
		{
			yield break;
		}
		Log("Handling BossPhase " + CurrentBossPhaseId + "...", CLogLevel.NORMAL, forcePrintInUnity: true);
		if (NextBossPhaseDelay > 0)
		{
			NextBossPhaseDelay--;
		}
		for (int i = BossPhasesActionsToExecute.Count - 1; i >= 0; i--)
		{
			MutableTuple<int, ABossPhaseActionController> delayActionPair = BossPhasesActionsToExecute[i];
			if (--delayActionPair.Item1 <= 0)
			{
				yield return delayActionPair.Item2.Execute();
				BossPhasesActionsToExecute.Remove(delayActionPair);
			}
		}
		foreach (BossPhaseHandler value in CurrentBossPhase.BossPhaseHandlers.Values)
		{
			if (value.IsLocked || !value.Conditions.All((ABossPhaseConditionController x) => x.IsValid()))
			{
				continue;
			}
			Log("Valid conditions for PhaseHandler " + value.BossPhaseHandlerDefinition.Id + " in phase " + CurrentBossPhaseId + "!", CLogLevel.NORMAL, forcePrintInUnity: true);
			foreach (ABossPhaseActionController action in value.Actions)
			{
				int delay = action.Delay;
				if (delay > 0)
				{
					BossPhasesActionsToExecute.Insert(0, new MutableTuple<int, ABossPhaseActionController>(delay, action));
				}
				else
				{
					yield return action.Execute();
				}
			}
		}
		if (NextBossPhaseDelay == 0 && !string.IsNullOrEmpty(NextBossPhaseId))
		{
			CurrentBossPhaseId = NextBossPhaseId;
			NextBossPhaseDelay = -1;
			NextBossPhaseId = string.Empty;
		}
	}

	public IEnumerator HandleEndNightCleaning()
	{
		if (CurrentBossPhase == null || CurrentBossPhase.BossPhaseDefinition.EndNightBuildingsActorsToCleanIds.Count == 0)
		{
			yield break;
		}
		foreach (string endNightBuildingsActorsToCleanId in CurrentBossPhase.BossPhaseDefinition.EndNightBuildingsActorsToCleanIds)
		{
			CleanBuildingActors(endNightBuildingsActorsToCleanId);
		}
	}

	public void HandleBossPhaseActorDeath(IBossPhaseActor bossPhaseActor)
	{
		if (!BossPhaseActors.TryRemoveAtKey(bossPhaseActor.BossPhaseActorId, bossPhaseActor))
		{
			LogError(bossPhaseActor.BossPhaseActorId + " key was not present in BossPhaseActors, something is wrong");
		}
		BossPhase currentBossPhase = CurrentBossPhase;
		if (currentBossPhase != null && currentBossPhase.ShouldTriggerVictory)
		{
			IsBossVanquished = true;
			Log("BossPhase Victory!", CLogLevel.MAJOR);
			if (SpawnWaveManager.CurrentSpawnWave != null && TPSingleton<GameManager>.Instance.Game.CurrentNightHour >= SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.Duration)
			{
				SpawnWaveManager.CurrentSpawnWave?.RemainingEnemiesToSpawn.Clear();
				SpawnWaveManager.CurrentSpawnWave?.RemainingEliteEnemiesToSpawn.Clear();
			}
		}
	}

	public IEnumerator MoveUnitsCoroutine(List<IBehaviorModel> bossUnits, bool moveCamera = false)
	{
		sortedBossUnits.Clear();
		sortedBossUnits = RemoveSkippedBehaviours(bossUnits, updateSkippedTurns: false);
		sortedBossUnits = SortBehaviors(sortedBossUnits);
		List<BossUnit> bosses = sortedBossUnits.Cast<BossUnit>().ToList();
		SetUnitsComputingStepsTo(bosses, IBehaviorModel.E_GoalComputingStep.BeforeMoving);
		ComputeGoals(sortedBossUnits);
		if (moveCamera)
		{
			List<BossUnit> list = new List<BossUnit>();
			foreach (BossUnit item in bosses)
			{
				if (item.Path[item.Path.Count - 1] != item.OriginTile)
				{
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				Vector3 zero = Vector3.zero;
				foreach (BossUnit item2 in list)
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
		for (int num = sortedBossUnits.Count - 1; num >= 0; num--)
		{
			EnemyUnit enemyUnit = sortedBossUnits[num] as BossUnit;
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
			if (enemyUnit.TargetTile != null)
			{
				enemyUnit.Log($"I will not reach my current targeted tile {enemyUnit.TargetTile.Position}, so I'm setting it to null", CLogLevel.DETAILED);
				enemyUnit.OccupiedTiles.ForEach(delegate(Tile tile)
				{
					tile.WillBeReachedBy = null;
				});
				enemyUnit.TargetTile = null;
			}
		}
		TPSingleton<EnemyUnitManager>.Instance.PlayMoveSounds(hashSet);
		MoveUnitsTaskGroup.OnCompleteAction = delegate
		{
			GameController.SetState(Game.E_State.Management);
			MoveUnitsTaskGroup = null;
		};
		MoveUnitsTaskGroup.Run();
		yield return new WaitUntil(() => MoveUnitsTaskGroup == null);
		SetUnitsComputingStepsTo(bosses, IBehaviorModel.E_GoalComputingStep.AfterMoving);
	}

	public IEnumerator PlayDeathCutscene(BossUnit bossUnit)
	{
		CutsceneData cutsceneData = new CutsceneData(null, bossUnit.OriginTile, bossUnit);
		DeathCutscene = TPSingleton<CutsceneManager>.Instance.GetGenericCutsceneView();
		DeathCutscene.Init(bossUnit.BossUnitTemplateDefinition.DeathCutsceneId, cutsceneData);
		CutsceneManager.PlayCutscene(DeathCutscene);
		yield return DeathCutscene.WaitUntilIsOver;
		DeathCutscene = null;
	}

	public IEnumerator TryPlayDeathCutscene(BossUnit bossUnit, bool isFinalDeath)
	{
		GameController.LockEndTurn = true;
		if (isFinalDeath && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.FinalBossDeath)
		{
			TPSingleton<GameManager>.Instance.Game.NightTurn = Game.E_NightTurn.FinalBossDeath;
		}
		if (bossUnit.HasDeathCutscene)
		{
			yield return PlayDeathCutscene(bossUnit);
		}
		else
		{
			ACameraView.Zoom(zoomIn: true);
			ACameraView.MoveTo(bossUnit.UnitView.transform.position + Vector3.up * 2f, CameraView.AnimationMoveSpeed);
			yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
			bossUnit.DamageableView.PlayDieAnim();
			if (isFinalDeath)
			{
				TPSingleton<EnemyUnitManager>.Instance.ExileAllUnits(countAsKills: false, disableDieAnim: false, new List<TheLastStand.Model.Unit.Unit>(1) { bossUnit });
			}
			yield return new WaitUntil(() => bossUnit.UnitView.DieAnimationIsFinished);
		}
		GameController.LockEndTurn = false;
		if (isFinalDeath)
		{
			SpawnWaveManager.RefreshEnemyWeightModifierXpRatio();
			SpawnWaveManager.CurrentSpawnWave = null;
			GameController.EndTurn();
		}
	}

	public override IEnumerator PrepareSkillsForGroups()
	{
		if (turboMode)
		{
			yield break;
		}
		base.IsDone = false;
		Log("Starting skill preparation for Boss groups");
		for (int skillGroupIndex = 0; skillGroupIndex < skillGroups.Count; skillGroupIndex++)
		{
			RecentlySpawnedUnitsBySector.Clear();
			if (skillGroups[skillGroupIndex].SkillCasterAttackGroups.All((SkillCasterAttackGroup attackGroup) => attackGroup.GoalsToExecute.All((ComputedGoal computedGoal) => computedGoal.Goal.Skill.SkillAction is SpawnSkillAction)))
			{
				yield return PlayPreCastFXsAndExecute(skillGroups[skillGroupIndex]);
			}
			else
			{
				yield return MoveCameraAndExecute(skillGroups[skillGroupIndex]);
			}
			if (RecentlySpawnedUnitsBySector.Count > 0)
			{
				PlayFocusOnEachSpawnedUnitsCoroutine = TPSingleton<BossManager>.Instance.StartCoroutine(PlayFocusOnEachSpawnedUnits());
				yield return new WaitUntil(() => PlayFocusOnEachSpawnedUnitsCoroutine == null);
			}
		}
		base.IsDone = true;
	}

	public void TriggerBossesAffix(E_EffectTime effectTime)
	{
		for (int i = 0; i < BossUnits.Count; i++)
		{
			BossUnits[i].EnemyUnitController.TriggerAffixes(effectTime);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		RecentlySpawnedUnitsBySector = new UnitsToSpawnBySector();
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

	private void CleanBuildingActors(string actorId)
	{
		if (!BossPhaseActors.TryGetValue(actorId, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if (value[num] is TheLastStand.Model.Building.Building building)
			{
				BuildingManager.DestroyBuilding(building.OriginTile, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: false);
			}
		}
	}

	private IEnumerator PlayFocusOnEachSpawnedUnits()
	{
		ACameraView.AllowUserPan = false;
		for (int unitsBySectorIndex = 0; unitsBySectorIndex < RecentlySpawnedUnitsBySector.Count; unitsBySectorIndex++)
		{
			if (RecentlySpawnedUnitsBySector[unitsBySectorIndex] != null && RecentlySpawnedUnitsBySector[unitsBySectorIndex].Count != 0)
			{
				ACameraView.MoveTo(TPSingleton<SectorManager>.Instance.Sectors[unitsBySectorIndex].transform.position, BossUnitDatabase.BossStagingDefinitions[CurrentBossUnitTemplateId].MovementDuration, BossUnitDatabase.BossStagingDefinitions[CurrentBossUnitTemplateId].MovementEasing);
				yield return new WaitForSeconds(BossUnitDatabase.BossStagingDefinitions[CurrentBossUnitTemplateId].MovementDuration);
				yield return SpawnEveryEnemiesInSectors(RecentlySpawnedUnitsBySector[unitsBySectorIndex]);
				yield return new WaitForSeconds(BossUnitDatabase.BossStagingDefinitions[CurrentBossUnitTemplateId].PauseDuration);
			}
		}
		PlayFocusOnEachSpawnedUnitsCoroutine = null;
	}

	private IEnumerator PlayPreCastFXsAndExecute(SkillCasterCluster skillCasterCluster)
	{
		if (skillCasterCluster.TryGetFirstComputedGoalWithPreCastFXs(out var computedGoal))
		{
			ACameraView.MoveTo(((MonoBehaviour)computedGoal.Goal.Holder.TileObjectView).transform.position, CameraView.AnimationMoveSpeed);
			yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
			float num = computedGoal.Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PlayPreCastFxs();
			if (num > 0f)
			{
				yield return SharedYields.WaitForSeconds(num);
			}
		}
		yield return ExecuteSkillsForGroups(skillCasterCluster);
		TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnBehaviorClusterExecutionEnd);
	}

	private void SetUnitsComputingStepsTo(List<BossUnit> bossUnits, IBehaviorModel.E_GoalComputingStep computingStep)
	{
		foreach (BossUnit bossUnit in bossUnits)
		{
			bossUnit.GoalComputingStep = computingStep;
		}
	}

	private IEnumerator SpawnEveryEnemiesInSectors(Dictionary<TheLastStand.Model.Skill.Skill, Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>> dictionary)
	{
		foreach (KeyValuePair<TheLastStand.Model.Skill.Skill, Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>> skillAndUnitsByTilesToSpawn in dictionary)
		{
			TheLastStand.Model.Skill.Skill skill = skillAndUnitsByTilesToSpawn.Key;
			SpawnSkillAction spawnSkillAction = skill.SkillAction as SpawnSkillAction;
			foreach (KeyValuePair<(string, UnitCreationSettings), List<Tile>> kvp in skillAndUnitsByTilesToSpawn.Value.Item2)
			{
				ConcurrentQueue<Tile> spawnableTiles = new ConcurrentQueue<Tile>(kvp.Value);
				if (spawnSkillAction.SpawnSkillActionExecution.CastFx != null)
				{
					spawnSkillAction.SpawnSkillActionExecution.Caster = skillAndUnitsByTilesToSpawn.Value.Item1;
					spawnSkillAction.SpawnSkillActionExecution.CastFx.SourceTile = skillAndUnitsByTilesToSpawn.Value.Item1.OriginTile;
					spawnSkillAction.SpawnSkillActionExecution.CastFx.TargetTile = spawnableTiles.ElementAt(0);
					spawnSkillAction.SpawnSkillActionExecution.SkillExecutionController.SetCastFxAffectedTiles(spawnSkillAction.SpawnSkillActionExecution.CastFx, spawnableTiles.ElementAt(0), spawnableTiles.ToList(), TileObjectSelectionManager.E_Orientation.NONE);
					spawnSkillAction.SpawnSkillActionExecution.CastFx.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), skill.Owner);
				}
				yield return SharedYields.WaitForSeconds(spawnSkillAction.SpawnSkillActionDefinition.Delay);
				Tile result;
				while (spawnableTiles.TryDequeue(out result))
				{
					if (EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.TryGetValue(kvp.Key.Item1, out var value))
					{
						EnemyUnitManager.CreateEliteEnemyUnit(value, result, kvp.Key.Item2);
					}
					else
					{
						EnemyUnitManager.CreateEnemyUnit(EnemyUnitDatabase.EnemyUnitTemplateDefinitions[kvp.Key.Item1], result, kvp.Key.Item2);
					}
				}
			}
		}
	}

	private void Update()
	{
		Debug_Update();
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (!(container is SerializedBossData serializedBossData))
		{
			return;
		}
		foreach (SerializedEnemyUnit bossUnit2 in serializedBossData.BossUnits)
		{
			UnitCreationSettings unitCreationSettings = new UnitCreationSettings(bossUnit2.BossPhaseActorId, castSpawnSkill: false, playSpawnAnim: false, playSpawnCutscene: false, waitSpawnAnim: false, bossUnit2.OverrideVariantId, null, bossUnit2.IsGuardian, bossUnit2.IgnoreFromEnemyUnitsCount);
			UnitView unitView = UnityEngine.Object.Instantiate(EnemyUnitManager.EnemyUnitViewPrefab, EnemyUnitManager.UnitsTransform);
			BossUnit bossUnit = new BossUnitController(bossUnit2, unitView, unitCreationSettings, saveVersion).BossUnit;
			StartCoroutine(InitCreatedBossUnit(bossUnit.OriginTile, bossUnit.BossUnitController, unitView, unitCreationSettings));
			bossUnit.EnemyUnitController.UpdatePoisonFeedbackCondition();
			bossUnit.EnemyUnitController.UpdateInjuryStage();
		}
		currentBossPhaseId = serializedBossData.CurrentBossPhaseId;
		CurrentBossPhase?.Deserialize(serializedBossData.BossPhase, saveVersion);
		foreach (SerializedBossPhaseActorsKills bossPhaseActorsKill in serializedBossData.BossPhaseActorsKills)
		{
			BossPhaseActorsKills[bossPhaseActorsKill.ActorId] = bossPhaseActorsKill.Amount;
			if (!BossPhaseActors.ContainsKey(bossPhaseActorsKill.ActorId))
			{
				BossPhaseActors[bossPhaseActorsKill.ActorId] = new List<IBossPhaseActor>();
			}
		}
		for (int num = serializedBossData.PendingBossPhaseActions.Count - 1; num >= 0; num--)
		{
			SerializedDelayedBossPhaseAction serializedDelayedBossPhaseAction = serializedBossData.PendingBossPhaseActions[num];
			ABossPhaseActionController aBossPhaseActionController = BossPhases.GetValueOrDefault(serializedDelayedBossPhaseAction.PhaseId)?.BossPhaseHandlers.GetValueOrDefault(serializedDelayedBossPhaseAction.HandlerId).Actions.ElementAtOrDefault(serializedDelayedBossPhaseAction.ActionIndex);
			if (aBossPhaseActionController != null)
			{
				BossPhasesActionsToExecute.Insert(0, new MutableTuple<int, ABossPhaseActionController>(serializedDelayedBossPhaseAction.Delay, aBossPhaseActionController));
			}
		}
		GameView.TopScreenPanel.TurnPanel.PhasePanel.SetNightSliderValue(serializedBossData.NightProgressionValue);
	}

	public ISerializedData Serialize()
	{
		List<SerializedEnemyUnit> list = new List<SerializedEnemyUnit>();
		foreach (BossUnit bossUnit in BossUnits)
		{
			if (!bossUnit.IsDying && !bossUnit.IsDead)
			{
				list.Add((SerializedEnemyUnit)bossUnit.Serialize());
			}
		}
		List<SerializedBossPhaseActorsKills> list2 = new List<SerializedBossPhaseActorsKills>();
		foreach (KeyValuePair<string, int> bossPhaseActorsKill in BossPhaseActorsKills)
		{
			list2.Add(new SerializedBossPhaseActorsKills
			{
				ActorId = bossPhaseActorsKill.Key,
				Amount = bossPhaseActorsKill.Value
			});
		}
		List<SerializedDelayedBossPhaseAction> list3 = new List<SerializedDelayedBossPhaseAction>();
		foreach (MutableTuple<int, ABossPhaseActionController> item in BossPhasesActionsToExecute)
		{
			list3.Add(new SerializedDelayedBossPhaseAction
			{
				Delay = item.Item1,
				ActionIndex = item.Item2.ActionIndex,
				PhaseId = item.Item2.BossPhaseParentHandler.BossPhaseParentId,
				HandlerId = item.Item2.BossPhaseParentHandler.BossPhaseHandlerDefinition.Id
			});
		}
		return new SerializedBossData
		{
			BossUnits = list,
			CurrentBossPhaseId = currentBossPhaseId,
			BossPhase = (SerializedBossPhase)(CurrentBossPhase?.Serialize()),
			BossPhaseActorsKills = list2,
			PendingBossPhaseActions = list3,
			NightProgressionValue = GameView.TopScreenPanel.TurnPanel.PhasePanel.CurrentNightProgressionValue
		};
	}

	[DevConsoleCommand(Name = "EnemyBossSpawnEnable")]
	public static void Debug_EnableBossSpawn([StringConverter(typeof(BossUnit.StringToBossUnitTemplateIdConverter))] string bossTemplateId = "")
	{
		TPSingleton<EnemyUnitManager>.Instance.DebugEnemyUnitTemplateDefinition = null;
		TPSingleton<EnemyUnitManager>.Instance.DebugEliteEnemyUnitToSpawnTemplateDefinition = null;
		if (!BossUnitDatabase.BossUnitTemplateDefinitions.TryGetValue(bossTemplateId, out TPSingleton<BossManager>.Instance.DebugBossUnitTemplateDefinition))
		{
			TPSingleton<BossManager>.Instance.LogError("Boss " + bossTemplateId + " not found in database!");
		}
	}

	[DevConsoleCommand(Name = "ShowAreaOfSkill")]
	public static void ShowAreaOfSkill(int minRange = 0, int maxRange = 1)
	{
	}

	private void Debug_Update()
	{
		Debug_UpdateSpawnBoss();
	}

	private void Debug_UpdateSpawnBoss()
	{
		if (DebugBossUnitTemplateDefinition != null && InputManager.GetButton(24) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
		{
			if (!willSpawnBossNextFrame)
			{
				willSpawnBossNextFrame = true;
				return;
			}
			Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
			if (DebugBossUnitTemplateDefinition.CanSpawnOn(tile))
			{
				UnitCreationSettings unitCreationSettings = new UnitCreationSettings(null, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: true);
				StartCoroutine(CreateBossUnit(DebugBossUnitTemplateDefinition, tile, unitCreationSettings));
			}
			willSpawnBossNextFrame = false;
		}
		else
		{
			willSpawnBossNextFrame = false;
		}
	}
}
