using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Trap;
using TheLastStand.Manager.Turret;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Manager;

public class NightTurnsManager : Manager<NightTurnsManager>
{
	[Range(0f, 2f)]
	public float WaitAfterSpawnDuration = 0.83f;

	[Range(0f, 2f)]
	public float WaitAfterMoveDuration = 0.25f;

	[Range(0f, 2f)]
	public float WaitAfterTrapsDuration = 0.25f;

	[Range(0f, 2f)]
	public float WaitAfterTurretsDuration = 0.25f;

	[Range(0f, 2f)]
	public float WaitAfterBonePilesDuration = 0.25f;

	[Range(0f, 2f)]
	public float WaitAfterEnemiesDuration = 0.83f;

	[Range(0f, 2f)]
	public float WaitAfterTurnEndDuration;

	[SerializeField]
	private bool disableAI;

	public bool DisableAI => disableAI;

	public bool IsEndingNight { get; private set; }

	public static bool HandleUnitsDeath(TheLastStand.Model.Unit.Unit unit)
	{
		if (SpawnWaveManager.CurrentSpawnWave == null)
		{
			return true;
		}
		bool flag = SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.IsBossWave && TPSingleton<BossManager>.Instance.IsBossVanquished;
		if (!flag)
		{
			BossWaveSettings bossWaveSettings = SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.BossWaveSettings;
			if (bossWaveSettings != null && !bossWaveSettings.IsInfiniteWave)
			{
				flag = TPSingleton<BossManager>.Instance.VictoryConditionIsToFinishWave || TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory;
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && IsSpawnWaveFinished(flag))
		{
			TPSingleton<BossManager>.Instance.IsBossVanquished = true;
			GameManager.ExileAllEnemies(countAsKills: false, resetSpawnWave: true, disableDieAnim: false, new List<TheLastStand.Model.Unit.Unit>(1) { unit });
			TPSingleton<NightTurnsManager>.Instance.StopAllCoroutines();
			TPSingleton<NightTurnsManager>.Instance.StartCoroutine(TPSingleton<NightTurnsManager>.Instance.EndNightCoroutine());
			return true;
		}
		return false;
	}

	public static bool IsSpawnWaveFinished(bool bossWaveVanquished)
	{
		int num;
		if (SpawnWaveManager.CurrentSpawnWave != null)
		{
			BossWaveSettings bossWaveSettings = SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.BossWaveSettings;
			if ((bossWaveSettings == null || !bossWaveSettings.IsInfiniteWave) && SpawnWaveManager.CurrentSpawnWave.RemainingEnemiesToSpawn.Count == 0)
			{
				num = ((SpawnWaveManager.CurrentSpawnWave.RemainingEliteEnemiesToSpawn.Count == 0) ? 1 : 0);
				goto IL_004d;
			}
		}
		num = 0;
		goto IL_004d;
		IL_004d:
		bool flag = (byte)num != 0;
		if (!GameController.LockEndTurn && TPSingleton<EnemyUnitManager>.Instance.ComputedEnemyUnitsCount == 0 && (SpawnWaveManager.CurrentSpawnWave == null || flag))
		{
			SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
			return currentSpawnWave == null || !currentSpawnWave.SpawnWaveDefinition.IsBossWave || bossWaveVanquished;
		}
		return false;
	}

	public static void ForceStopTurnExecution()
	{
		TPSingleton<NightTurnsManager>.Instance?.StopAllCoroutines();
		TPSingleton<BossManager>.Instance?.StopAllCoroutines();
		TPSingleton<EnemyUnitManager>.Instance?.StopAllCoroutines();
		TPSingleton<TrapManager>.Instance?.StopAllCoroutines();
		TPSingleton<TurretManager>.Instance?.StopAllCoroutines();
		TPSingleton<BonePileManager>.Instance?.StopAllCoroutines();
	}

	public static void StartTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night || TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.EnemyUnits)
		{
			return;
		}
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		if (currentSpawnWave != null && currentSpawnWave.SpawnWaveDefinition.IsBossWave)
		{
			if (TPSingleton<BossManager>.Instance.IsBossVanquished)
			{
				return;
			}
			if (SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.IsInfinite && SpawnWaveManager.CurrentSpawnWave.CurrentCustomNightHour >= SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.Duration)
			{
				SpawnWaveManager.CurrentSpawnWave.CurrentCustomNightHour = 0;
				SpawnWaveManager.CurrentSpawnWave.SpawnWaveController.PopulateSpawnWave();
			}
		}
		TPSingleton<NightTurnsManager>.Instance.StartCoroutine(TPSingleton<NightTurnsManager>.Instance.PlayTurn());
	}

	public IEnumerator EndNightCoroutine()
	{
		IsEndingNight = true;
		yield return TPSingleton<BossManager>.Instance.HandleEndNightCleaning();
		yield return TPSingleton<BuildingManager>.Instance.ClearRandomBuildings();
		GameController.EndTurn();
		IsEndingNight = false;
	}

	public IEnumerator PlayTurn()
	{
		Log("PlayTurn...", CLogLevel.MAJOR);
		if (disableAI)
		{
			Log("AI Disabled, skipping turn");
			GameController.EndTurn();
			yield return null;
		}
		yield return TPSingleton<EnemyUnitManager>.Instance.WaitUntilDeathRattlingEnemiesAreDone;
		if (SpawnWaveManager.CurrentSpawnWave.CurrentCustomNightHour == 0)
		{
			TPSingleton<EnemyUnitManager>.Instance.ResetSpawnedGuardians();
			yield return TPSingleton<BuildingManager>.Instance.LitBraziers();
		}
		if (SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.IsBossWave)
		{
			yield return TPSingleton<BossManager>.Instance.HandleBossPhases();
			if (TPSingleton<EnemyUnitManager>.Instance.EnemiesExecutingSkillsOnSpawn.Count > 0)
			{
				yield return TPSingleton<EnemyUnitManager>.Instance.WaitUntilEnemiesExecutingSkillsOnSpawnAreDone;
			}
		}
		yield return SpawnEnemies();
		if (TPSingleton<EnemyUnitManager>.Instance.EnemiesExecutingSkillsOnSpawn.Count > 0)
		{
			yield return TPSingleton<EnemyUnitManager>.Instance.WaitUntilEnemiesExecutingSkillsOnSpawnAreDone;
		}
		if (SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.IsBossWave)
		{
			yield return PlayBossesMove();
			TPSingleton<BossManager>.Instance.TriggerBossesAffix(E_EffectTime.OnMovementEnd);
			yield return PlayBossesGoals();
			yield return PlayEnergeticBossesTurn();
			TPSingleton<BossManager>.Instance.TriggerBossesAffix(E_EffectTime.OnEndNightTurnEnemy);
		}
		yield return PlayBonePilesGoals();
		yield return PlayEnemiesMove();
		TPSingleton<EnemyUnitManager>.Instance.TriggerEnemiesAffix(E_EffectTime.OnMovementEnd);
		yield return EffectTimeEventManager.InvokeEventOnAllPlayable(E_EffectTime.OnEnemyMovementEnd);
		ACameraView.AllowUserPan = false;
		yield return PlayTrapsGoals();
		yield return PlayTurretsGoals();
		yield return PlayEnemiesGoals();
		yield return PlayEnergeticEnemiesTurn();
		TPSingleton<EnemyUnitManager>.Instance.TriggerEnemiesAffix(E_EffectTime.OnEndNightTurnEnemy);
		if (!TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying)
		{
			yield return SharedYields.WaitForSeconds(WaitAfterTurnEndDuration);
		}
		Log("...ALL DONE", CLogLevel.MAJOR);
		GameController.EndTurn();
	}

	private IEnumerator SpawnEnemies()
	{
		Log("      SpawnEnemies...", CLogLevel.MAJOR);
		yield return SpawnWaveManager.CurrentSpawnWave.SpawnWaveController.SpawnEnemies();
		yield return SharedYields.WaitForSeconds(WaitAfterSpawnDuration);
		Log("                      ...DONE", CLogLevel.MAJOR);
	}

	public void StopCoroutinesExecution()
	{
		TPSingleton<NightTurnsManager>.Instance.StopAllCoroutines();
		TPSingleton<TurretManager>.Instance.StopAllCoroutines();
		TPSingleton<TrapManager>.Instance.StopAllCoroutines();
	}

	private IEnumerator PlayBossesMove()
	{
		Log("      MoveBossUnits...", CLogLevel.MAJOR);
		yield return TPSingleton<BossManager>.Instance.MoveUnitsCoroutine(TPSingleton<BossManager>.Instance.BehaviorModels);
		yield return SharedYields.WaitForSeconds(WaitAfterMoveDuration);
		Log("                      ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayEnemiesMove()
	{
		Log("      PlayEnemiesMove...", CLogLevel.MAJOR);
		if (TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count > 0)
		{
			yield return TPSingleton<EnemyUnitManager>.Instance.MoveUnitsCoroutine(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits);
			yield return SharedYields.WaitForSeconds(WaitAfterMoveDuration);
		}
		Log("                        ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayEnergeticBossMove(BossUnit energeticBoss)
	{
		Log("      PlayEnergeticBossMove...");
		yield return TPSingleton<BossManager>.Instance.MoveUnitsCoroutine(new List<IBehaviorModel>(1) { energeticBoss }, moveCamera: true);
		yield return SharedYields.WaitForSeconds(WaitAfterMoveDuration);
		Log("                               ...DONE");
	}

	private IEnumerator PlayEnergeticEnemyMove(EnemyUnit energeticEnemy)
	{
		Log("      PlayEnergeticEnemyMove...");
		yield return TPSingleton<EnemyUnitManager>.Instance.MoveUnitsCoroutine(new List<EnemyUnit>(1) { energeticEnemy }, moveCamera: true);
		yield return SharedYields.WaitForSeconds(WaitAfterMoveDuration);
		Log("                               ...DONE");
	}

	private IEnumerator PlayBonePilesGoals()
	{
		Log("      PlayBonePilesGoals...", CLogLevel.MAJOR);
		if (TPSingleton<BonePileManager>.Instance.BonePiles.Count > 0)
		{
			yield return TPSingleton<BonePileManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine();
			yield return SharedYields.WaitForSeconds(WaitAfterBonePilesDuration);
		}
		Log("                         ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayBossesGoals()
	{
		Log("      PlayBossUnitsGoals...", CLogLevel.MAJOR);
		yield return TPSingleton<BossManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine();
		yield return SharedYields.WaitForSeconds(WaitAfterEnemiesDuration);
		Log("                           ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayEnemiesGoals()
	{
		Log("      PlayEnemiesGoals...", CLogLevel.MAJOR);
		if (TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count > 0)
		{
			yield return TPSingleton<EnemyUnitManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine();
			yield return SharedYields.WaitForSeconds(WaitAfterEnemiesDuration);
		}
		Log("                         ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayEnergeticBossGoals(BossUnit energeticBoss)
	{
		energeticBoss.EnemyUnitController.DecrementGoalsCooldown();
		yield return TPSingleton<BossManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine(new List<IBehaviorModel>(1) { energeticBoss });
		yield return SharedYields.WaitForSeconds(WaitAfterEnemiesDuration);
	}

	private IEnumerator PlayEnergeticEnemyGoals(EnemyUnit energeticEnemy)
	{
		energeticEnemy.EnemyUnitController.DecrementGoalsCooldown();
		yield return TPSingleton<EnemyUnitManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine(new List<IBehaviorModel>(1) { energeticEnemy });
		yield return SharedYields.WaitForSeconds(WaitAfterEnemiesDuration);
	}

	private IEnumerator PlayTrapsGoals()
	{
		Log("      PlayTrapsGoals...", CLogLevel.MAJOR);
		if (TPSingleton<TrapManager>.Instance.Traps.Count > 0)
		{
			yield return TPSingleton<TrapManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine();
			yield return SharedYields.WaitForSeconds(WaitAfterTrapsDuration);
		}
		Log("                       ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayTrapsUnderEnergeticEnemyGoals(EnemyUnit energeticEnemy)
	{
		List<BattleModule> list = (from tile in energeticEnemy.OccupiedTiles
			where tile.Building?.IsTrap ?? false
			select tile.Building.BattleModule).ToList();
		if (list.Count == 0)
		{
			Log("      Skip PlayTrapsUnderEnergeticEnemyGoals. No traps under this energetic enemy (\"" + energeticEnemy.UniqueIdentifier + "\").");
			yield break;
		}
		Log("      PlayTrapsUnderEnergeticEliteEnemiesGoals...");
		foreach (BattleModule item in list)
		{
			item.BattleModuleController.DecrementGoalsCooldown();
		}
		yield return TPSingleton<TrapManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine(((IEnumerable<IBehaviorModel>)list).ToList());
		yield return SharedYields.WaitForSeconds(WaitAfterTrapsDuration);
		Log("                                                 ...DONE");
	}

	private IEnumerator PlayTurretsGoals()
	{
		Log("      PlayTurretsGoals...", CLogLevel.MAJOR);
		if (TPSingleton<TurretManager>.Instance.Turrets.Count > 0)
		{
			yield return TPSingleton<TurretManager>.Instance.ExecuteBehaviorModelsSkillsCoroutine();
			yield return SharedYields.WaitForSeconds(WaitAfterTurretsDuration);
		}
		Log("                         ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayEnergeticBossesTurn()
	{
		List<IBehaviorModel> sortedEnergeticBosses = TPSingleton<BossManager>.Instance.SortBehaviors(((IEnumerable<IBehaviorModel>)TPSingleton<BossManager>.Instance.BossUnits.Where((BossUnit x) => x.Affixes.Any((EnemyAffix affix) => affix.EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect == EnemyAffixEffectDefinition.E_EnemyAffixEffect.Energetic))).ToList());
		if (sortedEnergeticBosses.Count == 0)
		{
			Log("      Skip PlayEnergeticBossesTurn. No energetic bosses found.", CLogLevel.MAJOR);
			yield break;
		}
		Log("      PlayEnergeticEliteBossesTurn...", CLogLevel.MAJOR);
		int i = 0;
		while (i < sortedEnergeticBosses.Count)
		{
			yield return PlayEnergeticBossTurn(sortedEnergeticBosses[i] as BossUnit);
			int num = i + 1;
			i = num;
		}
		Log("                                      ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayEnergeticEnemiesTurn()
	{
		List<IBehaviorModel> sortedEnergeticEnemies = TPSingleton<EnemyUnitManager>.Instance.SortBehaviors(((IEnumerable<IBehaviorModel>)TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Where((EnemyUnit x) => x.Affixes.Any((EnemyAffix affix) => affix.EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect == EnemyAffixEffectDefinition.E_EnemyAffixEffect.Energetic))).ToList());
		if (sortedEnergeticEnemies.Count == 0)
		{
			Log("      Skip PlayEnergeticEnemiesTurn. No energetic enemies found.", CLogLevel.MAJOR);
			yield break;
		}
		Log("      PlayEnergeticEliteEnemiesTurn...", CLogLevel.MAJOR);
		int i = 0;
		while (i < sortedEnergeticEnemies.Count)
		{
			yield return PlayEnergeticEnemyTurn(sortedEnergeticEnemies[i] as EnemyUnit);
			int num = i + 1;
			i = num;
		}
		Log("                                      ...DONE", CLogLevel.MAJOR);
	}

	private IEnumerator PlayEnergeticBossTurn(BossUnit energeticBoss)
	{
		yield return PlayEnergeticBossMove(energeticBoss);
		yield return PlayTrapsUnderEnergeticEnemyGoals(energeticBoss);
		yield return PlayEnergeticBossGoals(energeticBoss);
	}

	private IEnumerator PlayEnergeticEnemyTurn(EnemyUnit energeticEnemy)
	{
		yield return PlayEnergeticEnemyMove(energeticEnemy);
		yield return PlayTrapsUnderEnergeticEnemyGoals(energeticEnemy);
		yield return PlayEnergeticEnemyGoals(energeticEnemy);
	}

	[DevConsoleCommand(Name = "ForceEndEnemyTurn")]
	public static void Debug_ForceEndEnemyTurn()
	{
		TPSingleton<NightTurnsManager>.Instance.StopAllCoroutines();
		GameController.EndTurn();
	}

	[DevConsoleCommand(Name = "PlayTrapsGoals")]
	public static void Debug_PlayTrapsGoals()
	{
		TrapManager.Debug_TrapsResetTurn();
		TPSingleton<NightTurnsManager>.Instance.StartCoroutine(TPSingleton<NightTurnsManager>.Instance.PlayTrapsGoals());
	}

	[DevConsoleCommand(Name = "PlayTurretsGoals")]
	public static void Debug_PlayTurretsGoals()
	{
		TurretManager.Debug_TurretsResetTurn();
		TPSingleton<NightTurnsManager>.Instance.StartCoroutine(TPSingleton<NightTurnsManager>.Instance.PlayTurretsGoals());
	}
}
