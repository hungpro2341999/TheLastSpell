using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Model.Unit.Boss;

public class BossPhase : FormulaInterpreterContext, ISerializable, IDeserializable
{
	public int PhaseStartedAtTurn { get; private set; } = -1;

	public Dictionary<string, BossPhaseHandler> BossPhaseHandlers { get; } = new Dictionary<string, BossPhaseHandler>();

	public List<ABossPhaseConditionController> VictoryConditionsControllers { get; }

	public List<ABossPhaseConditionController> DefeatConditionsControllers { get; }

	public bool ShouldTriggerVictory => VictoryConditionsControllers?.All((ABossPhaseConditionController x) => x.IsValid()) ?? false;

	public bool ShouldTriggerDefeat => DefeatConditionsControllers?.All((ABossPhaseConditionController x) => x.IsValid()) ?? false;

	public bool VictoryConditionIsToFinishWave
	{
		get
		{
			if (VictoryConditionsControllers != null && VictoryConditionsControllers.Count == 1)
			{
				return VictoryConditionsControllers[0] is FinishWaveBossPhaseConditionController;
			}
			return false;
		}
	}

	public BossPhaseDefinition BossPhaseDefinition { get; }

	public BossPhase(BossPhaseDefinition bossPhaseDefinition)
	{
		BossPhaseDefinition = bossPhaseDefinition;
		foreach (BossPhaseHandlerDefinition bossPhaseHandlerDefinition in bossPhaseDefinition.BossPhaseHandlerDefinitions)
		{
			BossPhaseHandlers.Add(bossPhaseHandlerDefinition.Id, new BossPhaseHandler(bossPhaseHandlerDefinition, BossPhaseDefinition.Id));
		}
		if (bossPhaseDefinition.VictoryConditionsDefinitions.Count > 0)
		{
			VictoryConditionsControllers = new List<ABossPhaseConditionController>(bossPhaseDefinition.VictoryConditionsDefinitions.Count);
			foreach (IBossPhaseConditionDefinition victoryConditionsDefinition in bossPhaseDefinition.VictoryConditionsDefinitions)
			{
				if (BossPhaseConditionsFactory.BossPhaseConditionControllerFromDefinition(victoryConditionsDefinition, out var bossPhaseConditionController))
				{
					VictoryConditionsControllers.Add(bossPhaseConditionController);
				}
			}
		}
		if (bossPhaseDefinition.DefeatConditionsDefinitions.Count <= 0)
		{
			return;
		}
		DefeatConditionsControllers = new List<ABossPhaseConditionController>(bossPhaseDefinition.DefeatConditionsDefinitions.Count);
		foreach (IBossPhaseConditionDefinition defeatConditionsDefinition in bossPhaseDefinition.DefeatConditionsDefinitions)
		{
			if (BossPhaseConditionsFactory.BossPhaseConditionControllerFromDefinition(defeatConditionsDefinition, out var bossPhaseConditionController2))
			{
				DefeatConditionsControllers.Add(bossPhaseConditionController2);
			}
		}
	}

	public void Init()
	{
		PhaseStartedAtTurn = Mathf.Max(1, TPSingleton<GameManager>.Instance.Game.CurrentNightHour);
		foreach (BossPhaseHandler value in BossPhaseHandlers.Values)
		{
			value.Init();
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedBossPhase serializedBossPhase = (SerializedBossPhase)container;
		PhaseStartedAtTurn = serializedBossPhase.PhaseStartedAtTurn;
		int i;
		for (i = 0; i < serializedBossPhase.BossPhaseHandlers.Count; i++)
		{
			BossPhaseHandlers.Values.FirstOrDefault((BossPhaseHandler x) => x.BossPhaseHandlerDefinition.Id == serializedBossPhase.BossPhaseHandlers[i].Id)?.Deserialize(serializedBossPhase.BossPhaseHandlers[i], saveVersion);
		}
	}

	public ISerializedData Serialize()
	{
		List<SerializedBossPhaseHandler> list = new List<SerializedBossPhaseHandler>();
		foreach (BossPhaseHandler value in BossPhaseHandlers.Values)
		{
			list.Add((SerializedBossPhaseHandler)value.Serialize());
		}
		return new SerializedBossPhase
		{
			PhaseStartedAtTurn = PhaseStartedAtTurn,
			BossPhaseHandlers = list
		};
	}
}
