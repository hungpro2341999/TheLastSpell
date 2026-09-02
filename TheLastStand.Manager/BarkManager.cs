using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Database;
using TheLastStand.Definition;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.Manager;

public sealed class BarkManager : Manager<BarkManager>
{
	[SerializeField]
	private float delayBetweenBarks = 0.5f;

	[SerializeField]
	private float delayPostAttack = 0.8f;

	[SerializeField]
	private float delayPostAttackFriendlyFirePerpetrator = 0.9f;

	[SerializeField]
	private float delayPostPlayableUnitDeath = 1.2f;

	[SerializeField]
	private float delayPostNewCycleAndEnemiesSpawn;

	[SerializeField]
	private int maximumQueueSize = 4;

	[SerializeField]
	private BarkView barkViewPrefab;

	[SerializeField]
	private Transform barksParent;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Percentage of enemy wave spawn to trigger the bark")]
	[FormerlySerializedAs("bigEnemyUnitSpawnPercentage")]
	private float manyEnemyUnitSpawnPercentage = 0.15f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Percentage of building remaining health to trigger the bark")]
	private float buildingCriticalHealthPercentage = 0.25f;

	[SerializeField]
	[Range(0f, 100f)]
	[Tooltip("How many kills the player have to execute during one turn to trigger the bark")]
	private float killsNeededDuringOneTurn = 20f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("How much health a playable unit has to lose during the enemy turn to trigger the bark")]
	private float bigAmountOfHealthLostPercentage = 0.25f;

	[SerializeField]
	[Range(0f, 20f)]
	[Tooltip("Below this amount of mana a playable unit will bark")]
	private int criticalManaCount = 8;

	private List<Bark> barksBeingDisplayed = new List<Bark>();

	private Queue<Bark> barksInPreparation = new Queue<Bark>();

	private Queue<Bark> barksReadyToBeDisplayed = new Queue<Bark>();

	private List<Bark> barksWaiting = new List<Bark>();

	public static float BigAmountOfHealthLost => TPSingleton<BarkManager>.Instance.bigAmountOfHealthLostPercentage;

	public static int DisplayedBarksCount { get; private set; }

	public static float CriticalManaCount => TPSingleton<BarkManager>.Instance.criticalManaCount;

	public static int CurrentBarksCount => TPSingleton<BarkManager>.Instance.barksInPreparation.Count + TPSingleton<BarkManager>.Instance.barksWaiting.Count + TPSingleton<BarkManager>.Instance.barksReadyToBeDisplayed.Count + TPSingleton<BarkManager>.Instance.barksBeingDisplayed.Count;

	public static float DelayPostAttack => TPSingleton<BarkManager>.Instance.delayPostAttack;

	public static float DelayPostNewCycleAndEnemiesSpawn => TPSingleton<BarkManager>.Instance.delayPostNewCycleAndEnemiesSpawn;

	public static float ManyEnemyUnitSpawnPercentage => TPSingleton<BarkManager>.Instance.manyEnemyUnitSpawnPercentage;

	public void AddPotentialBark(string barkId, IBarker barker, float waitTime, int sentenceIndex = -1, bool forceSucceed = false, bool ignoreDeathCheck = false)
	{
		if (!BarkDatabase.BarkDefinitions.ContainsKey(barkId))
		{
			LogError("There is not BarkDefinition with the Id " + barkId + " in BarkDefinitions --> Abort adding bark to queue", CLogLevel.MAJOR);
			return;
		}
		if (CurrentBarksCount >= maximumQueueSize)
		{
			LogError($"Can not add {barkId} to queue because it is already full. Max barks = {maximumQueueSize}", CLogLevel.DETAILED, forcePrintInUnity: true, printStackTrace: false);
			return;
		}
		if (barker.HasBark)
		{
			LogError("Can not add " + barkId + " to queue because the target already has a bark", CLogLevel.DETAILED, forcePrintInUnity: true, printStackTrace: false);
			return;
		}
		if (!ignoreDeathCheck && !CheckBarkerIsNotDead(barker))
		{
			LogError("Can not add " + barkId + " to queue because the target is dead", CLogLevel.DETAILED, forcePrintInUnity: true, printStackTrace: false);
			return;
		}
		BarkDefinition barkDefinition = BarkDatabase.BarkDefinitions[barkId];
		if (!forceSucceed)
		{
			int randomRange = RandomManager.GetRandomRange(this, 0, 100);
			Log("Proba to add " + barkId + " --> " + (((float)randomRange > barkDefinition.Proba * 100f) ? "Failed" : "Succeed"), CLogLevel.DETAILED);
			if ((float)randomRange > barkDefinition.Proba * 100f)
			{
				return;
			}
		}
		string text = null;
		if (sentenceIndex != -1)
		{
			text = BarkDefinition.GetSentence(BarkDatabase.BarkDefinitions[barkId], barker, sentenceIndex);
			if (text == null)
			{
				LogError($"Can not find sentence {sentenceIndex} in {barkId} sentences", CLogLevel.MAJOR);
				return;
			}
		}
		BarkView pooledComponent = ObjectPooler.GetPooledComponent("BarkViews", barkViewPrefab, barksParent);
		Bark bark = new BarkController(BarkDatabase.BarkDefinitions[barkId], pooledComponent, barker).Bark;
		if (text != null)
		{
			bark.Sentence = text;
		}
		barker.HasBark = true;
		bark.Barker = barker;
		bark.WaitTime = waitTime;
		bark.IgnoreDeathCheck = ignoreDeathCheck;
		barksInPreparation.Enqueue(bark);
	}

	public void AddPlayableUnitKilledBarks()
	{
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			if (!TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].IsDead)
			{
				TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitDeath", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i], delayPostPlayableUnitDeath);
			}
		}
	}

	public void CheckAttack(IBarker attackedObject, AttackSkillActionExecutionTileData attackData, ISkillCaster attacker)
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CutscenePlaying || CutsceneManager.AnyCutscenePlaying)
		{
			return;
		}
		if (attackedObject is PlayableUnit && attackData.TargetRemainingHealth > 0f)
		{
			if (attacker is PlayableUnit)
			{
				TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitHitByPlayableUnit", attackedObject, delayPostAttack);
			}
			else
			{
				TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitHit", attackedObject, delayPostAttack);
			}
		}
		if (attackedObject is BlueprintModule blueprintModule && blueprintModule.BuildingParent.ProductionModule?.BuildingGaugeEffect != null && attackData.TargetRemainingHealth > 0f)
		{
			if (attackData.TargetRemainingHealth + attackData.HealthDamage > blueprintModule.BuildingParent.DamageableModule.HealthTotal * buildingCriticalHealthPercentage && attackData.TargetRemainingHealth <= blueprintModule.BuildingParent.DamageableModule.HealthTotal * buildingCriticalHealthPercentage)
			{
				TPSingleton<BarkManager>.Instance.AddPotentialBark("BuildingHasCriticalHealth", attackedObject, delayPostAttack);
			}
			TPSingleton<BarkManager>.Instance.AddPotentialBark("BuildingHit", attackedObject, delayPostAttack);
		}
		if (!(attacker is PlayableUnit barker))
		{
			return;
		}
		if (attackedObject is PlayableUnit)
		{
			TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitAttackPlayableUnit", barker, delayPostAttackFriendlyFirePerpetrator);
		}
		if (attackData.IsCrit)
		{
			TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitCriticAttack", barker, delayPostAttack);
		}
		if (!(attackedObject is EnemyUnit) || !(attackData.TargetRemainingHealth <= 0f))
		{
			return;
		}
		TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitKillAnEnemy", barker, delayPostAttack);
		if ((float)TPSingleton<TrophyManager>.Instance.EnemiesKilledThisTurn > killsNeededDuringOneTurn)
		{
			for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
			{
				TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitsHaveKilledALot", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i], delayPostAttack);
			}
		}
	}

	public void CheckNewCycle(Game.E_Cycle newCycle)
	{
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<BarkManager>.Instance.AddPotentialBark((newCycle == Game.E_Cycle.Day) ? "DayCycleBeginning" : "NightCycleBeginning", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i], delayPostNewCycleAndEnemiesSpawn);
		}
		Display();
	}

	public void CheckPlayableUnitOutOfResources(PlayableUnit playableUnit, TheLastStand.Model.Skill.Skill skill)
	{
		if (skill.ManaCost > 0 && playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana) <= CriticalManaCount)
		{
			TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitsAlmostOutOfMana", playableUnit, delayPostAttack);
		}
	}

	public void CheckDawnStart()
	{
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			AddPotentialBark("DawnStart", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i], 0f);
		}
		Display();
	}

	public void Display()
	{
		for (int num = barksInPreparation.Count - 1; num >= 0; num--)
		{
			Bark bark = barksInPreparation.Dequeue();
			if (bark.Barker != null && CheckBarkerIsNotDead(bark))
			{
				barksWaiting.Add(bark);
				StartCoroutine(BarkWait(barksWaiting[barksWaiting.Count - 1]));
			}
			else
			{
				DestroyBarkWithoutBarker(bark);
			}
		}
	}

	public void RemoveBark(Bark barkToRemove)
	{
		barkToRemove.Barker.HasBark = false;
		barksBeingDisplayed.Remove(barkToRemove);
		barkToRemove.BarkView.gameObject.SetActive(value: false);
	}

	private IEnumerator BarkWait(Bark bark)
	{
		yield return SharedYields.WaitForSeconds(bark.WaitTime);
		barksWaiting.Remove(bark);
		if (bark.Barker != null && CheckBarkerIsNotDead(bark))
		{
			barksReadyToBeDisplayed.Enqueue(bark);
		}
		else
		{
			DestroyBarkWithoutBarker(bark);
		}
	}

	private bool CheckBarkerIsNotDead(IBarker barker)
	{
		if (barker is IDamageable damageable)
		{
			return !damageable.IsDead;
		}
		return true;
	}

	private bool CheckBarkerIsNotDead(Bark bark)
	{
		if (!bark.IgnoreDeathCheck)
		{
			return CheckBarkerIsNotDead(bark.Barker);
		}
		return true;
	}

	private IEnumerator DisplayCoroutine()
	{
		while (true)
		{
			if (barksReadyToBeDisplayed.Count > 0)
			{
				Bark bark = barksReadyToBeDisplayed.Dequeue();
				if (bark.Barker != null && CheckBarkerIsNotDead(bark))
				{
					DisplayedBarksCount++;
					bark.BarkView.Display(delegate
					{
						DisplayedBarksCount--;
					});
					barksBeingDisplayed.Add(bark);
				}
				else
				{
					DestroyBarkWithoutBarker(bark);
				}
				yield return SharedYields.WaitForSeconds(delayBetweenBarks);
			}
			yield return null;
		}
	}

	private void Start()
	{
		StartCoroutine(DisplayCoroutine());
	}

	private void DestroyBarkWithoutBarker(Bark bark)
	{
		Log("Remove bark beacause its barker is null or dead", CLogLevel.DETAILED);
		bark.BarkView.gameObject.SetActive(value: false);
	}

	[DevConsoleCommand("PlayRandomBarks")]
	public static void DebugAddBark(int barksCount = 1)
	{
		for (int i = 0; i < barksCount; i++)
		{
			List<string> list = new List<string>(BarkDatabase.BarkDefinitions.Keys);
			TPSingleton<BarkManager>.Instance.AddPotentialBark(list[RandomManager.GetRandomRange(TPSingleton<BarkManager>.Instance, 0, list.Count)], RandomManager.GetRandomElement("DEBUG", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits), 0f, -1, forceSucceed: true);
		}
		TPSingleton<BarkManager>.Instance.Display();
	}

	[DevConsoleCommand("PlayBark")]
	public static void DebugAddBark([StringConverter(typeof(StringToBarkIdConverter))] string barkId, int sentenceIndex = -1, [StringConverter(typeof(PlayableUnit.StringToRaceIdConverter))] string raceId = "")
	{
		if (string.IsNullOrEmpty(raceId))
		{
			TPSingleton<BarkManager>.Instance.AddPotentialBark(barkId, RandomManager.GetRandomElement("DEBUG", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits), 0f, sentenceIndex, forceSucceed: true);
		}
		else
		{
			List<PlayableUnit> list = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.FindAll((PlayableUnit playableUnit) => playableUnit.RaceDefinition.Id == raceId);
			if (list.Count == 0)
			{
				TPSingleton<BarkManager>.Instance.LogError("Couldn't find a playable unit with race " + raceId + " to Play a bark !");
				return;
			}
			TPSingleton<BarkManager>.Instance.AddPotentialBark(barkId, RandomManager.GetRandomElement("DEBUG", list), 0f, sentenceIndex, forceSucceed: true);
		}
		TPSingleton<BarkManager>.Instance.Display();
	}
}
