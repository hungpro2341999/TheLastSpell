using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class BloodyKilledAfterEatingAlliesTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "BloodyKilledAfterEatingAllies";

	public BloodyKilledAfterEatingAlliesTrophyDefinition BloodyKilledAfterEatingAlliesTrophyDefinition => base.TrophyConditionDefinition as BloodyKilledAfterEatingAlliesTrophyDefinition;

	public BloodyKilledAfterEatingAlliesTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type BloodyKilledAfterEatingAllies need only 2 argument", CLogLevel.DETAILED);
		}
		else if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type BloodyKilledAfterEatingAllies should have as first argument an int!", CLogLevel.DETAILED);
		}
		else if (!(args[1] is EnemyUnit enemyUnit))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type BloodyKilledAfterEatingAllies should have as second argument a Model.Unit.Enemy.EnemyUnit !", CLogLevel.DETAILED);
		}
		else if (enemyUnit.EnemyUnitController.EnemyUnit.AlliesKilled >= BloodyKilledAfterEatingAlliesTrophyDefinition.Value)
		{
			if (!base.ProgressionPerUnitId.ContainsKey(key))
			{
				base.ProgressionPerUnitId.Add(key, 1);
			}
			else
			{
				base.ProgressionPerUnitId[key]++;
			}
			OnValueChange();
		}
	}

	protected override void CheckCompleteState()
	{
		if (isCompleted)
		{
			return;
		}
		switch (BloodyKilledAfterEatingAlliesTrophyDefinition.Target)
		{
		case "All":
		{
			int num = 0;
			foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
			{
				num += item.Value;
			}
			isCompleted = num > 0;
			break;
		}
		case "One":
		{
			foreach (KeyValuePair<int, int> item2 in base.ProgressionPerUnitId)
			{
				if (item2.Value > 0)
				{
					isCompleted = true;
					break;
				}
			}
			break;
		}
		default:
			isCompleted = false;
			break;
		}
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type BloodyKilledAfterEatingAllies needs 2 arguments", CLogLevel.DETAILED);
		}
		else if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type BloodyKilledAfterEatingAllies should have as first argument a string!", CLogLevel.DETAILED);
		}
		else if (!(args[1] is EnemyUnit enemyUnit))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type BloodyKilledAfterEatingAllies should have as second argument a Model.Unit.Enemy.EnemyUnit !", CLogLevel.DETAILED);
		}
		else if (enemyUnit.EnemyUnitController.EnemyUnit.AlliesKilled >= BloodyKilledAfterEatingAlliesTrophyDefinition.Value)
		{
			if (!base.ProgressionPerUnitId.ContainsKey(key))
			{
				base.ProgressionPerUnitId.Add(key, 1);
			}
			else
			{
				base.ProgressionPerUnitId[key] = 1;
			}
			OnValueChange();
		}
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (BloodyKilledAfterEatingAlliesTrophyDefinition.Target == "All")
		{
			text += $"    {GetTotal()} hero(es) has killed at least a Bloody who killed {BloodyKilledAfterEatingAlliesTrophyDefinition.Value} Clawers. \r\n";
		}
		foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
		{
			text += $"    {GetUnitName(item.Key)} killed {item.Value} Bloody(ies) who killed {BloodyKilledAfterEatingAlliesTrophyDefinition.Value} Clawers. \r\n";
		}
		return text;
	}
}
