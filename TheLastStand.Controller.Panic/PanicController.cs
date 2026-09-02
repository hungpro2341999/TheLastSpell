using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Panic;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Panic;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Panic;
using UnityEngine;

namespace TheLastStand.Controller.Panic;

public class PanicController
{
	public TheLastStand.Model.Panic.Panic Panic { get; private set; }

	public PanicController(PanicDefinition panicDefinition, PanicView panicView)
	{
		Panic = new TheLastStand.Model.Panic.Panic(panicDefinition, this, panicView);
		Panic.PanicReward = new PanicRewardController(Panic).PanicReward;
	}

	public float AddValue(float value, bool useAttackMultiplier = false, bool updateView = true)
	{
		if (useAttackMultiplier)
		{
			value *= Panic.PanicDefinition.PanicAttackMultiplier;
		}
		value *= ApocalypseManager.CurrentApocalypse.PanicGainMultiplier;
		return SetValue(Panic.Value + value, updateView);
	}

	public float ComputeExpectedValue(bool updateView = false)
	{
		float num = 0f;
		List<EnemyUnit> list = new List<EnemyUnit>(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
		list.AddRange(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits);
		list.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			EnemyUnit enemyUnit = list[num2];
			if (!enemyUnit.IsDead && !enemyUnit.IsDeathRattling && !enemyUnit.IsStunned && !enemyUnit.WillDieByPoison && enemyUnit.IsInCity)
			{
				num += GetModifiedExpectedValueToAdd(enemyUnit.EnemyUnitStatsController.EnemyUnitStats.Stats[UnitStatDefinition.E_Stat.Panic].FinalClamped);
			}
		}
		SetExpectedValue(Panic.Value + num, updateView);
		return Panic.ExpectedValue;
	}

	public float RemoveValue(float value)
	{
		return SetValue(Panic.Value - value);
	}

	public void ResetExpectedValue()
	{
		SetExpectedValue(0f);
	}

	public void Reset()
	{
		SetValue(0f);
		ResetExpectedValue();
		Panic.PanicReward.PanicRewardController.Reset();
	}

	public float SetValue(float value, bool updateView = true)
	{
		Panic.Value = Mathf.Min(value, Panic.PanicDefinition.ValueMax);
		if (updateView)
		{
			Panic.PanicView.RefreshPanicValue();
		}
		return Panic.Value;
	}

	private float GetModifiedExpectedValueToAdd(float expectedValueToAdd)
	{
		return expectedValueToAdd * ApocalypseManager.CurrentApocalypse.PanicGainMultiplier;
	}

	private float SetExpectedValue(float value, bool updateView = true)
	{
		Panic.ExpectedValue = Mathf.Min(value, Panic.PanicDefinition.ValueMax);
		if (updateView)
		{
			Panic.PanicView.RefreshPanicExpectedValue();
		}
		return Panic.ExpectedValue;
	}
}
