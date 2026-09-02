using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Apocalypse.ApocalypseEffects;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Stat;

namespace TheLastStand.Controller.Apocalypse.ApocalypseEffects;

public class AddEnemiesStatModifierFromTurnApocalypseEffectController : AApocalypseEffectController
{
	public AddEnemiesStatModifierFromTurnApocalypseEffect StatModifierEffect => base.AApocalypseEffect as AddEnemiesStatModifierFromTurnApocalypseEffect;

	public AddEnemiesStatModifierFromTurnApocalypseEffectController(AddEnemiesStatModifierFromTurnApocalypseEffectDefinition effectDefinition)
		: base(effectDefinition)
	{
	}

	public void Activate(bool onLoad)
	{
		if (!StatModifierEffect.IsActive)
		{
			StatModifierEffect.IsActive = true;
			OnActivation(onLoad);
		}
	}

	public void CheckIfCanActivate(bool onLoad)
	{
		int num = 0;
		if (TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game != null)
		{
			num = TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
		}
		if (StatModifierEffect.CanActivateAtTurn(num))
		{
			TPSingleton<ApocalypseManager>.Instance.Log($"++ Unlocking apocalypse effect AddEnemiesStatModifierFromTurn at turn: {num}", CLogLevel.NORMAL, forcePrintInUnity: true);
			UnHookActivationConditions();
			Activate(onLoad);
		}
	}

	public void DeActivate(bool onLoad, bool hookActivationConditions = false)
	{
		if (StatModifierEffect.IsActive)
		{
			StatModifierEffect.IsActive = false;
			UnHookActivationConditions();
			OnDeactivation(onLoad);
			if (hookActivationConditions)
			{
				HookActivationConditions();
			}
		}
	}

	public void HookActivationConditions()
	{
		if (!StatModifierEffect.IsHookedForActivation)
		{
			ApocalypseManager.OnStatModifierFromTurnCheck += CheckIfCanActivate;
			StatModifierEffect.IsHookedForActivation = true;
		}
	}

	public void UnHookActivationConditions()
	{
		if (StatModifierEffect.IsHookedForActivation)
		{
			ApocalypseManager.OnStatModifierFromTurnCheck -= CheckIfCanActivate;
			StatModifierEffect.IsHookedForActivation = false;
		}
	}

	public void HookDeactivationConditions()
	{
		if (!StatModifierEffect.IsHookedForDeactivation)
		{
			ApocalypseManager.OnDeactivateModifiersWithTurnConditions += DeactivateAfterConditionsMet;
			StatModifierEffect.IsHookedForDeactivation = true;
		}
	}

	public void UnHookDeactivationConditions()
	{
		if (StatModifierEffect.IsHookedForDeactivation)
		{
			ApocalypseManager.OnDeactivateModifiersWithTurnConditions -= DeactivateAfterConditionsMet;
			StatModifierEffect.IsHookedForDeactivation = false;
		}
	}

	protected override AApocalypseEffect CreateModel(ApocalypseEffectDefinition effectDefinition)
	{
		return new AddEnemiesStatModifierFromTurnApocalypseEffect(effectDefinition as AddEnemiesStatModifierFromTurnApocalypseEffectDefinition, this);
	}

	protected override void OnActivation(bool onLoad)
	{
		base.OnActivation(onLoad);
		foreach (Tuple<UnitStatDefinition.E_Stat, int> statModifier in StatModifierEffect.StatModifierEffectDefinition.StatModifiers)
		{
			if (!ApocalypseManager.CurrentApocalypse.ActiveStatModifierFromTurn.ContainsKey(statModifier.Item1))
			{
				ApocalypseManager.CurrentApocalypse.ActiveStatModifierFromTurn.Add(statModifier.Item1, statModifier.Item2);
			}
			else
			{
				ApocalypseManager.CurrentApocalypse.ActiveStatModifierFromTurn[statModifier.Item1] += statModifier.Item2;
			}
			if (!ApplicationManager.Application.IsGameState)
			{
				continue;
			}
			List<EnemyUnit> list = new List<EnemyUnit>(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
			list.AddRange(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits);
			list.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
			foreach (EnemyUnit item in list)
			{
				EnemyUnitStat stat = item.EnemyUnitStatsController.GetStat(statModifier.Item1);
				if (stat != null)
				{
					stat.Apocalypse += statModifier.Item2;
					if (!onLoad && stat.ChildStat != null)
					{
						item.EnemyUnitStatsController.IncreaseBaseStat(stat.ChildStat.StatId, statModifier.Item2, includeChildStat: false);
					}
				}
			}
		}
	}

	protected override void OnDeactivation(bool onLoad)
	{
		base.OnDeactivation(onLoad);
		foreach (Tuple<UnitStatDefinition.E_Stat, int> statModifier in StatModifierEffect.StatModifierEffectDefinition.StatModifiers)
		{
			if (ApocalypseManager.CurrentApocalypse.ActiveStatModifierFromTurn.ContainsKey(statModifier.Item1))
			{
				ApocalypseManager.CurrentApocalypse.ActiveStatModifierFromTurn[statModifier.Item1] -= statModifier.Item2;
			}
			if (!ApplicationManager.Application.IsGameState)
			{
				continue;
			}
			List<EnemyUnit> list = new List<EnemyUnit>(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
			list.AddRange(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits);
			list.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
			foreach (EnemyUnit item in list)
			{
				EnemyUnitStat stat = item.EnemyUnitStatsController.GetStat(statModifier.Item1);
				if (stat != null)
				{
					stat.Apocalypse -= statModifier.Item2;
					if (!onLoad && stat.ChildStat != null)
					{
						item.EnemyUnitStatsController.DecreaseBaseStat(stat.ChildStat.StatId, statModifier.Item2, includeChildStat: false);
					}
				}
			}
		}
	}

	private void CheckIfCanActivate()
	{
		CheckIfCanActivate(onLoad: false);
	}

	private void DeactivateAfterConditionsMet()
	{
		DeActivate(onLoad: false, hookActivationConditions: true);
	}
}
